using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New CharacterData", menuName = "Cards/CharacterCardData", order = 51)]
public class CharacterCardData : ItemCardData
{
    [field: SerializeField] public ItemCardData characterItemData { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int hp { get; private set; } = 2;
    [field: SerializeField, HorizontalGroup("HP")] public int attack { get; private set; } = 1;
    [field: SerializeField] public int startSouls = 0; //возможно убрать и перенести в эффект
}
