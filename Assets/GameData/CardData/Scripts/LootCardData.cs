using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New LootData", menuName = "Cards/LootCardData", order = 51)]
public class LootCardData : CardData
{
    [field: SerializeField] public List<LootEffect> lootEffects { get; private set; }
    public Effect GetPlayEffect()
    {
        for(int i = 0; i < lootEffects.Count; i++)
        {
            if(lootEffects[i].type == LootEffectType.Play)
            {
                return lootEffects[i].effect;
            }
        }
        return null;
    }
    public Effect GetTrinketEffect()
    {
        for(int i = 0; i < lootEffects.Count; i++)
        {
            if(lootEffects[i].type == LootEffectType.Trinket)
            {
                return lootEffects[i].effect;
            }
        }
        return null;
    }
}
