using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New LootData", menuName = "Cards/LootCardData", order = 51)]
public class LootCardData : CardData
{
    [field: SerializeField] public LootEffect lootEffect { get; private set; }
}
