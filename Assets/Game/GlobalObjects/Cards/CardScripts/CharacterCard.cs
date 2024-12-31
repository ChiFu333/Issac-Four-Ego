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
            Flip();
            TurnManager.Inst.SetPrior(GetMyPlayer());
            SubSystems.Inst.SelectCardByType<LootCard>(GetMyPlayer(),"MyHand");
            SubSystems.OnSelected = (Card c) => 
            {
                if(c != null)
                {
                    GetComponentInParent<Player>().LootCanPlay++;
                    GetComponentInParent<Player>().PlayCard(c as LootCard);
                }
            };
        }
    }
}
