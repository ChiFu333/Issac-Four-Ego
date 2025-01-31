using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public float deltaCard = Card.CARDPIXELSIZE.x * Card.CARDSIZE; // 1.13 и 0.28 для стандарта Card.CARDSIZE.x * GameMaster.CARDSIZE;
    private const float MAXLENGTH =  6.5f;
    public const float UPMOVE = 0.4f;
    [field: SerializeField] public List<Card> cards { get; private set; } = new List<Card>();
    public void AddCard(Card card)
    {
        card.render.sortingLayerName = "HandCards"; //Слой для рук!!
        cards.Add(card);
        AddMoveUpAndDownTweens(card);
        card.transform.SetParent(transform);
        card.transform.localScale = Vector3.one * Card.CARDSIZE;
        UpdateCardPositions();
        UIOnDeck.inst.UpdateTexts();
    }
    public void PlayCard(LootCard card)
    {
        ExitCardFromHand(card);
        card.PlayCard();    
    }
    public void DiscardCard(LootCard card)
    {
        ExitCardFromHand(card);
        card.DiscardCard();
    }
    private void ExitCardFromHand(LootCard card)
    {
        card.MouseDown -= MoveUp;
        card.MouseExit -= MoveDown;
        card.transform.DOKill();
        
        card.MoveTo(transform.TransformPoint(new Vector3(0, UPMOVE * 1.5f)), null);

        cards.Remove(card);
        
        UIOnDeck.inst.UpdateTexts();
        UpdateCardPositions();
    }
    private void UpdateCardPositions()
    {
        for(int i = 0; i < cards.Count; i++)
        {
            cards[i].render.sortingOrder = i;
        }

        for(int i = 0; i < cards.Count; i++)
        {
            float delCard = ((cards.Count - 1) * deltaCard / 2f) > MAXLENGTH/2 ? MAXLENGTH / (cards.Count - 1): deltaCard;
            float dStart = -(cards.Count - 1) * delCard / 2f;
            float localXPos = dStart + i * delCard;

            cards[i].MoveTo(transform.TransformPoint(new Vector3(localXPos, 0)), null, null, false);
            
            cards[i].Collider.size = new Vector2(delCard/Card.CARDSIZE, Card.CARDPIXELSIZE.y + UPMOVE/Card.CARDSIZE);
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
        c.render.sortingOrder = 1000;
    }
    private void MoveDown(Card c)
    {
        c.transform.DOLocalMoveY(0, 0.3f);
        for(int i = 0; i < cards.Count; i++)
        {
            if(cards[i] == c) 
            {
                c.render.sortingOrder = i;
                break;
            }
        }
    }
}
