using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;

public class GameMaster : MonoBehaviour
{
    //Constants
    public const int PLAYERCOUNT = 3;
    public const float CARDSPEED = 0.4f;
    public static GameMaster inst { get; private set; }
    //Systems
    [field: SerializeField] public CardPlaces cardPlaces;
    [field: SerializeField] public PhaseSystem phaseSystem { get; private set; }
    [field: SerializeField] public DeckBuilder deckBuilder { get; private set; }
    //Decks
    [Header("CardLists")]
    private List<GameObject> lootDeckList;
    private List<GameObject> shopDeckList;
    private List<GameObject> characterList;
    private List<GameObject> monsterList;
    private List<GameObject> eventList;
    private List<GameObject> lootStashList, monsterStashList, shopStashList;
    private GameObject deckHolder, zoneHolder;
    public void Awake()
    {
        inst = this;
        deckHolder = new GameObject("Deck_Holder");
        //zoneHolder = new GameObject("Zone_Holder");

        characterList = deckBuilder.GetList(deckBuilder.characterDeck);
        lootDeckList = deckBuilder.GetList(deckBuilder.lootDeck);
        shopDeckList = deckBuilder.GetList(deckBuilder.treasureDeck);
        monsterList = deckBuilder.GetList(deckBuilder.monsterDeck);
        eventList = deckBuilder.GetList(deckBuilder.eventDeck);

        lootStashList = new List<GameObject>();
        monsterStashList = new List<GameObject>();
        shopStashList = new List<GameObject>();
    }
    public async void Start()
    {
        cardPlaces.Init();

        G.Decks.characterDeck = InitDeck(characterList, false);
        G.Decks.lootDeck = InitDeck(lootDeckList, false);
        G.Decks.lootStash = InitDeck(lootStashList, true);
        G.Decks.shopDeck = InitDeck(shopDeckList, false);
        G.Decks.shopStash = InitDeck(shopStashList, true);
        G.Decks.monsterDeck = InitDeck(monsterList, false); 
        G.Decks.monsterStash = InitDeck(monsterStashList, true);

        G.shop = InitShop();
        G.monsterZone = InitMonsterZone();

        AddListInDeck(G.Decks.monsterDeck, eventList);

        G.Players.players = InitAllPlayers();
        G.Players.playerTurnId = 0;
        G.Players.priorId = 0;

        for(int i = 0; i < G.Players.players.Count; i++)
        {
            Player player = G.Players.players[i];

            G.Players.priorId = i;
            await player.Init(InitHand(player));
            for(int j = 0; j < 3; j++)
            {
                Entity c = G.Decks.lootDeck.TakeOneCard();
                player.TakeOneLootCard(c);
                await Task.Delay(100);
            }
            player.AddMoney(3);
        }

        UIOnDeck.inst.UpdateTexts();
        HealEveryone();
        
        phaseSystem.StartStartTurn();
    }
    public void SwitchTurn()
    {
        G.Players.playerTurnId++;
        G.Players.playerTurnId %= G.Players.players.Count;

        G.Players.priorId = G.Players.playerTurnId;
        GameMaster.inst.phaseSystem.StartStartTurn();
    }
    public void EndTurn()
    {
        
        _ = GameMaster.inst.phaseSystem.StartEndPhase();
    }
    private CardDeck InitDeck(List<GameObject> list, bool IsFaceUp)
    {
        Dictionary<List<GameObject>, Vector3> posDictionary = new Dictionary<List<GameObject>, Vector3>
        {
            {lootDeckList, G.CardPlaces.lootDeck.position},
            {lootStashList, G.CardPlaces.lootStash.position},
            
            {shopDeckList, G.CardPlaces.shopDeck.position},
            {shopStashList, G.CardPlaces.shopStash.position},
            
            {monsterList, G.CardPlaces.monsterDeck.position},
            {monsterStashList, G.CardPlaces.monsterStash.position},

            {characterList, new Vector3(-10, 0, 0)}
        };
        Dictionary<List<GameObject>, string> deckName = new Dictionary<List<GameObject>, string>
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
        g.transform.parent = deckHolder.transform;

        CardDeck deck = g.AddComponent<CardDeck>();
        deck.InitDeck(list, IsFaceUp);
        deck.Shuffle();
        return deck;
    }
    private void AddListInDeck(CardDeck deck,List<GameObject> list)
    {
        deck.AddAndShuffle<EventCard>(list); //ВООБЩЕ КРИВАЯ ШТУКА !!!
        deck.Shuffle();
    }
    private Shop InitShop()
    {
        GameObject s = new GameObject("Shop");
        s.transform.position = G.CardPlaces.shopDeck.position;
        
        Shop sh = s.AddComponent<Shop>();
        sh.Init();
        return sh;
    }
    private MonsterZone InitMonsterZone()
    {
        GameObject m = new GameObject("MonsterZone");
        m.transform.position = G.CardPlaces.monsterDeck.position;
        
        MonsterZone mz = m.AddComponent<MonsterZone>();
        mz.Init();
        return mz;
    }
    private List<Player> InitAllPlayers()
    {
        List<Player> playersList = new List<Player>();
        for(int j = GameMaster.PLAYERCOUNT; j < 4; j++)
        {
            G.CardPlaces.playersTransformToDeconstruct[j].gameObject.SetActive(false);
        }
        for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
        {
            //Создание объекта игрока
            GameObject p = new GameObject();
            Player player = p.AddComponent<Player>();
            playersList.Add(player);
            p.name = "Player " + i;  
        }
        return playersList;
    }
    private Hand InitHand(Player player)
    {
        int i = G.Players.GetPlayerId(player);

        GameObject h = new GameObject("Hand");
        if(i != 0) h.transform.Rotate(0, 0, 180);
        Hand ha = h.AddComponent<Hand>();
        ha.transform.position = G.CardPlaces.hands[i].position;
        ha.transform.parent = player.transform;
        ha.transform.localScale = G.CardPlaces.hands[i].lossyScale;

        return ha;
    }
    public void HealEveryone()
    {
        for(int i = 0; i < G.Players.players.Count; i++)
        {
            G.Players.players[i].HealHp(100, true);
            G.Players.players[i].SetBaseStats();
        }
    }
}
public static partial class G
{
    public static class Decks
    {
        public static CardDeck characterDeck;
        public static CardDeck lootDeck, lootStash;
        public static CardDeck monsterDeck, monsterStash;
        public static CardDeck shopDeck, shopStash;
    }
    public static Shop shop;
    public static MonsterZone monsterZone;
    public static class Players
    {
        public static List<Player> players;
        public static int playerTurnId;
        public static int priorId;
        public static Player activePlayer { get => players[playerTurnId];}
        public static Player priorPlayer { get => players[playerTurnId];} 
        public static int GetPlayerId(Player p)
        {
            for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
            {
                if(players[i] == p) return i;
            }
            return -1;
        }
        public static void SetPrior(Player player)
        {
            for(int i = 0; i < players.Count; i++)
            {
                if(players[i] == player) 
                {
                    priorId = i;
                    return;
                }
            }
        }
        public static void RestorePrior()
        {
            priorId = playerTurnId;
        }
        public static bool IsMyTurn()
        {
            return playerTurnId == 0;
        }
    }
}
