using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Hand : MonoBehaviour
{
    public const float UPMOVE = 0.4f;
    public SlotZone handZone { get; private set; }

    public void Init(float zWidth)
    {
        handZone = gameObject.AddComponent<SlotZone>();
        handZone.Init(zWidth, CheckIsUnderLootPlace, PlayC);
    }

    public async UniTask AddCard(Card card)
    {
        card.AddTag(new TagInHand());
        //AudioManager.inst.Play(R.Audio.cardTaked);
        await handZone.AddCard(card);
    }

    public async UniTask PlayCard(Card card)
    {
        await UniTask.CompletedTask;
        ExitCardFromHand(card);
        await G.Main.StackSystem.PutStackUnit(new StackUnitLootCard(card));
        
        /*
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
        */
    }

    public async UniTask DiscardCard(Card card)
    {
        ExitCardFromHand(card);
        await card.DiscardCard();
    }

    private void ExitCardFromHand(Card card)
    {
        card.visual.transform.eulerAngles = Vector3.zero;
        card.visual.transform.localScale = Vector3.one;
        card.visual.DOKill(true);
        
        card.RemoveTag(card.Get<TagInHand>());
        
        _ = handZone.RemoveCard(card);

        GetComponentInParent<Player>().UpdateStats();
        card.transform.parent = null;
    }

    public int GetCount()
    {
        return handZone.GetCardCount();
    }

    public int GetMySortingOrder(Card ent)
    {
        return handZone.GetSortingOrder(ent);
    }

    public float GetDeltaYInHand(Card entity)
    {
        return handZone.GetDeltaY(entity);
    }

    public bool CheckIsUnderLootPlace()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        Collider2D[] colliders = hits.Select(hit => hit.collider).ToArray();
        return colliders.Any(col => col.CompareTag("LootPlayZone"));
    }

    public async UniTask<bool> PlayC(Card c)
    {
        if (c.Get<TagPlayEffect>() != null)
        {
            Effect eff = c.Get<TagPlayEffect>().effect;
            if (!await eff.SetTargets(GetComponentInParent<Player>(), c))
            {
                c.RestoreLit();
                return false;
            }
        }
        c.RestoreLit();
        //G.AudioManager.PlayWithRandomPitch(R.Audio.PlayLootCard, 0.2f);
        _ = PlayCard(c);
        return true;
    }
}
