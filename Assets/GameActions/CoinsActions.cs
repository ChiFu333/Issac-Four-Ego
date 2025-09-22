using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class DicrementalDelayTimer
{
    private int minDelay = 50;
    private float delayMultiplier = 0.93f;
    private int currentDelay;
    public DicrementalDelayTimer(int initDelay, int min, float multi)
    {
        minDelay = min;
        delayMultiplier = multi;
        currentDelay = initDelay;
    }

    public int GetDelay()
    {
        int result = currentDelay;
        currentDelay = Mathf.Max((int)(currentDelay * delayMultiplier), minDelay);
        return result;
    }
}
public class CoinAction
{
    private static int randomStartIndex;
    protected DicrementalDelayTimer GetDelayTimer()
    {
        return new DicrementalDelayTimer(180, 50, 0.93f);
    }
    protected void HandleCoinChange(Player player, int index, int totalCount, bool isLastCoin, bool muteSound = false)
    {
        bool isGainingCoins = totalCount > 0;
        
        if (index == 0 && !muteSound) randomStartIndex = Random.Range(1, 4) * (isGainingCoins ? 1 : -1);
        
        player.ChangeCoin(isGainingCoins ? 1 : -1);
        if(!muteSound) G.AudioManager.PlaySound(R.Audio.pennypickup, -1 + CalculatePitch(index * (isGainingCoins ? 1 : -1) + randomStartIndex, 0.05f,0.4f));
    
        if (isLastCoin)
        {
            _ = player.statsText.ShowDeltaEffectAsync(
                "¢", 
                totalCount, 
                "¢",
                isGainingCoins ? TextMeshProExtensions.coinColor : TextMeshProExtensions.LoseColor, 
                duration: 1f
            );
        }
    }
    float CalculatePitch(int i, float growthRate = 0.1f, float minPitch = 0.1f) 
    {
        if (i > 0) 
        {
            return 1.0f + (Mathf.Pow(1.0f + growthRate, Mathf.Abs(i)) - 1.0f);
        }
        else if(i < 0)
        {
            return Mathf.Max(minPitch, 1.0f * Mathf.Pow(1.0f - growthRate, Mathf.Abs(i)));
        }

        return 1;
    }
}
[AddTypeMenu(ActionNames.CoinName + "1. GainCoins")]
[Serializable]
public class GainCoins : CoinAction, IGameAction
{
    public int count;
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Player> players = container.ConvertToPlayers();
        UniTask lastTask = UniTask.CompletedTask;

        foreach (var player in players)
        {
            DicrementalDelayTimer delayTimer = GetDelayTimer();
            for (int i = 0; i < count; i++)
            {
                int currentIndex = i;
                lastTask = player.Get<TagBasePlayerData>().characterCard.GetCoinVisual(
                    source.effectCardSource.transform.position, 
                    () => HandleCoinChange(player, currentIndex, count, currentIndex == count - 1)
                );
                await UniTask.Delay(delayTimer.GetDelay());
            }
        }
        
        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.CoinName + "2. LoseCoins")]
[Serializable]
public class LoseCoinsActions : CoinAction, IGameAction
{
    public int count;
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Player> players = container.ConvertToPlayers();
        UniTask lastTask = UniTask.CompletedTask;
        
        foreach (var player in players)
        {
            DicrementalDelayTimer delayTimer = GetDelayTimer();
            int realCount = Mathf.Min(player.Get<TagBasePlayerData>().coins, count);
            if (realCount == 0 && players.Count == 1) return false;

            for (int i = 0; i < realCount; i++)
            {
                int currentIndex = i;
                lastTask = G.Main.Decks.treasureDeck.GetCoinVisual(
                    player.Get<TagBasePlayerData>().characterCard.transform.position,
                    () => HandleCoinChange(player, currentIndex, -count, currentIndex == realCount - 1)
                );
                await UniTask.Delay(delayTimer.GetDelay());
            }
        }

        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.CoinName + "3. PayCoins")]
[Serializable]
public class PayCoinsActions : CoinAction, IGameAction
{
    public int count;
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        Player player = container.ConvertToPlayer();
        DicrementalDelayTimer delayTimer = GetDelayTimer();
        UniTask lastTask = UniTask.CompletedTask;
        
        int realCount = Mathf.Min(player.Get<TagBasePlayerData>().coins, count);
        if (realCount != count) return false;
        
        for (int i = 0; i < realCount; i++)
        {
            int currentIndex = i;
            lastTask = G.Main.Decks.treasureDeck.GetCoinVisual(
                player.Get<TagBasePlayerData>().characterCard.transform.position,
                () => HandleCoinChange(player, currentIndex, -count, currentIndex == realCount - 1)
            );
            await UniTask.Delay(delayTimer.GetDelay());
        }

        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.CoinName + "4. StealCoins")]
[Serializable]
public class StealCoinsAction : CoinAction, IGameAction
{
    public int count;
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        Player sourcePlayer = source.effectPlayerSource;
        Player targetPlayer = container.ConvertToPlayer();
        DicrementalDelayTimer delayTimer = GetDelayTimer();
        UniTask lastTask = UniTask.CompletedTask;

        int realCount = Mathf.Min(targetPlayer.Get<TagBasePlayerData>().coins, count);
        if (realCount == 0) return false;
        for (int i = 0; i < realCount; i++)
        {
            int currentIndex = i;
            lastTask = sourcePlayer.Get<TagBasePlayerData>().characterCard.GetCoinVisual(
                targetPlayer.Get<TagBasePlayerData>().characterCard.transform.position,
                () =>
                {
                    HandleCoinChange(sourcePlayer, currentIndex, count, currentIndex == realCount - 1);
                    HandleCoinChange(targetPlayer, currentIndex, -count, currentIndex == realCount - 1, true);
                });
            await UniTask.Delay(delayTimer.GetDelay());
        }
        
        return true;
    }
}
