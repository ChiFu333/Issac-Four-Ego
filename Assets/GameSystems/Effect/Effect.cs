using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;

[Serializable] public class Effect
{
    [HorizontalGroup("Row")] public When when;
    [HorizontalGroup("Row")] public EffectType type;
    public List<EffectAction> effectActions;
    public async Task PlayActions()
    {
        if(type == EffectType.Common)
        {
            foreach (EffectAction action in effectActions) await action.PlayAction();
        }
        else if(type == EffectType.YouSelectOne)
        {
            int id = await EffectSelector.inst.SelectEffect(effectActions.Count);
            await effectActions[id].PlayAction();
        }
        else if(type == EffectType.Roll)
        {
            int id = await CubeManager.inst.ThrowDice() - 1;
            await effectActions[id].PlayAction();
        }
        return;
    }
}
[Serializable] public class EffectAction
{
    public Target target;
    public UnityEvent result;
    public async Task PlayAction()
    {
        if(target == Target.EveryPlayer)
        {
            for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
            {
                GameMaster.inst.turnManager.cardTarget = GameMaster.inst.turnManager.priorPlayer.GetMyCard();
                result?.Invoke();
                GameMaster.inst.turnManager.SetPrior(GameMaster.inst.turnManager.players[(GameMaster.inst.turnManager.priorId + 1) % GameMaster.PLAYERCOUNT]);
            }
        }
        else if(target == Target.EveryMonster)
        {
            for(int i = 0; i < GameMaster.inst.monsterZone.monstersInSlots.Count; i++)
            {
                GameMaster.inst.turnManager.cardTarget = GameMaster.inst.monsterZone.monstersInSlots[i];
                result?.Invoke();
            }
        }
        else
        {
            await PutTargetInStack();
            result?.Invoke();
            return;
        }
    }  
    public async Task PutTargetInStack()
    {
        Card t = null;
        switch(target)
        {
            case Target.None:
            {
                t = null;
            } break;
            case Target.PlayerMe:
            {
                t = GameMaster.inst.turnManager.priorPlayer.GetMyCard();
            } break;
            case Target.PlayerActive:
            {
                t = GameMaster.inst.turnManager.activePlayer.GetMyCard();
            } break;
            case Target.PlayerYouSelect:
            {
                t = await SubSystems.inst.SelectCardByType<CharacterCard>("InPlay");
            } break;
            case Target.YouSelectMonster:
            {
                t = await SubSystems.inst.SelectCardByType<MonsterCard>("InPlay");
            } break;
            case Target.YouSelectDamagable:
            {
                t = await SubSystems.inst.SelectCardByTypes<CharacterCard, MonsterCard>("InPlay");
            } break;
            case Target.YouSelectActiveItem:
            {
                while(true)
                {
                    ItemCard c = await SubSystems.inst.SelectCardByType<ItemCard>("InPlay");
                    if(c is not CharacterCard && c.IsFlippable)
                    {
                        t = c;
                        break;
                    }
                }
            } break;
        }
        GameMaster.inst.turnManager.cardTarget = t;
        return;
    }
} 
[Serializable] public class ItemEffect
{
    [HorizontalGroup("Row")] public ItemEffectType type;
    [HorizontalGroup("Row"), ShowIf("@type == ItemEffectType.Buy")] public ValueType value;
    [HorizontalGroup("Row"), ShowIf("@type == ItemEffectType.Buy")] public int count;
    public List<Effect> effects;
    public bool IsFlippable() => type == ItemEffectType.Flip;
}
[Serializable] public class EventEffect
{
    public EventEffectType type;
    public List<Effect> effects;
    public async Task GiveCurse(Card c)
    {
        if(type == EventEffectType.Curse)
        {
            Card t = await SubSystems.inst.SelectCardByType<CharacterCard>("InPlay");
            t.GetMyPlayer().AddCurse(c);
        }
    }
    public async Task PlayAction()
    {
        foreach(Effect effect in effects) await effect.PlayActions();
    }
}
public enum ItemEffectType { Flip, Buy, Passive }
[Serializable] public class LootEffect
{
    public LootEffectType type;
    public List<Effect> effects;
}
public enum LootEffectType { Play, Trinket };
public enum EventEffectType { Play, Curse };
public enum ValueType { Coin, Loot, HP }
public enum EffectType { Common, YouSelectOne, Roll}
public enum When { Now, Always, AfterBuy, AfterEnd, AfterPlayerDeath }
public enum Target 
{ 
    None, 
    PlayerActive, PlayerMe, PlayerMyLeft, PlayerMyRight, PlayerYouSelect, 
    YouSelectMonster, YouSelectDamagable, 
    YouSelectActiveItem, 
    EveryPlayer, EveryMonster 
}
//Монстр (атакуемый, неатакуемый, любой, выбранный)
//Игрок (мёртвый, живой)
//Убийца (монстра игрока)
//Кубик (атаки, не-атаки, любой)