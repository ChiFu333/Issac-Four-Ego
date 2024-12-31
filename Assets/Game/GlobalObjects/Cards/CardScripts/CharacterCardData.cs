using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterData", menuName = "Cards/CharacterCardData", order = 51)]
public class CharacterCardData : ItemCardData
{
    public ItemCardData CharacterItemData;
    public int Hp = 2;
    public int Attack = 1;
    public int StartSouls = 0;
}
