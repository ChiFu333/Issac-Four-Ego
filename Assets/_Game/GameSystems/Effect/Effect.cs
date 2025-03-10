using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;
using Cysharp.Threading.Tasks;

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
    
    public async UniTask PlayEffect(int id = -1)
    {
        if(id == -1)
        {
            foreach (var t in effectActions)
            {
                await t.PlaySubActions();
            }
        }
        else
        {
            await effectActions[id].PlaySubActions();
        }
    }
    public async UniTask SetTargets(Entity source, int id = -1)
    {
        if(id == -1)
        {
            foreach (var t in effectActions)
            {
                await t.SetTargets(source);
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
    
    public async UniTask PlaySubActions()
    {
        bool b = true;
        foreach (var t in subActions)
        {
            if(t.connection == SubActionConnection.Start)
            {
                b = await t.InvokePlayAction();
            }
            else if(t.connection == SubActionConnection.Then)
            {
                b = await t.InvokePlayAction();
                
            }
            else if(t.connection == SubActionConnection.And && b)
            {
                b = await t.InvokePlayAction();
            }
            else
            {
                b = false;
            }
        }
    }
    public async UniTask SetTargets(Entity source)
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
    
    public Func<int, UniTask<bool>> actionDelegate {get; private set;}
    public Entity targetCard {get; private set;}    
    
    [field: HideInInspector] public Entity targetCardBefore;
    public async UniTask SetTarget(Entity source)
    {
        actionDelegate = GameActions.GetDelegate[actionType];
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
                t = await G.CardSelector.SelectCardByType<CardTypeTag>("InPlay", CardType.characterCard);
                Console.WriteText("Выбор сделан");
            } break;
            case Target.YouSelectMonster:
            {
                Console.WriteText("Выбери монстра");
                t = await G.CardSelector.SelectCardByType<CardTypeTag>("InPlay", CardType.monsterCard);
                Console.WriteText("Выбор сделан");
            } break;
            case Target.YouSelectDamagable:
            {
                Console.WriteText("Выбери существо");
                t = await G.CardSelector.SelectCardByType<Characteristics>("InPlay");
                Console.WriteText("Выбор сделан");
            } break;
            case Target.YouSelectActiveItem:
            {
                Console.WriteText("Выбери предмет");
                while(true)
                {
                    Entity c = await G.CardSelector.SelectCardByType<CardTypeTag>("InPlay", CardType.treasureCard);
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
                    Entity c = await G.CardSelector.SelectCardByType<CardTypeTag>("InPlay");
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
    public async UniTask<bool> InvokePlayAction()
    {
        actionDelegate = GameActions.GetDelegate[actionType];
        //Тут надо докрутить, что эффект сработал если хотябы 1 чел получил что-то
        if(target == Target.EveryPlayer)
        {
            for(int i = 0; i < G.Players.players.Count; i++)
            {
                StackSystem.inst.cardTarget = G.Players.priorPlayer.GetMyCard();
                await PlayAction();
                G.Players.SetPrior(G.Players.players[(G.Players.priorId + 1) % G.Players.players.Count]);
            }
        }
        else if(target == Target.EveryMonster)
        {
            foreach (var t in G.monsterZone.monstersInSlots)
            {
                StackSystem.inst.cardTarget = t;
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
    private async UniTask<bool> PlayAction() => await actionDelegate.Invoke(count);
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