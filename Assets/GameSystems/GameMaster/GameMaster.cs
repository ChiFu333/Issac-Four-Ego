using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    //Constants
    public const int PLAYERCOUNT = 3;
    public const float CARDSPEED = 0.4f;
    public static GameMaster inst { get; private set; }
    //Systems
    [field: SerializeField] public Shop shop { get; private set; }
    [field: SerializeField] public MonsterZone monsterZone { get; private set; }
    [field: SerializeField] public TurnManager turnManager { get; private set; }
    [field: SerializeField] public PhaseSystem phaseSystem { get; private set; }
    [field: SerializeField] public DeckBuilder deckBuilder { get; private set; }
    //Decks
    [HideInInspector] public CardDeck characterDeck;
    [HideInInspector] public CardDeck lootDeck, lootStash;
    [HideInInspector] public CardDeck shopDeck, shopStash;
    [HideInInspector] public CardDeck monsterDeck, monsterStash;
    [Header("CardLists")]
    private CardListSO lootDeckList;
    [SerializeField] private CardListSO shopDeckList;
    [SerializeField] private CardListSO characterList;
    [SerializeField] private CardListSO monsterList;
    [SerializeField] private CardListSO eventList;
    private CardListSO lootStashList, monsterStashList, shopStashList;
    public void Awake()
    {
        inst = this;
        lootDeckList = deckBuilder.GetLootList();
        lootStashList = ScriptableObject.CreateInstance<CardListSO>();
        monsterStashList = ScriptableObject.CreateInstance<CardListSO>();
        shopStashList = ScriptableObject.CreateInstance<CardListSO>();
    }
    public void Start()
    {
        CardPlaces.inst.Init();
        
        InitDeck<CharacterCard>(out characterDeck, characterList, false);

        InitDeck<LootCard>(out lootDeck, lootDeckList, false);
        InitDeck<LootCard>(out lootStash, lootStashList, true);

        InitDeck<ItemCard>(out shopDeck, shopDeckList, false);
        InitDeck<ItemCard>(out shopStash, shopStashList, true);

        InitDeck<MonsterCard>(out monsterDeck, monsterList, false);        
        InitDeck<MonsterCard>(out monsterStash, monsterStashList, true);

        InitShopNMonsterPlaces(); 

        AddListInDeck(monsterDeck, eventList);

        turnManager.Init();
    }
    private void InitDeck<T>(out CardDeck deck,CardListSO list, bool IsFaceUp) where T : Card
    {
        Dictionary<CardListSO, Vector3> posDictionary = new Dictionary<CardListSO,Vector3>
        {
            {lootDeckList, CardPlaces.inst.lootDeck.position},
            {lootStashList, CardPlaces.inst.lootStash.position},
            
            {shopDeckList, CardPlaces.inst.shopDeck.position},
            {shopStashList, CardPlaces.inst.shopStash.position},
            
            {monsterList, CardPlaces.inst.monsterDeck.position},
            {monsterStashList, CardPlaces.inst.monsterStash.position},

            {characterList, new Vector3(-10, 0, 0)}
        };
        Dictionary<CardListSO, string> deckName = new Dictionary<CardListSO, string>
        {
            {lootDeckList, "LootDeck"},
            {lootStashList, "LootStash"},

            {shopDeckList, "ShopDeck"},
            {shopStashList, "ShopStash"},
            
            {monsterList, "MonsterDeck"},
            {monsterStashList, "MonsterStash"},
            
            {characterList, "CharacterDeck"}
        };
        GameObject g = new GameObject(deckName[list]);
        g.transform.position = posDictionary[list];
        g.transform.localScale = Vector3.one * Card.CARDSIZE;
        g.transform.parent = transform;
        deck = g.AddComponent<CardDeck>();
        deck.InitDeck<T>(list.list, IsFaceUp);
        deck.Shuffle();
    }
    private void AddListInDeck(CardDeck deck,CardListSO list)
    {
        deck.AddAndShuffle<EventCard>(list.list); //ВООБЩЕ КРИВАЯ ШТУКА !!!
        deck.Shuffle();
    }
    private void InitShopNMonsterPlaces()
    {
        GameObject s = new GameObject("Shop");
        s.transform.position = CardPlaces.inst.shopDeck.position;
        s.transform.parent = transform;
        
        Shop sh = s.AddComponent<Shop>();
        shop = sh;
        sh.Init();

        GameObject m = new GameObject("MonsterZone");
        m.transform.position = CardPlaces.inst.monsterDeck.position;
        m.transform.parent = transform;
        
        MonsterZone mz = m.AddComponent<MonsterZone>();
        monsterZone = mz;
        mz.Init();
    }
}