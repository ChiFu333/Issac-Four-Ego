using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
[CreateAssetMenu(fileName = "New ItemCard", menuName = "Cards/ItemCardData", order = 51)]
public class ItemCardData : CardData
{
    [field: SerializeField] public bool isStartFlipped { get; private set; } = false; //Возможно убрать и вживить как эффект 
    [field: SerializeField] public List<ItemEffect> effects { get; private set; }
    public bool IsFlippable 
    {
        get
        {
            foreach (var effect in effects) if (effect.IsFlippable()) return true;
            return false;
        }
    }
    public bool IsEternal
    {
        get
        {
            foreach (var effect in effects) if (effect.type == ItemEffectType.Eternal) return true;
            return false;
        }
    }
    public bool IsGuppy
    {
        get
        {
            foreach (var effect in effects) if (effect.type == ItemEffectType.Guppy) return true;
            return false;
        }
    }
    public async Task<Effect> GetFlipEffect()
    {
        List<Effect> effs = new List<Effect>();
        foreach (var effect in effects) 
        {
            if (effect.IsFlippable()) effs.Add(effect.effect);
        }
        if(effs.Count == 1) return effs[0];
        else
        {
            return effs[await EffectSelector.inst.SelectEffect(face, effs.Count)]; //await EffectSelector.inst.SelectEffect(face, effs.Count)
        }
    }
    public Effect GetPassiveEffect()
    {
        List<Effect> effs = new List<Effect>();
        foreach (var effect in effects) 
        {
            if (effect.type == ItemEffectType.Passive) effs.Add(effect.effect);
        }
        if(effs.Count > 0) return effs[0];
        else
        {
            return null; //await EffectSelector.inst.SelectEffect(face, effs.Count)
        }
    }
}
