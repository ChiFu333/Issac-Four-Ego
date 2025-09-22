using UnityEngine;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

[AddTypeMenu(ActionNames.ItemName + "1. GainTreasures")]
[Serializable]
public class GainTreasuresAction : IGameAction
{
    public int count;
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Player> players = container.ConvertToPlayers();
        UniTask lastTask = UniTask.CompletedTask;
        
        foreach (var player in players)
        {
            DicrementalDelayTimer delayTimer = new DicrementalDelayTimer(250, 30, 0.95f);
            for (int i = 0; i < count; i++)
            {
                lastTask = player.AddItem(G.Main.Decks.treasureDeck.TakeOneCard(i, true));
                G.AudioManager.PlaySound(R.Audio.TreasureGet, 0.2f * i);
                await UniTask.Delay(delayTimer.GetDelay());
            }
        }

        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.ItemName + "2. RechargeItem")]
[Serializable]
public class RechargeItem : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Card> c = container.ConvertToCards();
        UniTask lastTask = UniTask.CompletedTask;
        foreach (var card in c)
        {
            if(card.Is<TagTappable>()) lastTask = card.Get<TagTappable>().Recharge();
            await UniTask.Delay(100);
        }
        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.ItemName + "3. DestroyItem")]
[Serializable]
public class DestroyItem : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Card> c = container.ConvertToCards();
        UniTask lastTask = UniTask.CompletedTask;
        foreach (var card in c)
        {
            card.GetMyPlayer()?.TryRemoveItem(card);
            if(card.Is<TagInShop>()) G.Main.Shop.RemoveMeFromShop(card);
            await card.BombCard();
            lastTask = card.DiscardCard();
            await UniTask.Delay(100);
        }
        await lastTask;
        await G.Main.Shop.RestockSlots();
        return true;
    }
}
[AddTypeMenu(ActionNames.ItemName + "4. StealItem")]
[Serializable]
public class StealItem : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Card> c = container.ConvertToCards();
        UniTask lastTask = UniTask.CompletedTask;
        foreach (var card in c)
        {
            card.GetMyPlayer()?.TryRemoveItem(card);
            if(card.Is<TagInShop>()) G.Main.Shop.RemoveMeFromShop(card);
            await card.SmokeCard();
            lastTask = source.effectPlayerSource.AddItem(card);
            await UniTask.Delay(200);
        }
        await lastTask;
        await G.Main.Shop.RestockSlots();
        return true;
    }
}
[AddTypeMenu(ActionNames.ItemName + "5. TurnIntoItemAndGainIt")]
[Serializable]
public class TurnIntoItemAndGainIt : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Card> c = container.ConvertToCards();
        UniTask lastTask = UniTask.CompletedTask;
        foreach (var card in c)
        {
            if(!card.Is<TagIsItem>()) card.AddTag(new TagIsItem());
            await card.TurnIntoItemEffect();
            await source.effectPlayerSource.AddItem(card);
            await PlayOnEnterGame(card);
            await UniTask.Delay(200);
        }
        return true;
    }

    private async UniTask PlayOnEnterGame(Card c)
    {
        List<IOnEnterGame> onEnterGame = c.state.OfType<IOnEnterGame>().ToList();
        foreach (var variable in onEnterGame)
        {
            await variable.OnEnterGame();
        }
    }
}
[AddTypeMenu(ActionNames.ItemName + "5. TurnInto SOUL")]
[Serializable]
public class TurnIntoSoulAndGainIt : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Card> c = container.ConvertToCards();
        UniTask lastTask = UniTask.CompletedTask;
        foreach (var card in c)
        {
            if (card.Is<TagIsItem>()) card.RemoveTag(card.Get<TagIsItem>());
            if(!card.Is<TagIsSoul>()) card.AddTag(new TagIsSoul());
            await card.TurnIntoSoulEffect();
            await source.effectPlayerSource.AddSoul(card);
            await UniTask.Delay(200);
        }
        return true;
    }
}