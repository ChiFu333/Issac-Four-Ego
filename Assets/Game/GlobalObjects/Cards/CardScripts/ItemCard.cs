using DG.Tweening;
using UnityEngine;

public class ItemCard : Card
{
    public bool IsFlippable = false;
    public bool IsFlipped = false;
    public override void Init(CardData d, bool isFaceUp)
    {
        base.Init(d, isFaceUp);
        IsFlippable = (d as ItemCardData).IsFlippable;

        MouseClicked += (Card c) => 
        {
            PlayFlipEffect();
        };
    }
    public virtual void PlayFlipEffect()
    {
        if(!IsFlipped && IsFlippable) Flip();
    }
    public void Flip()
    {
        if(IsFlippable)
        {
            transform.DORotate(new Vector3(0,0,-90), GameMaster.CARDSPEED);
            IsFlipped = true;
        }        
    }
    public void Recharge()
    {
        IsFlipped = false;
        transform.DORotate(new Vector3(0,0, 0), GameMaster.CARDSPEED);
    }
}
