using UnityEngine;

public class CharacterCard : ItemCard
{
    public override void Init(CardData d, bool isFaceUp)
    {
        base.Init(d, isFaceUp);
        IsFlippable = true;
    }
    public override void PlayFlipEffect()
    {
        if(!IsFlipped)
        {
            GetMyPlayer().lootPlayCount++;
            Flip();
        }
    }
}
