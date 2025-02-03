using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class LootCard : Card
{
    private bool itemTrigger = false;
    public bool isItem = false;
    public override void Init(CardData d, bool isFaceUp = true)
    {
        base.Init(d, isFaceUp);
        MouseClicked += ClickToPlay;
    }
    public async void PlayCard()
    {
        Effect e = GetData<LootCardData>().GetPlayEffect();
        
        CardStackEffect eff = new CardStackEffect(e, this);
    
        await StackSystem.inst.PushEffect(eff);
        GameMaster.inst.turnManager.RestorePrior();

        Console.WriteText("Разыграна карта лута");
    }
    public void TurnIntoItem()
    {
        itemTrigger = true;
        isItem = true;
        MouseClicked -= ClickToPlay;
    }
    public override async Task DiscardCard()
    {
        if(!itemTrigger)
        {
            await DiscardCard<LootCard>();
        } 
        else
        {
            itemTrigger = false;
        }
    }
    private void ClickToPlay(Card c)
    {
        GameMaster.inst.turnManager.SetPrior(GetMyPlayer());
        GetMyPlayer().PlayLootCard(this);
    }
}