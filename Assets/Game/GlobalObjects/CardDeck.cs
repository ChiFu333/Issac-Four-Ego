using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class CardDeck : MonoBehaviour
{ 
    public List<CardData> Cards;
    public bool IsFaceUp = false;
    private SpriteRenderer FaceRenderer;
    public void InitDeck(List<CardData> list, bool isFaceUp)
    {
        IsFaceUp = isFaceUp;
        FaceRenderer = gameObject.AddComponent<SpriteRenderer>();
        FaceRenderer.sortingOrder = 5;
        Cards = new List<CardData>(0);
        if(list != null)
        {
            for(int i = 0; i < list.Count; i++)
            {
                Cards.Add(list[i]);
            }
        }
        UpdateFace();
    }
    public CardData TakeOneCard()
    {
        if(Cards.Count != 0)
        {
            CardData d = Cards[0];
            Cards.RemoveAt(0);
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
        for(int i = 0; i < Cards.Count+1; i++)
        {
            if(i == 0) 
            {
                templ.Add(c.data);
            }
            else
            {
                templ.Add(Cards[i-1]);
            }
        }
        Cards = templ;
        c.DestroyMe();
        UpdateFace();
    }
    public void Shuffle()
    {
        for (int i = Cards.Count - 1; i >= 1; i--)
        {
            int j = Random.Range(0,i + 1);
            // обменять значения data[j] и data[i]
            var temp = Cards[j];
            Cards[j] = Cards[i];
            Cards[i] = temp;
        }
        UpdateFace();
    }
    public void UpdateFace()
    {
        if(Cards.Count == 0)
        {
            FaceRenderer.sprite = null;
        }
        else
        {
            FaceRenderer.sprite = IsFaceUp ? Cards[0].face : Cards[0].back;
        }
    }
    public void AddAndShuffle(List<CardData> list)
    {
        Cards.AddRange(list);
        Shuffle();
    }
}
