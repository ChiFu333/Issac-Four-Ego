using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    [field: SerializeField] public DeckList lootDeck { get; private set; }
    [field: SerializeField] public DeckList monsterDeck { get; private set; }
    [field: SerializeField] public DeckList treasureDeck { get; private set; }

    public CardListSO GetLootList()
    {
        DeckList deck = lootDeck;
        CardListSO list = ScriptableObject.CreateInstance<CardListSO>();
        list.Init();
        for(int i = 0; i < deck.cardsList.Count; i++)
        {
            if(lootDeck.cardsList[i].willAdd)
            {
                for(int j = 0; j < deck.cardsList[i].count; j++)
                {
                    list.list.Add(deck.cardsList[i].data);
                }
            }
        }
        return list;
    }
}
