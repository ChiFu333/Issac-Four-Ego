using UnityEngine;
[CreateAssetMenu(fileName = "New ItemCard", menuName = "Cards/ItemCardData", order = 51)]
public class ItemCardData : CardData
{
    public bool IsFlippable = false;
    public bool IsStartFlipped = false;
}
