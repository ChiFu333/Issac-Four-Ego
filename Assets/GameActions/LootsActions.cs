using UnityEngine;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

//Если что-то надо сбросить, например 2 лута, то достаточно 1 сброса, чтобы вызвать тру

[AddTypeMenu(ActionNames.LootName + "1. GainLootCards")]
[Serializable]
public class GainLootCardsAction : IGameAction
{
    private GameMain config;
    public int count;

    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Player> players = container.ConvertToPlayers();
        UniTask lastTask = UniTask.CompletedTask;
        
        foreach (var player in players)
        {
            DicrementalDelayTimer delayTimer = new DicrementalDelayTimer(150, 30, 0.9f);
            for (int i = 0; i < count; i++)
            {
                lastTask = player.AddOneLootCard(G.Main.Decks.lootDeck.TakeOneCard(i, G.Main.MainInitSettings.ShowOthersLootCards || G.Main.Players[0] == player));
                await UniTask.Delay(delayTimer.GetDelay());
            }
        }

        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.LootName + "2. DiscardLootCards")]
[Serializable]
public class DiscardLootCardsAction : IGameAction
{
    public int count;

    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Player> players = container.ConvertToPlayers();
        UniTask lastTask = UniTask.CompletedTask;

        foreach (var player in players)
        {
            int realCount = Mathf.Min(count, player.Get<TagBasePlayerData>().hand.GetCount());
            if (realCount == 0 && players.Count == 1) return false;
        
            for (int i = 0; i < realCount; i++)
            {
                Card c = await G.Main.CardSelector.SelectCardByType<Card>(G.Main.Decks.lootStash.transform, false, 
                        card => 
                        card.Get<TagCardType>().cardType == CardType.lootCard && 
                        card.GetMyPlayer() == player
                );
                lastTask = player.RemoveOneLootCard(c);
                await UniTask.Delay(300);
            }
        }
        
        await lastTask;
        return true;
    }
}