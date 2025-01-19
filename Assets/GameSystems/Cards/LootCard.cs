using System.Collections.Generic;
using UnityEngine;

public class LootCard : Card
{
    public override void Init(CardData d, bool isFaceUp = true)
    {
        base.Init(d, isFaceUp);
        MouseClicked += (Card c) => 
        {
            GameMaster.inst.turnManager.SetPrior(GetMyPlayer());
            GetMyPlayer().PlayCard(this);
        };
    }
    public async void PlayCard()
    {
        LootCardData d = GetData<LootCardData>();
        switch(d.lootEffect.type)
        {
            case LootEffectType.Trinket:
            {
                GameMaster.inst.turnManager.priorPlayer.AddItem(GetComponent<LootCard>());
            } break;
            case LootEffectType.Play:
            {
                foreach(Effect eff in d.lootEffect.effects)
                {
                    GameMaster.inst.turnManager.cardTarget = this;
                    await eff.PlayActions();
                }
            } break;
        }

        GameMaster.inst.turnManager.RestorePrior();
        Console.WriteText("Разыграна карта лута");
    }
}