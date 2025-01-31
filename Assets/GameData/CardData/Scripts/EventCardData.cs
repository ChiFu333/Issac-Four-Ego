using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "New EventCard", menuName = "Cards/EventCards", order = 51)]
public class EventCardData : CardData
{
    [field: SerializeField] public List<EventEffect> eventEffects { get; private set; }
    public bool isCurse
    {
        get
        {
            foreach (var effect in eventEffects) if (effect.type == EventEffectType.Curse) return true;
            return false;
        }
    }
}
