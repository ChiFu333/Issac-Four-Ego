using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public class CardDeck : MonoBehaviour
{ 
    [field: SerializeField] public List<Entity> cards { get; private set; }
    private bool isFaceUp { get; set; } = false;
    private SpriteRenderer faceRenderer;
    public void InitDeck(List<GameObject> list, bool isFaceUp)
    {
        this.isFaceUp = isFaceUp;
        faceRenderer = gameObject.AddComponent<SpriteRenderer>();
        faceRenderer.sortingOrder = 5;
        cards = new List<Entity>(0);
        
        if(list != null)
        {
            for(int i = 0; i < list.Count; i++)
            {
                Entity c = Entity.CreateEntity(list[i], true);
                cards.Add(c);
                c.transform.parent = transform;
            }
        }
        UpdateFace();
    }
    public Entity TakeOneCard()
    {
        if(cards.Count != 0)
        {
            Entity c = cards[0];
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
    public void PutOneCardUp(Entity c) //временная перегрузка
    {
        c.SetActive(false);
        c.transform.parent = transform;
        List<Entity> templ = new List<Entity>();
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
            faceRenderer.sprite = isFaceUp ? cards[0].GetTag<CardSpritesData>().front : cards[0].GetTag<CardSpritesData>().back;
        }
    }
    public void AddAndShuffle<T>(List<GameObject> list) where T : Card
    {
        foreach(GameObject d in list)
        {
            Entity c = Entity.CreateEntity(d, true);
            cards.Add(c);
            c.transform.parent = transform;
        }
        Shuffle();
    }
}
