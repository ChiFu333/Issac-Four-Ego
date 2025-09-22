using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public enum CardType {none, characterCard, lootCard, monsterCard, treasureCard, eventCard, soulCard};

[Serializable] 
public class TagCardType : EntityComponentDefinition
{
    public CardType cardType = CardType.none;
    public StackCardUnitType cloneType = StackCardUnitType.None;
}
[Serializable] 
public class TagCharacterItemPrefab : EntityComponentDefinition
{
    public GameObject item;
}
[Serializable] public class TagTappable : EntityComponentDefinition, IInitable, IOnMouseDown, IHaveUI
{
    private Card card;
    private GameObject tapSprite;
    public bool tapped = false;
    public async void Init(Card c) {
        card = c;
        tapSprite = card.GetVisualObject<TagTappable>();
        
        if(card.isFaceUp) 
            ShowUI();
        else
            HideUI();
    }
    public void Tap()
    {
        tapped = true;
        ShowUI();
        card.visual.transform.parent.DORotate(new Vector3(0,0,-20-360), 0.5f,RotateMode.FastBeyond360).SetEase(Ease.InOutBack);
        card.AddGreyScale();
    }
    public async UniTask Recharge()
    {
        if(!tapped) return;
        tapped = false;
        R.Audio.batterycharge.PlayAsSoundRandomPitch(0.2f);
        card.RemoveGreyScale();
        ShowUI();
        await card.visual.transform.parent.DORotate(new Vector3(0,0,720), 0.5f,RotateMode.FastBeyond360).SetEase(Ease.InOutBack).AsyncWaitForCompletion().AsUniTask();
    }

    async UniTask IOnMouseDown.OnMouseDown()
    {
        if (card.Is<TagInShop>() || card.Is<TapBalatro>()) return;
        if (tapped) return;

        foreach (var tag in card.state.ToList())
        {
            if (tag is ITapEffect tapEffect && !await tapEffect.OnTap()) return;
        }

        Tap();
        R.Audio.ActiveItem.PlayAsSoundRandomPitch(0.1f);
    }

    public void ShowUI()
    {
        if(card.Is<TagInShop>() || card.Is<TapBalatro>()) 
            tapSprite.SetActive(false);
        else
            tapSprite.SetActive(!tapped);
    }
    public void HideUI()
    {
        tapSprite.SetActive(false);
    }
}

[Serializable]
public class TagInShop : EntityComponentDefinition, IInitable, IRemovable
{
    private Card card;
    public void Init(Card c)
    {
        card = c;
        if (card.Is<TagTappable>())
        {
            card.Get<TagTappable>().HideUI();
        }
    }

    public void Remove()
    {
        if (card.Is<TagTappable>())
        {
            card.Get<TagTappable>().ShowUI();
        }
    }
}
[Serializable]
public class TagIsItem : EntityComponentDefinition
{
    
}
[Serializable]
public class TagIsEternal : EntityComponentDefinition
{
    
}
[Serializable]
public class TagIsSoul : EntityComponentDefinition
{
    
}
