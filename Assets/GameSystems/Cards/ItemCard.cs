using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Analytics;
using System.Threading.Tasks;

public class ItemCard : Card
{
    [field: SerializeField] public bool IsFlippable { get; protected set; }
    [field: SerializeField] public bool IsFlipped { get; private set; }
    public override void Init(CardData d, bool isFaceUp)
    {
        base.Init(d, isFaceUp);
        IsFlippable = (d as ItemCardData).IsFlippable;

        MouseClicked += (Card c) => 
        {
            PlayFlipEffect();
        };
    }
    public void PlayFlipEffect()
    {
        if(!SubSystems.inst.isSelectingSomething &&!IsFlipped && IsFlippable && transform.parent != GameMaster.inst.shop.transform) 
        {
            Flip();
            InvokeFlipEffect();
        }
    }
    public void Flip()
    {
        if(IsFlippable)
        {
            IsFlipped = true;
            transform.DORotate(new Vector3(0,0,-90), GameMaster.CARDSPEED);
        }        
    }
    public void Recharge()
    {
        IsFlipped = false;
        transform.DORotate(new Vector3(0,0, 0), GameMaster.CARDSPEED);
    }
    private async void InvokeFlipEffect()
    {
        GameMaster.inst.turnManager.SetPrior(GetMyPlayer());
        ItemCardData d = GetData<ItemCardData>();
        
        CardStackEffect csf = new CardStackEffect(await GetData<ItemCardData>().GetFlipEffect(), this);
        await StackSystem.inst.PushEffect(csf);

        GameMaster.inst.turnManager.RestorePrior();
        Console.WriteText("Использован предмет");
    }
    public override async Task DiscardCard()
    {
        await DiscardCard<ItemCard>();
    }
}
