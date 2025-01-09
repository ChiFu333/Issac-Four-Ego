using UnityEngine;
using Sirenix.OdinInspector;
[CreateAssetMenu(fileName = "New MonsterCard", menuName = "Cards/MonsterCardData", order = 51)]
public class MonsterCardData : CardData
{
    [field: SerializeField, HorizontalGroup("HP")] public int hp { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int dodge { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int attack { get; private set; }
    [field: SerializeField] public Effect reward { get; private set; }
}
