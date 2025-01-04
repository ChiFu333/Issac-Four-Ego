using UnityEngine;

public class LootCard : Card
{
    public override void Init(CardData d, bool isFaceUp = true)
    {
        base.Init(d, isFaceUp);
        MouseClicked += (Card c) => 
        {
            if(TurnManager.Inst.IsMyTurn() && c.GetMyPlayer() == TurnManager.Inst.ActivePlayer) TurnManager.Inst.Players[0].PlayCard(this);
        };
    }
    public void PlayCard()
    {
        LootCardData d = data as LootCardData;
        if(d.LootEffect.Type == LootEffectType.Play)
        {
            foreach(Effect l in d.LootEffect.Effects) l.Result.Invoke();
        }
        else if(d.LootEffect.Type == LootEffectType.Trinket)
        {
            TurnManager.Inst.GetPriorPlayer().AddItem(GetComponent<LootCard>());
        }
        Console.WriteText("Разыграна карта лута");
    }
}
