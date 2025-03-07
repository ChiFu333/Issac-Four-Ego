using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
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
    public void PlayCard()
    {
        
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
        G.Players.SetPrior(GetMyPlayer());
        //GetMyPlayer().PlayLootCard(this);
    }
}