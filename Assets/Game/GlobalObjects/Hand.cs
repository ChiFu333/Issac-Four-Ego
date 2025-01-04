using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.Serialization;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public float DeltaCard = Card.CARDPIXELSIZE.x * Card.CARDSIZE; // 1.13 и 0.28 для стандарта Card.CARDSIZE.x * GameMaster.CARDSIZE;
    private const float MAXLENGTH =  6.5f;
    private const float UPMOVE = 0.4f;
    public List<Card> Cards = new List<Card>();
    public void AddCard(Card card)
    {
        card.Renderer.sortingLayerName = "HandCards"; //Слой для рук!!
        Cards.Add(card);
        AddMoveUpAndDownTweens(card);
        card.transform.SetParent(transform);
        card.transform.localScale = Vector3.one * Card.CARDSIZE;
        UpdateCardPositions();
        UIOnDeck.Inst.UpdateTexts();
    }
    public void PlayCard(LootCard card)
    {
        card.Collider.enabled = false;
        card.MouseDown -= MoveUp;
        card.MouseExit -= MoveDown;
        card.transform.DOKill();
        card.transform.localPosition = new Vector3(card.transform.localPosition.x, UPMOVE);

        Cards.Remove(card);
        card.transform.SetParent(null);
        
        UIOnDeck.Inst.UpdateTexts();
        UpdateCardPositions();

        card.PlayCard();
        if((card.data as LootCardData).LootEffect.Type == LootEffectType.Play)
        {
            card.transform.DOMove(CardPlaces.Inst.LootStash.position, GameMaster.CARDSPEED).onComplete = () => GameMaster.Inst.LootStash.PutOneCardUp(card);
        }
    }
    public void UpdateCardPositions()
    {
        for(int i = 0; i < Cards.Count; i++)
        {
            Cards[i].Renderer.sortingOrder = i;
        }

        for(int i = 0; i < Cards.Count; i++)
        {
            float delCard = ((Cards.Count - 1) * DeltaCard / 2f) > MAXLENGTH/2 ? MAXLENGTH / (Cards.Count - 1): DeltaCard;
            float dStart = -(Cards.Count - 1) * delCard / 2f;
            Cards[i].transform.DOLocalMove(new Vector3(dStart + i * delCard, 0), GameMaster.CARDSPEED);
            Cards[i].ChangeColliderSize(new Vector2(delCard/Card.CARDSIZE, Card.CARDPIXELSIZE.y + UPMOVE/Card.CARDSIZE));
            Cards[i].Collider.offset = new Vector2(0, -UPMOVE/2 /Card.CARDSIZE);
        }
    }
    public void AddMoveUpAndDownTweens(Card c)
    {
        c.MouseDown += MoveUp;
        c.MouseExit += MoveDown;
    }
    public void MoveUp(Card c)
    {
        c.transform.DOLocalMoveY(UPMOVE, 0.3f);
        c.Renderer.sortingOrder = 100;
    }
    public void MoveDown(Card c)
    {
        c.transform.DOLocalMoveY(0, 0.3f);
        for(int i = 0; i < Cards.Count; i++)
        {
            if(Cards[i] == c) 
            {
                c.Renderer.sortingOrder = i;
                break;
            }
        }
    }
}
