using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class CardDeck : MonoBehaviour
{ 
    public List<CardData> cards { get; private set; }
    private bool isFaceUp { get; set; } = false;
    private SpriteRenderer faceRenderer;
    public void InitDeck(List<CardData> list, bool isFaceUp)
    {
        this.isFaceUp = isFaceUp;
        faceRenderer = gameObject.AddComponent<SpriteRenderer>();
        faceRenderer.sortingOrder = 5;
        cards = new List<CardData>(0);
        if(list != null)
        {
            for(int i = 0; i < list.Count; i++)
            {
                cards.Add(list[i]);
            }
        }
        UpdateFace();
    }
    public CardData TakeOneCard()
    {
        if(cards.Count != 0)
        {
            CardData d = cards[0];
            cards.RemoveAt(0);
            UpdateFace();
            return d;
        }
        else
        {
            return null;
        }
    }
    public void PutOneCardUp(Card c)
    {
        List<CardData> templ = new List<CardData>();
        for(int i = 0; i < cards.Count+1; i++)
        {
            if(i == 0) 
            {
                templ.Add(c.data);
            }
            else
            {
                templ.Add(cards[i-1]);
            }
        }
        cards = templ;
        c.DestroyMe();
        UpdateFace();
    }
    public void PutOneCardUp(CardData cd) //временная перегрузка
    {
        List<CardData> templ = new List<CardData>();
        for(int i = 0; i < cards.Count+1; i++)
        {
            if(i == 0) 
            {
                templ.Add(cd);
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
    public void UpdateFace()
    {
        if(cards.Count == 0)
        {
            faceRenderer.sprite = null;
        }
        else
        {
            faceRenderer.sprite = isFaceUp ? cards[0].face : cards[0].back;
        }
    }
    public void AddAndShuffle(List<CardData> list)
    {
        cards.AddRange(list);
        Shuffle();
    }
}
