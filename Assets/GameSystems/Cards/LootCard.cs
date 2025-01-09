using UnityEngine;

public class LootCard : Card
{
    public override void Init(CardData d, bool isFaceUp = true)
    {
        base.Init(d, isFaceUp);
        MouseClicked += (Card c) => 
        {
            GetMyPlayer().PlayCard(this);
        };
    }
    public void PlayCard()
    {
        LootCardData d = data as LootCardData;
        if(d.lootEffect.Type == LootEffectType.Trinket)
        {
             GameMaster.inst.turnManager.priorPlayer.AddItem(GetComponent<LootCard>());
        }
        else if(d.lootEffect.Type == LootEffectType.Play)
        {
            
        }
        Console.WriteText("Разыграна карта лута");
    }
}
