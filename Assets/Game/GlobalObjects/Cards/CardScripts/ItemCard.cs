using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ItemCard : Card
{
    public bool IsFlippable = false;
    public bool IsFlipped = false;
    public override void Init(CardData d, bool isFaceUp)
    {
        base.Init(d, isFaceUp);
        IsFlippable = (d as ItemCardData).IsFlippable();

        MouseClicked += (Card c) => 
        {
            PlayFlipEffect();
        };
    }
    public virtual void PlayFlipEffect()
    {
        if(!IsFlipped && IsFlippable) 
        {
            Flip();
            InvokeFlipEffect();
        }
    }
    public void Flip()
    {
        if(IsFlippable)
        {
            transform.DORotate(new Vector3(0,0,-90), GameMaster.CARDSPEED);
            IsFlipped = true;
        }        
    }
    public void Recharge()
    {
        IsFlipped = false;
        transform.DORotate(new Vector3(0,0, 0), GameMaster.CARDSPEED);
    }
    private async void InvokeFlipEffect()
    {
        ItemCardData d = (ItemCardData)data;
        Effect eff = null;

        for(int i  = 0; i < d.Effects.Count; i++)
        {
            if(d.Effects[i].Type == ItemEffectType.Flip) 
            {
                eff = d.Effects[i].Effect;
            }
        }
        if(eff == null) return;

        Player p = null;
        switch(eff.Who)
        {
            case Who.YouSelect:
                CharacterCard c = await SubSystems.Inst.SelectCardByType<CharacterCard>("InPlay");
                p = c.GetMyPlayer();
            break;
        }
        switch(eff.When)
        {
            case When.Now:
                TurnManager.Inst.SetPrior(p);
                eff.Result.Invoke();
                TurnManager.Inst.RestorePrior();
                Console.WriteText("Эффект разыгран");
            break;
        }
    }
}
