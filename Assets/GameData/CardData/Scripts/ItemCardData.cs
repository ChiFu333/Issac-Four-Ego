using System.Collections.Generic;
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
}
