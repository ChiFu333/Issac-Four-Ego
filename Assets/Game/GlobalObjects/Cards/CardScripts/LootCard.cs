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
        d.actionToPlay.Invoke();
        Console.WriteText("Разыграна карта лута");
    }
}
