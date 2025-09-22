using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Runtime;
using UnityEngine.Rendering.Universal;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class Deck : MonoBehaviour, GameView, ISelectableTarget
{
    [field: SerializeField] public List<Card> cards { get; private set; }

    public SpriteRenderer faceRenderer;
    public BoxCollider2D collider;
    public bool isFaceUp = false;
    private Vector3 basePlace;
    private int baseOrder = 20;
    public List<Card> InitDeck(List<CardInDeckInfo> list, bool isUp)
    {
        isFaceUp = isUp;
        InitFace();
        cards = new List<Card>();

        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].count; j++)
                {
                    Card cardView = Card.CreateCard(list[i].entity.AsEntity().DeepCopy(), isFaceUp);
                    cardView.transform.parent = transform;
                    cards.Add(cardView);
                }
            }
        }

        basePlace = transform.position;
        Shuffle();
        UpdateFace();
        return cards.ToList();
    }
    public Card TakeOneCard(int i, bool openCard)
    {
        if (cards.Count != 0)
        {
            G.AudioManager.PlaySound(R.Audio.CardTaked, 0.3f * i);
            Card c = cards[0];
            c.transform.position = GetMyPos(false);
            cards.RemoveAt(0);
            UpdateFace();
            c.SetActive(true);
            if(openCard) _ = c.Flip(true);
            return c;
        }
        else
        {
            return null;
        }
    }
    public void PutOneCardUp(Card c)
    {
        PutSetup(c);

        cards = cards.Prepend(c).ToList(); 
        UpdateFace();
    }
    public void PutOneCardUnder(Card c)
    {
        PutSetup(c);
        cards.Add(c);
        UpdateFace();
    }

    private void PutSetup(Card c)
    {
        c.SetActive(false);
        c.transform.SetParent(transform);
        c.transform.localPosition = Vector3.zero;
        c.RestoreLit();
    }
    public void Shuffle()
    {
        for (int i = cards.Count - 1; i >= 1; i--)
        {
            int j = Random.Range(0, i + 1);
            // обменять значения data[j] и data[i]
            (cards[j], cards[i]) = (cards[i], cards[j]);
        }

        UpdateFace();
    }

    public Card FindAndGetCard(Sprite s)
    {
        Card c = cards.FirstOrDefault(x => x.Get<TagSprite>().sprite == s);
        cards.Remove(c);
        UpdateFace();
        c.SetActive(true);
        _ = c.Flip(true);
        return c;
    }

    private void InitFace()
    {
        GameObject face = new GameObject("Face");
        face.transform.parent = transform;
        face.transform.localPosition = Vector3.zero;
        face.transform.localScale = Vector3.one;
        faceRenderer = face.AddComponent<SpriteRenderer>();
        collider = faceRenderer.gameObject.AddComponent<BoxCollider2D>();
        collider.size = Card.CARDPIXELSIZE;
        faceRenderer.sortingOrder = baseOrder;
        faceRenderer.material = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<DeckListConfig>())!.Get<DeckListConfig>().cardMaterial;
        GenerateShadowCards();
    }

    private void UpdateFace()
    {
        for (int i = 0; i < shadowCards.Count; i++)
        {
            shadowCards[i].SetActive(cards.Count > i * betweenShadowCardsCouns);
        }

        ChangeShadowSprite();
        if (cards.Count == 0)
        {
            faceRenderer.sprite = null;
            return;
        }

        faceRenderer.sprite = isFaceUp
            ? cards[0].Get<TagSprite>().sprite
            : cards[0].Get<TagCardType>().cardType.GetBackSprite();
        faceRenderer.transform.localPosition = new Vector3(0, DeltaOneCard() * Mathf.Min(cards.Count, 100));

    }

    public async UniTask MoveTo(Vector3 pos, float scale, int plusOrder, bool changeOrderAfter)
    {
        if (!changeOrderAfter)
        {
            faceRenderer.sortingOrder = baseOrder + plusOrder;
            UpdateShadowCards(plusOrder);
        }
        transform.DOScale(Card.CARDSIZE * scale, 0.75f).SetEase(Ease.InOutExpo);
        await transform.DOMove(pos, 0.75f).SetEase(Ease.InOutExpo).AsyncWaitForCompletion().AsUniTask();
        if (changeOrderAfter)
        {
            faceRenderer.sortingOrder = baseOrder + plusOrder;
            UpdateShadowCards(plusOrder);
        }
    }

    public async UniTask ReturnToBasePos()
    {
        await MoveTo(basePlace, 1, 0, true);
    }
    public Vector3 GetMyPos(bool bottomPos)
    {
        if (cards.Count == 0 || bottomPos)
        {
            return transform.position; // Если колода пуста, возвращаем позицию колоды
        }
        return faceRenderer.transform.position;
    }
    [HideInInspector] public List<GameObject> shadowCards = new();
    private int shadowsCardsCount = 8; // высота теней в штуках (т.е. если штук 20, то до в два раза выше)
    private int betweenShadowCardsCouns = 15;
    private float deltaUp = 0.3f;
    public float DeltaOneCard()
    {
        return deltaUp / betweenShadowCardsCouns;
    }
    private void GenerateShadowCards()
    {
        for (int i = 0; i < shadowsCardsCount; i++)
        {
            GameObject face = new GameObject("ShadowCard - " + i);
            face.transform.parent = transform;
            face.transform.localPosition = new Vector3(0, i * deltaUp);
            face.transform.localScale = Vector3.one;
            SpriteRenderer sr = face.AddComponent<SpriteRenderer>();
            sr.material = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<DeckListConfig>())!.Get<DeckListConfig>().cardMaterial;
            sr.sprite = CardType.lootCard.GetBackSprite();
            sr.sortingOrder = 5 + i;
            int c = 90;
            sr.color = new Color(c/256f, c/256f, c/256f);
            shadowCards.Add(face);
        }
    }

    private void UpdateShadowCards(int plusOrder)
    {
        for (int i = 0; i < shadowsCardsCount; i++)
        {
            SpriteRenderer sr = shadowCards[i].GetComponent<SpriteRenderer>();
            sr.sortingOrder = 5 + i + plusOrder;
        }
    }
    private void ChangeShadowSprite()
    {
        foreach (var c in shadowCards)
        {
            if (cards.Count != 0)
            {
                c.GetComponent<SpriteRenderer>().sprite = cards[0].Get<TagCardType>().cardType.GetBackSprite();
            }
            else
            {
                c.GetComponent<SpriteRenderer>().sprite = CardType.lootCard.GetBackSprite();
            }
        }
    }

    public void SetLit(float amount)
    {
        faceRenderer.material.SetFloat("_LitAmount", amount);
        shadowCards.ForEach(x => x.GetComponent<SpriteRenderer>().material.SetFloat("_LitAmount", amount));
    }
}
