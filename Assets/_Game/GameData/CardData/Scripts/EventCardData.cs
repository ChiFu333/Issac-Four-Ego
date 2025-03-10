using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "New EventCard", menuName = "Cards/EventCards", order = 51)]
public class EventCardData : CardData
{
    [field: SerializeField] public List<EventEffect> eventEffects { get; private set; }
    public Effect GetPlayEffect()
    {
        for(int i = 0; i < eventEffects.Count; i++)
        {
            if(eventEffects[i].type == EventEffectType.Play)
            {
                return eventEffects[i].effect;
            }
        }
        Debug.Log("NullThing");
        return null;
    }
    public Effect GetPassiveEffect()
    {
        List<Effect> effs = new List<Effect>();
        foreach (var effect in eventEffects) 
        {
            if (effect.type == EventEffectType.Curse) effs.Add(effect.effect);
        }
        if(effs.Count > 0) return effs[0];
        else
        {
            return null; //await EffectSelector.inst.SelectEffect(face, effs.Count)
        }
    }
    public bool isCurse
    {
        get
        {
            foreach (var effect in eventEffects) if (effect.type == EventEffectType.Curse) return true;
            return false;
        }
    }
}
