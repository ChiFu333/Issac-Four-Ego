using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New ItemCard", menuName = "Cards/ItemCardData", order = 51)]
public class ItemCardData : CardData
{
    public bool IsStartFlipped = false;
    public List<ItemEffect> Effects;
    public bool IsFlippable()
    {
        foreach (var effect in Effects)
        {
            if (effect.IsFlippable()) return true;
        }
        return false;
    }
}
