using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Interactions;
[Serializable] public class Effect
{
    [field: SerializeField, HorizontalGroup("Row")] public When when {get; private set;}
    [field: SerializeField, HorizontalGroup("Row"), ShowIf("@when == When.AtDiceWouldRoll || when == When.AtDiceRolls")] public int diceValue {get; private set;}
    [field: SerializeField, HorizontalGroup("Row")] public EffectType type {get; private set;}
    [field: SerializeField] public List<EffectAction> effectActions {get; private set;}
    public Effect(When when, int diceValue, EffectType type, List<EffectAction> effectActions)
    {
        this.when = when;
        this.diceValue = diceValue;
        this.type = type;
        this.effectActions = effectActions;
    }
    public async Task PlayEffect(int id = -1)
    {
        if(id == -1)
        {
            for(int i = 0; i < effectActions.Count; i++)
            {
                await effectActions[i].PlaySubActions();
            }
        }
        else
        {
            await effectActions[id].PlaySubActions();
        }
    }
    public async Task SetTargets(Entity source, int id = -1)
    {
        if(id == -1)
        {
            for(int i = 0; i < effectActions.Count; i++)
            {
                await effectActions[i].SetTargets(source);
            }
        }
        else
        {
            await effectActions[id].SetTargets(source);
        }
    }
}
[Serializable] public class EffectAction
{
    [field: SerializeField] public List<EffectSubAction> subActions {get; private set;}
    public async Task PlaySubActions()
    {
        bool b = true;
        for(int i = 0; i < subActions.Count; i++)
        {
            if(subActions[i].connection == SubActionConnection.Start)
            {
                b = await subActions[i].InvokePlayAction();
            }
            else if(subActions[i].connection == SubActionConnection.Then)
            {
                b = await subActions[i].InvokePlayAction();
                
            }
            else if(subActions[i].connection == SubActionConnection.And && b)
            {
                b = await subActions[i].InvokePlayAction();
            }
            else
            {
                b = false;
                
            }
        }
    }
    public async Task SetTargets(Entity source)
    {
        for(int i = 0; i < subActions.Count; i++)
        {
            if(i != 0) subActions[i].targetCardBefore = subActions[i-1].targetCard;
            await subActions[i].SetTarget(source);
        }
    }
} 
[Serializable] public class EffectSubAction
{
    [field: SerializeField] public SubActionConnection connection {get; private set;} 
    [field: SerializeField, HorizontalGroup("Row2"), Space] public Target target {get; private set;}    
    [field: SerializeField, HorizontalGroup("Row2"), Space] public ActionType actionType {get; private set;} 
    [field: SerializeField, HorizontalGroup("Row2"), Space] public int count;
    public Func<int, Task<bool>> actionDelegate {get; private set;}
    public Entity targetCard {get; private set;}    
    [field: HideInInspector] public Entity targetCardBefore;
    public async Task SetTarget(Entity source)
    {
        actionDelegate = CardTweaks.GetDelegate[actionType];
        Entity t = null;
        switch(target)
        {
            case Target.None:
            {
                t = null;
            } break;
            case Target.PlayerMe:
            {
                t = G.Players.priorPlayer.GetMyCard();
            } break;
            case Target.PlayerActive:
            {
                t = G.Players.activePlayer.GetMyCard();
            } break;
            case Target.SelectedTarget:
            {
                t = targetCardBefore;
            } break;
            case Target.It:
            {
                t = source;
            } break;
            case Target.PlayerYouSelect:
            {
                Console.WriteText("Выбери игрока");
                t = await SubSystems.inst.SelectCardByType("InPlay");
                Console.WriteText("Выбор сделан");
            } break;
            case Target.YouSelectMonster:
            {
                Console.WriteText("Выбери монстра");
                t = await SubSystems.inst.SelectCardByType("InPlay");
                Console.WriteText("Выбор сделан");
            } break;
            case Target.YouSelectDamagable:
            {
                Console.WriteText("Выбери существо");
                //t = await SubSystems.inst.SelectCardByTypes<CharacterCard, MonsterCard>("InPlay");
                Console.WriteText("Выбор сделан");
            } break;
            case Target.YouSelectActiveItem:
            {
                Console.WriteText("Выбери предмет");
                while(true)
                {
                    Entity c = await SubSystems.inst.SelectCardByType("InPlay");
                    if(/*c is not CharacterCard && c.IsFlippable*/true)
                    {
                        t = c;
                        break;
                    }
                }
                Console.WriteText("Выбор сделан");
            } break;
            case Target.YouSelectCurse:
            {
                Console.WriteText("Выбери проклятье");
                while(true)
                {
                    Entity c = await SubSystems.inst.SelectCardByType("InPlay");
                    if(/*c.GetData<EventCardData>().isCurse*/true)
                    {
                        t = c;
                        break;
                    }
                }
                Console.WriteText("Выбор сделан");
            } break;
            case Target.StackLootOrActiveOrBuy:
            {
                StackEffect eff = StackSystem.inst.stack.Peek();
                if(eff is CardStackEffect cardData)
                {
                    if(/*!cardData.triggeredEffect && (cardData.source is LootCard || cardData.source as ItemCard)*/true)
                    {
                        //изменить данные в стеке на карты, чтобы можно было 
                        t = G.Players.priorPlayer.GetMyCard();
                    }
                }
            } break;
        }
        targetCard = t;
    }
    public async Task<bool> InvokePlayAction()
    {
        actionDelegate = CardTweaks.GetDelegate[actionType];
        //Тут надо докрутить, что эффект сработал если хотябы 1 чел получил что-то
        if(target == Target.EveryPlayer)
        {
            for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
            {
                StackSystem.inst.cardTarget = G.Players.priorPlayer.GetMyCard();
                await PlayAction();
                G.Players.SetPrior(G.Players.players[(G.Players.priorId + 1) % GameMaster.PLAYERCOUNT]);
            }
        }
        else if(target == Target.EveryMonster)
        {
            for(int i = 0; i < G.monsterZone.monstersInSlots.Count; i++)
            {
                StackSystem.inst.cardTarget = G.monsterZone.monstersInSlots[i];
                await PlayAction();
            }
        }
        /*
        else if(targetCard != null)
        {
            StackSystem.inst.cardTarget = targetCard;
            return await PlayAction();
        }
        return false;
        */
        StackSystem.inst.cardTarget = targetCard;
        return await PlayAction();
    }
    private async Task<bool> PlayAction() => await actionDelegate.Invoke(count);
}
public enum EffectType { Common, YouSelectOne, Roll}
public enum SubActionConnection { Start, And, Then, IfYouDo }
[Serializable] public class MyUnityEvent : UnityEvent<int> {}
public enum LootEffectType { Play, Trinket };
[Serializable] public class LootEffect
{
    public LootEffectType type;
    public Effect effect;
}
public enum ItemEffectType { Flip, Buy, Passive, Eternal, Guppy }
[Serializable] public class MonsterEffect
{
    public MonsterEffectType type;
    public Effect effect;
}
public enum MonsterEffectType { Reward, Passive }
[Serializable] public class ItemEffect
{
    [HorizontalGroup("Row")] public ItemEffectType type;
    [HorizontalGroup("Row"), ShowIf("@type == ItemEffectType.Buy")] public ValueType value;
    [HorizontalGroup("Row"), ShowIf("@type == ItemEffectType.Buy")] public int count;
    public Effect effect;
    public bool IsFlippable() => type == ItemEffectType.Flip;
}
public enum EventEffectType { Play, Curse };
[Serializable] public class EventEffect
{
    public EventEffectType type;
    public Effect effect;
}


public enum ValueType { Coin, Loot, HP }

public enum When 
{ 
    Now = 0,
    Always = 1, 
    //AsTheStartOfTurn = 2,
    AtTheStartOfTurn = 3,  
    //AsTheStartOfMyTurn = 4,
    AtTheStartOfMyTurn = 5,
    //AsTheEndOfTurn = 6,
    AtTheEndOfTurn = 7,  
    //AsTheEndOfMyTurn = 8,
    AtTheEndOfMyTurn = 9,
    AtDicePuttedInStack = 10,
    AtDiceWouldRoll = 11,
    AtDiceRolls = 12
}
public enum Target 
{ 
    None, 
    PlayerActive, PlayerMe, PlayerMyLeft, PlayerMyRight, PlayerYouSelect, 
    YouSelectMonster, YouSelectDamagable, 
    YouSelectActiveItem, 
    EveryPlayer, EveryMonster,
    SelectedTarget,
    YouSelectCurse = 12,
    StackLootOrActiveOrBuy = 13,
    It = 14
}