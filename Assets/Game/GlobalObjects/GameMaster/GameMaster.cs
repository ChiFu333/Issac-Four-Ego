using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Inst;
    
    [Header("Decks")]
    public CardDeck CharacterDeck;
    public CardDeck LootDeck, LootStash;
    public CardDeck ShopDeck;
    public CardDeck MonsterDeck;
    [Header("CardLists")]
    public CardListSO LootDeckList;
    public CardListSO LootStashList;
    public CardListSO ShopDeckList;
    public CardListSO CharacterList;
    public CardListSO MonsterList;
    public CardListSO EventList;
    //Constants
    public const int PLAYERCOUNT = 2;
    public const float CARDSPEED = 0.4f;
    public void Awake()
    {
        Inst = this;
        LootStashList = ScriptableObject.CreateInstance<CardListSO>();
        LootStashList.list = new List<CardData>();
    }
    public void Start()
    {
        InitDeck(out LootDeck, LootDeckList, false);
        InitDeck(out LootStash, LootStashList, true);
        InitDeck(out CharacterDeck, CharacterList, false);
        InitDeck(out ShopDeck, ShopDeckList, false);
        InitDeck(out MonsterDeck, MonsterList, false);

        InitShopNMonsterPlaces(); 

        AddListInDeck(MonsterDeck, EventList);

        TurnManager.Inst.Init();
    }
    public void InitDeck(out CardDeck deck,CardListSO list, bool IsFaceUp)
    {
        Dictionary<CardListSO, Vector3> posDictionary = new Dictionary<CardListSO,Vector3>
        {
            {LootDeckList, CardPlaces.Inst.LootDeck.position},
            {LootStashList, CardPlaces.Inst.LootStash.position},
            {ShopDeckList, CardPlaces.Inst.ShopDeck.position},
            {MonsterList, CardPlaces.Inst.MonsterDeck.position},
            {CharacterList, new Vector3(-10, 0, 0)}
        };
        GameObject g = new GameObject();
        g.name = "Deck";
        g.transform.position = posDictionary[list];
        g.transform.localScale = Vector3.one * Card.CARDSIZE;
        g.transform.parent = transform;
        deck = g.AddComponent<CardDeck>();
        deck.InitDeck(list.list, IsFaceUp);
        deck.Shuffle();
    }
    public void AddListInDeck(CardDeck deck,CardListSO list)
    {
        deck.AddAndShuffle(list.list);
        deck.Shuffle();
    }
    public void InitShopNMonsterPlaces()
    {
        GameObject s = new GameObject("Shop");
        s.transform.position = CardPlaces.Inst.ShopDeck.position;
        s.transform.parent = transform;
        
        Shop shop = s.AddComponent<Shop>();
        shop.Init();

        for(int i = 0; i < 2; i++)
        {
            Card m = Card.CreateCard<MonsterCard>(MonsterDeck.TakeOneCard());
            m.transform.DOMove(CardPlaces.Inst.MonsterSlots[i].position,CARDSPEED);
        }
    }
}