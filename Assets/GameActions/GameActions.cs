using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public interface IAction {}
public interface IGameAction : IAction
{
    public UniTask<bool> Execute(Effect source, List<ISelectableTarget> container);
}
public interface ITargetAction : IAction
{
    public List<ISelectableTarget> container { get; set; }
}
public interface ITargetSelector : ITargetAction
{
    public UniTask<bool> SetTarget(Card source, bool isCancelabale = true);
}
public interface ITargetConverter : ITargetAction
{
    public void ConvertTarget(List<ISelectableTarget> container);
}

internal abstract class ActionNames
{
    public const string SelectingName = "1. Select Target/";
    public const string SelectingNamePlayer = "1. Players/";
    public const string SelectingNameDeck = "2. Decks/";
    
    public const string ConvertingName = "2. Convert Target/";
    public const string CoinName = "3. Coins/";
    public const string LootName = "4. LootCards/";
    public const string ItemName = "5. Items/";
    public const string StatName = "6. Stats/";
    public const string DamageName = "7. Damage & Death/";
    public const string WatchName = "8. Watching/";
}

[AddTypeMenu(ActionNames.DamageName + "1. RequestDamage")]
[Serializable]
public class RequestDamage : IGameAction
{
    
    public int count;

    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        List<Player> players = container.ConvertToPlayers();
        UniTask lastTask = UniTask.CompletedTask;

        foreach (var player in players)
        {
            lastTask = player.Get<TagBasePlayerData>().characterCard.Get<TagCharacteristics>().Damage(count);
            await UniTask.Delay(150);
        }
        
        await lastTask;
        return true;
    }
}
[AddTypeMenu( "19. CancelStackEffect")]
[Serializable]
public class CancelStackEffect : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        StackUnit su = container.ConvertToMyStackUnit();
        UniTask lastTask = UniTask.CompletedTask;

        await G.Main.StackSystem.RemoveStackUnit(su);
        
        await lastTask;
        return true;
    }
}

[AddTypeMenu( "20. RerollCube")]
[Serializable]
public class ChangeCubeValueEffect : IGameAction
{
    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        StackUnitCube su = container.ConvertToStackUnitCube();
        UniTask lastTask = UniTask.CompletedTask;

        await su.Reroll();
        
        await lastTask;
        return true;
    }
}