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
    [field: SerializeField] public List<Entity> cards { get; private set; } = new List<Entity>();
    public async Task AddCard(Entity card)
    {
        card.AddTag(new PlayFromHand());
        AudioManager.inst.Play(R.Audio.cardTaked);
        card.visual.render.sortingLayerName = "HandCards"; //Слой для рук!!
        cards.Add(card);
        AddMoveUpAndDownTweens(card);
        card.transform.SetParent(transform);
        card.transform.localScale = Vector3.one * Card.CARDSIZE;

        await UpdateCardPositions();
        
        //UIOnDeck.inst.UpdateTexts(G.Players.GetPlayerId(GetComponentInParent<Player>()));
    }
    public async Task PlayCard(Entity card)
    {
        ExitCardFromHand(card);
        await card.PutCardNearHand(this);
        
        Effect e = card.GetTag<PlayEffect>().effect;
        
        CardStackEffect eff = new CardStackEffect(e, card);
    
        await StackSystem.inst.PushEffect(eff);
        G.Players.RestorePrior();

        Console.WriteText("Разыграна карта лута");
    }
    public async Task DiscardCard(Entity card)
    {
        ExitCardFromHand(card);
        await card.DiscardEntity();
    }
    private void ExitCardFromHand(Entity card)
    {
        card.MouseDown -= MoveUp;
        card.MouseExit -= MoveDown;
        card.transform.DOKill();

        cards.Remove(card);
        
        UIOnDeck.inst.UpdateTexts();
        _ = UpdateCardPositions();
    }
    private async Task UpdateCardPositions()
    {
        for(int i = 0; i < cards.Count; i++)
        {
            cards[i].visual.render.sortingOrder = i;
        }
        List<bool> triggers = new List<bool>();
        for(int i = 0; i < cards.Count; i++)
        {
            bool aLot = ((cards.Count - 1) * deltaCard / 2f) > MAXLENGTH/2;
            float delCard = aLot ? MAXLENGTH / (cards.Count - 1): deltaCard;
            float dStart = -(cards.Count - 1) * delCard / 2f;
            float localXPos = dStart + i * delCard;

            triggers.Add(false);
            int t = i;
            _ = cards[i].MoveToForHand(transform.TransformPoint(new Vector3(localXPos, 0)), aLot ? cards[i].visual.GetAngleInHand() : 0, () => triggers[t] = true, false);
            
            cards[i].Collider.size = new Vector2(delCard/Card.CARDSIZE, Card.CARDPIXELSIZE.y + UPMOVE/Card.CARDSIZE);
            cards[i].Collider.offset = new Vector2(0, -UPMOVE/2 /Card.CARDSIZE);
        }
        while(CheckTriggers(triggers))
        {
            await Task.Yield();
        }
    }
    private bool CheckTriggers(List<bool> ts)
    {
        foreach (var b in ts)
        if(!b) return false;

        return true;
    }
    private void AddMoveUpAndDownTweens(Entity c)
    {
        c.MouseDown += MoveUp;
        c.MouseExit += MoveDown;
    }
    private void MoveUp(Entity c)
    {
        c.transform.DOLocalMoveY(UPMOVE, 0.3f);
        c.visual.render.sortingOrder = 1000;
    }
    private void MoveDown(Entity c)
    {
        c.transform.DOLocalMoveY(0, 0.3f);
        for(int i = 0; i < cards.Count; i++)
        {
            if(cards[i] == c) 
            {
                c.visual.render.sortingOrder = i;
                break;
            }
        }
    }
}
