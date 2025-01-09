using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ItemCard : Card
{
    [field: SerializeField] public bool IsFlippable { get; private set; }
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

        for(int i  = 0; i < d.effects.Count; i++)
        {
            if(d.effects[i].type == ItemEffectType.Flip) 
            {
                eff = d.effects[i].effect;
            }
        }
        if(eff == null) return;

        Player p = null;
        switch(eff.target)
        {
            case Target.YouSelectPlayer:
                CharacterCard c = await SubSystems.Inst.SelectCardByType<CharacterCard>("InPlay");
                p = c.GetMyPlayer();
            break;
        }
        switch(eff.when)
        {
            case When.Now:
                GameMaster.inst.turnManager.SetPrior(p);
                eff.result.Invoke();
                GameMaster.inst.turnManager.RestorePrior();
                Console.WriteText("Эффект разыгран");
            break;
        }
    }
}
