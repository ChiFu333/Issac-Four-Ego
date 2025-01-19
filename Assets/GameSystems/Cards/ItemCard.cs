using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Analytics;

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
    public virtual void PlayFlipEffect()
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
        ItemCardData d = GetData<ItemCardData>();
        foreach(ItemEffect itemEffect in d.effects)
        {
            switch(itemEffect.type)
            {
                case ItemEffectType.Flip:
                {
                    foreach(Effect eff in itemEffect.effects)
                    {
                        GameMaster.inst.turnManager.cardTarget = this;
                        await eff.PlayActions();
                    }
                } break;
            }
        }

        GameMaster.inst.turnManager.RestorePrior();
        Console.WriteText("Использован предмет");
    }
}
