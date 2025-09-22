using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

[AddTypeMenu(ActionNames.WatchName + "1. Watch X put 1 up")]
[Serializable]
public class WatchXPutOneUp : IGameAction
{
    public int count;
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        Deck deck = container.ConvertToDeck();
        UniTask lastTask = UniTask.CompletedTask;

        await G.Main.CardWatcher.WatchXPutOneUp(deck, count);
        
        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.WatchName + "2. Watch hand")]
[Serializable]
public class WatchHand : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Player> players = container.ConvertToPlayers();
        UniTask lastTask = UniTask.CompletedTask;
        foreach (var player in players)
        {
            await G.Main.CardWatcher.WatchHand(player);
        }
        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.WatchName + "3. Watch one card on deck")]
[Serializable]
public class WatchOneDeckCard : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        Deck deck = container.ConvertToDeck();
        UniTask lastTask = UniTask.CompletedTask;

        await G.Main.CardWatcher.WatchTopAndCanDiscard(deck);
        
        await lastTask;
        return true;
    }
}