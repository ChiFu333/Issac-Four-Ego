using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;

[Serializable] public class Effect
{
    [HorizontalGroup("Row")] public When when;
    [HorizontalGroup("Row")] public Target target;
    public UnityEvent result;
}
[Serializable] public class ItemEffect
{
    [HorizontalGroup("Row")] public ItemEffectType type;
    [HorizontalGroup("Row")][ShowIf("@Type == ItemEffectType.Buy")] public ValueType value;
    [HorizontalGroup("Row")][ShowIf("@Type == ItemEffectType.Buy")] public int count;
    public Effect effect;
    public bool IsFlippable() => type == ItemEffectType.Flip;
}
public enum ItemEffectType { Flip, Buy, Passive }
[Serializable] public class LootEffect
{
    public LootEffectType Type;
    public List<Effect> Effects;
}
public enum LootEffectType { Play, Trinket };
public enum ValueType { Coin, Loot, HP }
public enum When { Now, Always, AfterBuy, AfterEnd, AfterPlayerDeath }
public enum Target { None, Me, ActivePlayer, YouSelectPlayer, YouSelectDamagable, }
