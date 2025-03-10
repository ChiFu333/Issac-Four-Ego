using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "New MonsterCard", menuName = "Cards/MonsterCardData", order = 51)]
public class MonsterCardData : CardData
{
    [field: SerializeField, HorizontalGroup("HP")] public int hp { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int dodge { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int attack { get; private set; }
    [field: SerializeField] public List<MonsterEffect> monsterEffects { get; private set; }
    public Effect GetRewardEffect()
    {
        List<Effect> effs = new List<Effect>();
        foreach (var effect in monsterEffects) 
        {
            if (effect.type == MonsterEffectType.Reward) effs.Add(effect.effect);
        }
        if(effs.Count > 0) return effs[0];
        else
        {
            return null; //await EffectSelector.inst.SelectEffect(face, effs.Count)
        }
    }
}
