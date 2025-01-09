using UnityEngine;

public class CharacterCard : ItemCard
{
    public override void Init(CardData d, bool isFaceUp)
    {
        base.Init(d, isFaceUp);
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
