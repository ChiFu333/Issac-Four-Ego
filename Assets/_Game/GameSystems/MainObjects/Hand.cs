using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public float deltaCard = Card.CARDPIXELSIZE.x * Card.CARDSIZE; // 1.13 и 0.28 для стандарта Card.CARDSIZE.x * GameMaster.CARDSIZE;

    private const float MAXLENGTH = 6.5f;
    public const float UPMOVE = 0.4f;
    [field: SerializeField] public List<Entity> cards { get; private set; } = new List<Entity>();

    public async Task AddCard(Entity card)
    {
        card.AddTag(new PlayFromHand());
        AudioManager.inst.Play(R.Audio.cardTaked);
        card.visual.render.sortingLayerName = "HandCards"; //Слой для рук!!
        cards.Add(card);

        card.transform.SetParent(transform);
        card.transform.localScale = Vector3.one * Card.CARDSIZE;

        await UpdateCardPositions();

        //UIOnDeck.inst.UpdateTexts(G.Players.GetPlayerId(GetComponentInParent<Player>()));
    }

    public async Task PlayCard(Entity card)
    {
        
        card.visual.transform.eulerAngles = Vector3.zero;
        card.visual.transform.localScale = Vector3.one;
        card.visual.DOKill(true);
        
        card.RemoveTag(card.GetTag<PlayFromHand>());
        
        ExitCardFromHand(card);
        card.visual.transform.localPosition = Vector3.zero;
        
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
        cards.Remove(card);

        UIOnDeck.inst.UpdateTexts();
        _ = UpdateCardPositions();
    }

    private async Task UpdateCardPositions()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].visual.render.sortingOrder = i;
        }

        List<bool> triggers = new List<bool>();
        for (int i = 0; i < cards.Count; i++)
        {
            bool aLot = ((cards.Count - 1) * deltaCard / 2f) > MAXLENGTH / 2;
            float delCard = aLot ? MAXLENGTH / (cards.Count - 1) : deltaCard;
            float dStart = -(cards.Count - 1) * delCard / 2f;
            float localXPos = dStart + i * delCard;

            triggers.Add(false);
            int t = i;
            _ = cards[i].MoveToForHand(transform.TransformPoint(new Vector3(localXPos, 0)),
                aLot ? GetAngleInHand(cards[i]) : 0, () => triggers[t] = true, false);
            cards[i].visual.transform.DOLocalMoveY(GetDeltaYInHand(cards[i]), 0.3f);

            cards[i].Collider.size =
                new Vector2(delCard / Card.CARDSIZE, Card.CARDPIXELSIZE.y + UPMOVE / Card.CARDSIZE);
            cards[i].Collider.offset = new Vector2(0, -UPMOVE / 2 / Card.CARDSIZE);
        }

        while (CheckTriggers(triggers))
        {
            await Task.Yield();
        }
    }

    private bool CheckTriggers(List<bool> ts)
    {
        foreach (var b in ts)
            if (!b)
                return false;
        return true;
    }

    public int GetMySortingOrder(Entity c)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == c)
            {
                return i;
            }
        }

        return -1;
    }
    private AnimationCurve positioning = new AnimationCurve
    (
        new Keyframe(0f, 0f), 
        new Keyframe(0.08333334f, 0.4043479f), 
        new Keyframe(0.1666667f, 0.6431358f), 
        new Keyframe(0.25f, 0.8037757f), 
        new Keyframe(0.3333333f, 0.9118787f), 
        new Keyframe(0.4166667f, 0.9769853f), 
        new Keyframe(0.5f, 1f), 
        new Keyframe(0.5833333f, 0.9767235f), 
        new Keyframe(0.6666667f, 0.9109904f), 
        new Keyframe(0.75f, 0.8020976f), 
        new Keyframe(0.8333333f, 0.6407319f), 
        new Keyframe(0.9166667f, 0.4017494f), 
        new Keyframe(1f, 0f)
    );
    private float positioningInfluence = .025f;
    private AnimationCurve rotation = new AnimationCurve(
        new Keyframe(0f, 1f), // Время 0, значение 10
        new Keyframe(1f, -1f) // Время 0.33, значение 0.5
    );
    private float rotationInfluence = 0.25f;
    
    public float GetDeltaYInHand(Entity card)
    {
        
        float delta = positioning.Evaluate(GetNormalPosition(card)) * (cards.Count * positioningInfluence);
        float curveYOffset = cards.Count < 5 ? 0 : delta;

        return curveYOffset;
    }
    private float GetAngleInHand(Entity c)
    {
        float t = rotation.Evaluate(GetNormalPosition(c)) * (rotationInfluence * cards.Count);
        if(float.IsNaN(t)) t = 0;
        return t;
    }

    private float GetNormalPosition(Entity card)
    {
        float normalizedPos = 0;
        for (float i = 0; i < cards.Count; i++)
        {
            if (cards[(int)i] == card)
            {
                normalizedPos = i / (cards.Count - 1);
                break;
            }
        }

        return normalizedPos;
    }
}
