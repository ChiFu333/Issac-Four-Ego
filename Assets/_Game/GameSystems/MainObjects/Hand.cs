using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public const float UPMOVE = 0.4f;
    public SlotZone handZone { get; private set; }

    public void Init(float zWidth)
    {
        handZone = gameObject.AddComponent<SlotZone>();
        handZone.Init(zWidth);
    }

    public async UniTask AddCard(Entity card)
    {
        card.AddTag(new PlayFromHand());
        AudioManager.inst.Play(R.Audio.cardTaked);
        card.visual.render.sortingLayerName = "HandCards"; //Слой для рук!!
        await handZone.AddCard(card);
    }

    public async UniTask PlayCard(Entity card)
    {
        
        ExitCardFromHand(card);
        card.visual.transform.localPosition = Vector3.zero;
        
        await card.PutCardNearHand(this);
        card.visual.transform.DOLocalMoveY(0, 0.1f);
        card.visual.render.sortingOrder = 1000;
        card.visual.transform.DOScale(1, .15f).SetEase(Ease.OutBack);
        Effect e = card.GetTag<PlayEffect>().effect;

        CardStackEffect eff = new CardStackEffect(e, card);

        await StackSystem.inst.PushEffect(eff);
        G.Players.RestorePrior();

        Console.WriteText("Разыграна карта лута");
    }

    public async UniTask DiscardCard(Entity card)
    {
        ExitCardFromHand(card);
        await card.DiscardEntity();
    }

    private void ExitCardFromHand(Entity card)
    {
        card.visual.transform.eulerAngles = Vector3.zero;
        card.visual.transform.localScale = Vector3.one;
        card.visual.DOKill(true);
        
        card.RemoveTag(card.GetTag<PlayFromHand>());
        
        _ = handZone.RemoveCard(card);

        UIOnDeck.inst.UpdateTexts();
    }

    public int GetCount()
    {
        return handZone.GetCardCount();
    }

    public int GetMySortingOrder(Entity ent)
    {
        return handZone.GetSortingOrder(ent);
    }

    public float GetDeltaYInHand(Entity entity)
    {
        return handZone.GetDeltaY(entity);
    }
}
