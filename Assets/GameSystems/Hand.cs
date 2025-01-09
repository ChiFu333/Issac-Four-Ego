using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.Serialization;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public float deltaCard = Card.CARDPIXELSIZE.x * Card.CARDSIZE; // 1.13 и 0.28 для стандарта Card.CARDSIZE.x * GameMaster.CARDSIZE;
    private const float MAXLENGTH =  6.5f;
    private const float UPMOVE = 0.4f;
    [field: SerializeField] public List<Card> cards { get; private set; } = new List<Card>();
    public void AddCard(Card card)
    {
        card.Renderer.sortingLayerName = "HandCards"; //Слой для рук!!
        cards.Add(card);
        AddMoveUpAndDownTweens(card);
        card.transform.SetParent(transform);
        card.transform.localScale = Vector3.one * Card.CARDSIZE;
        UpdateCardPositions();
        UIOnDeck.inst.UpdateTexts();
    }
    public void PlayCard(LootCard card)
    {
        card.Collider.enabled = false;
        card.MouseDown -= MoveUp;
        card.MouseExit -= MoveDown;
        card.transform.DOKill();
        card.transform.localPosition = new Vector3(card.transform.localPosition.x, UPMOVE);

        cards.Remove(card);
        card.transform.SetParent(null);
        
        UIOnDeck.inst.UpdateTexts();
        UpdateCardPositions();

        card.PlayCard();
        if((card.data as LootCardData).lootEffect.Type == LootEffectType.Play)
        {
            card.transform.DOMove(CardPlaces.inst.lootStash.position, GameMaster.CARDSPEED).onComplete = () => GameMaster.inst.lootStash.PutOneCardUp(card);
        }
    }
    private void UpdateCardPositions()
    {
        for(int i = 0; i < cards.Count; i++)
        {
            cards[i].Renderer.sortingOrder = i;
        }

        for(int i = 0; i < cards.Count; i++)
        {
            float delCard = ((cards.Count - 1) * deltaCard / 2f) > MAXLENGTH/2 ? MAXLENGTH / (cards.Count - 1): deltaCard;
            float dStart = -(cards.Count - 1) * delCard / 2f;
            cards[i].transform.DOLocalMove(new Vector3(dStart + i * delCard, 0), GameMaster.CARDSPEED);
            cards[i].ChangeColliderSize(new Vector2(delCard/Card.CARDSIZE, Card.CARDPIXELSIZE.y + UPMOVE/Card.CARDSIZE));
            cards[i].Collider.offset = new Vector2(0, -UPMOVE/2 /Card.CARDSIZE);
        }
    }
    private void AddMoveUpAndDownTweens(Card c)
    {
        c.MouseDown += MoveUp;
        c.MouseExit += MoveDown;
    }
    private void MoveUp(Card c)
    {
        c.transform.DOLocalMoveY(UPMOVE, 0.3f);
        c.Renderer.sortingOrder = 100;
    }
    private void MoveDown(Card c)
    {
        c.transform.DOLocalMoveY(0, 0.3f);
        for(int i = 0; i < cards.Count; i++)
        {
            if(cards[i] == c) 
            {
                c.Renderer.sortingOrder = i;
                break;
            }
        }
    }
}
