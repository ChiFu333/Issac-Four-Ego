using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public class CardDeck : MonoBehaviour
{ 
    [field: SerializeField] public List<Entity> cards { get; private set; }
    
    private SpriteRenderer _faceRenderer;
    private bool _isFaceUp = false;
    
    public void InitDeck(List<GameObject> list, bool isUp)
    {
        _isFaceUp = isUp;
        _faceRenderer = gameObject.AddComponent<SpriteRenderer>();
        _faceRenderer.sortingOrder = 5;
        cards = new List<Entity>(0);
        
        if(list != null)
        {
            foreach (var t in list)
            {
                Entity c = Entity.CreateEntity(t, true);
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
        c.transform.position = transform.position;
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

    public void PutOneCardUnder(Entity c)
    {
        c.SetActive(false);
        c.transform.parent = transform;
        c.transform.position = transform.position;
        cards.Add(c);
        UpdateFace();
    }
    
    public void Shuffle()
    {
        for (int i = cards.Count - 1; i >= 1; i--)
        {
            int j = Random.Range(0,i + 1);
            // обменять значения data[j] и data[i]
            (cards[j], cards[i]) = (cards[i], cards[j]);
        }
        UpdateFace();
    }
    
    private void UpdateFace()
    {
        if(cards.Count == 0)
        {
            _faceRenderer.sprite = null;
        }
        else
        {
            _faceRenderer.sprite = _isFaceUp ? cards[0].GetTag<CardSpritesData>().front : cards[0].GetTag<CardSpritesData>().back;
        }
    }
    
    public void AddAndShuffle(List<GameObject> list)
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
