using UnityEngine;

public class CharacterCard : ItemCard
{
    public override void Init(CardData d, bool isFaceUp)
    {
        base.Init(d, isFaceUp);
        IsFlippable = true;
    }
    public override async void PlayFlipEffect()
    {
        if(!IsFlipped)
        {
            TurnManager.Inst.SetPrior(GetMyPlayer());
            Card c = await SubSystems.Inst.SelectCardByType<LootCard>("MyHand");
            if(c != null)
            {
                Flip();
                GetComponentInParent<Player>().LootCanPlay++;
                GetComponentInParent<Player>().PlayCard(c as LootCard);
            }
        }
    }
}
