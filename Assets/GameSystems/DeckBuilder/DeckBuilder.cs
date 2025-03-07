using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    [field: SerializeField] public DeckList characterDeck { get; private set; }
    [field: SerializeField] public DeckList lootDeck { get; private set; }
    [field: SerializeField] public DeckList monsterDeck { get; private set; }
    [field: SerializeField] public DeckList treasureDeck { get; private set; }
    [field: SerializeField] public DeckList eventDeck { get; private set; }

    public List<GameObject> GetList(DeckList source)
    {
        DeckList deck = source;
        List<GameObject> list = new List<GameObject>();
        for(int i = 0; i < deck.cardsList.Count; i++)
        {
            if(deck.cardsList[i].willAdd)
            {
                for(int j = 0; j < deck.cardsList[i].count; j++)
                {
                    list.Add(deck.cardsList[i].data);
                }
            }
        }
        return list;
    }
}
