using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class CardDeck : MonoBehaviour
{ 
    [field: SerializeField] public List<Card> cards { get; private set; }
    private bool isFaceUp { get; set; } = false;
    private SpriteRenderer faceRenderer;
    public void InitDeck<T>(List<CardData> list, bool isFaceUp) where T : Card
    {
        this.isFaceUp = isFaceUp;
        faceRenderer = gameObject.AddComponent<SpriteRenderer>();
        faceRenderer.sortingOrder = 5;
        cards = new List<Card>(0);
        
        if(list != null)
        {
            for(int i = 0; i < list.Count; i++)
            {
                Card c = Card.CreateCard<T>(list[i], true);
                cards.Add(c);
                c.transform.parent = transform;
            }
        }
        UpdateFace();
    }
    public Card TakeOneCard()
    {
        if(cards.Count != 0)
        {
            Card c = cards[0];
            cards.RemoveAt(0);
            UpdateFace();
            c.SetActive(true);
            return c;
        }
        else
        {
            return null;
        }
    }
    public void PutOneCardUp(Card c) //временная перегрузка
    {
        c.SetActive(false);
        c.transform.parent = transform;
        List<Card> templ = new List<Card>();
        for(int i = 0; i < cards.Count+1; i++)
        {
            if(i == 0) 
            {
                templ.Add(c);
            }
            else
            {
                templ.Add(cards[i-1]);
            }
        }
        cards = templ;
        UpdateFace();
    }
    public void Shuffle()
    {
        for (int i = cards.Count - 1; i >= 1; i--)
        {
            int j = Random.Range(0,i + 1);
            // обменять значения data[j] и data[i]
            var temp = cards[j];
            cards[j] = cards[i];
            cards[i] = temp;
        }
        UpdateFace();
    }
    private void UpdateFace()
    {
        if(cards.Count == 0)
        {
            faceRenderer.sprite = null;
        }
        else
        {
            faceRenderer.sprite = isFaceUp ? cards[0].GetData<CardData>().face : cards[0].GetData<CardData>().back;
        }
    }
    public void AddAndShuffle<T>(List<CardData> list) where T : Card
    {
        foreach(CardData d in list)
        {
            Card c = Card.CreateCard<T>(d, true);
            cards.Add(c);
            c.transform.parent = transform;
        }
        Shuffle();
    }
}
