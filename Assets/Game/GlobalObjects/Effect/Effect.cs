using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;

[Serializable]
public class Effect
{
    [HorizontalGroup("Row")]
    public When When;
    [HorizontalGroup("Row")]
    public Who Who;
    public UnityEvent Result;
}
[Serializable]
public class ItemEffect
{
    [HorizontalGroup("Row")]
    public ItemEffectType Type;
    [HorizontalGroup("Row")] [ShowIf("@Type == ItemEffectType.Buy")]
    public ValueType Value;
    [HorizontalGroup("Row")][ShowIf("@Type == ItemEffectType.Buy")]
    public int Count;
    public Effect Effect;
    public bool IsFlippable() => Type == ItemEffectType.Flip;
}
public enum ItemEffectType { Flip, Buy, Passive }
[Serializable]
public class LootEffect
{
    public LootEffectType Type;
    public List<Effect> Effects;
}
public enum LootEffectType { Play, Trinket };
public enum ValueType { Coin, Loot, HP }
public enum When { Now, Always, AfterBuy, AfterEnd, AfterPlayerDeath }
public enum Who { None, Me, ActivePlayer, YouSelect, Everyone }
