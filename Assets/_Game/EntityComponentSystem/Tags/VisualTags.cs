using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
using Random = UnityEngine.Random;

[Serializable] public class PlayFromHand : ITag, IOnMouseDown, IOnMouseEnter, IOnMouseExit, IRemovable
{   
    private Entity entity;
    public void Init(Entity entity) {
        this.entity = entity;
    }
    public UniTask OnMouseDown()
    {
        entity.GetMyPlayer().PlayLootCard(entity);
        return UniTask.CompletedTask;
    }

    public UniTask OnMouseEnter()
    {
        AudioManager.inst.Play(R.Audio.cardMouseOnLootInHands[Random.Range(0, R.Audio.cardMouseOnLootInHands.Count)]);
        entity.visual.transform.DOLocalMoveY(0.4f + entity.GetMyPlayer().hand.GetDeltaYInHand(entity), 0.3f);
        entity.visual.render.sortingOrder = 1000;
        
        entity.visual.transform.DOScale(1.15f, .15f).SetEase(Ease.OutBack);

        DOTween.Kill(2, true);
        entity.visual.transform.DOPunchRotation(Vector3.forward * 5, .15f, 20, 1).SetId(2);
        
        return UniTask.CompletedTask;
    }

    public UniTask OnMouseExit()
    {
        entity.visual.transform.DOLocalMoveY(0 + entity.GetMyPlayer().hand.GetDeltaYInHand(entity), 0.3f);
        entity.visual.render.sortingOrder = entity.GetMyPlayer().hand.GetMySortingOrder(entity);
        entity.visual.transform.DOScale(1, .15f).SetEase(Ease.OutBack);
        return UniTask.CompletedTask;
    }

    public void Remove()
    {
        entity.visual.transform.DOLocalMoveY(0, 0.1f);
        entity.visual.render.sortingOrder = 1000;
        entity.visual.transform.DOScale(1, .15f).SetEase(Ease.OutBack);
    }
}
[Serializable] public class TapBalatro : ITag, IOnMouseDown
{
    private Entity entity;
    public bool isTapped;
    public void Init(Entity entity) {
        this.entity = entity;
        isTapped = false;
    }
    
    public UniTask OnMouseDown()
    {
        isTapped = !isTapped;
        entity.visual.transform.DOLocalMoveY(isTapped ? 1f : 0, 0.1f);
        
        return UniTask.CompletedTask;
    }
}