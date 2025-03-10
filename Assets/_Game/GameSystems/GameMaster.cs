using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameMaster : MonoBehaviour
{
    //Удалить эту штуку
    public static GameMaster inst { get; private set; }
    
    [Header("StartValues")]
    [SerializeField] private int startLootCount;
    [SerializeField] private int playerCount;
    
    [Header("CardLists")] 
    [field: SerializeField] private DeckList characterDeck;
    [field: SerializeField] private DeckList lootDeck;
    [field: SerializeField] private DeckList monsterDeck;
    [field: SerializeField] private DeckList treasureDeck;
    [field: SerializeField] private DeckList eventDeck;
    
    //Constants
    public const float CARD_SPEED = 0.4f;
    
    //Systems
    [field: SerializeField] private CardPlaces cardPlaces;
    [field: SerializeField] public PhaseSystem phaseSystem { get; private set; }
    
    private GameObject _deckHolder, _zoneHolder;
    
    public void Awake()
    {
        inst = this;
        _deckHolder = new GameObject("Deck_Holder");
        //zoneHolder = new GameObject("Zone_Holder");
        R.Init();
    }
    public async UniTaskVoid Start()
    {
        cardPlaces.Init();
        
        G.Decks.characterDeck = InitDeck(characterDeck.GetList(), new Vector3(-10, 0, 0), "CharacterDeck", false);
        G.Decks.lootDeck = InitDeck(lootDeck.GetList(), G.CardPlaces.lootDeck.position, "LootDeck", false);
        G.Decks.lootStash = InitDeck(new List<GameObject>(), G.CardPlaces.lootStash.position, "LootStash", true);
        G.Decks.treasureDeck = InitDeck(treasureDeck.GetList(), G.CardPlaces.treasureDeck.position, "TreasureDeck", false);
        G.Decks.treasureStash = InitDeck(new List<GameObject>(), G.CardPlaces.treasureStash.position, "TreasureStash", true);
        G.Decks.monsterDeck = InitDeck(monsterDeck.GetList(), G.CardPlaces.monsterDeck.position, "MonsterDeck", false);
        G.Decks.monsterStash = InitDeck(new List<GameObject>(), G.CardPlaces.monsterStash.position, "MonsterStash", true);
        
        G.shop = InitShop();
        G.monsterZone = InitMonsterZone();

        AddListInDeck(G.Decks.monsterDeck, eventDeck.GetList());

        G.Players.players = InitAllPlayers();
        G.Players.playerTurnId = 0;
        G.Players.priorId = 0;

        G.CardSelector = new CardSelector();
        //StartGame
        for(int i = 0; i < G.Players.players.Count; i++)
        {
            Player player = G.Players.players[i];

            G.Players.priorId = i;
            await player.Init(InitHand(player));
            for(int j = 0; j < startLootCount; j++)
            {
                Entity c = G.Decks.lootDeck.TakeOneCard();
                player.TakeOneLootCard(c);
                await UniTask.Delay(20);
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
    
    private CardDeck InitDeck(List<GameObject> list, Vector3 position, string n, bool isFaceUp)
    {
        GameObject g = new GameObject(n)
        {
            transform =
            {
                position = position,
                localScale = Vector3.one * Card.CARDSIZE,
                parent = _deckHolder.transform
            }
        };

        CardDeck deck = g.AddComponent<CardDeck>();
        deck.InitDeck(list, isFaceUp);
        deck.Shuffle();
        return deck;
    }
    private void AddListInDeck(CardDeck deck,List<GameObject> list)
    {
        deck.AddAndShuffle(list); //ВООБЩЕ КРИВАЯ ШТУКА !!!
        deck.Shuffle();
    }
    
    private Shop InitShop()
    {
        GameObject s = new GameObject("Shop")
        {
            transform =
            {
                position = G.CardPlaces.treasureDeck.position
            }
        };

        Shop sh = s.AddComponent<Shop>();
        sh.Init();
        return sh;
    }
    private MonsterZone InitMonsterZone()
    {
        GameObject m = new GameObject("MonsterZone")
        {
            transform =
            {
                position = G.CardPlaces.monsterDeck.position
            }
        };

        MonsterZone mz = m.AddComponent<MonsterZone>();
        mz.Init();
        return mz;
    }
    
    private List<Player> InitAllPlayers()
    {
        List<Player> playersList = new List<Player>();
        for(int j = playerCount; j < 4; j++)
        {
            G.CardPlaces.playersTransformToDeconstruct[j].gameObject.SetActive(false);
        }
        for(int i = 0; i < playerCount; i++)
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
        if(i != 0) h.transform.localScale = new Vector3(1,-1, 1);
        Hand ha = h.AddComponent<Hand>();
        ha.transform.position = G.CardPlaces.hands[i].position;
        ha.transform.parent = player.transform;
        ha.transform.localScale = G.CardPlaces.hands[i].lossyScale;

        return ha;
    }
    
    public void HealEveryone()
    {
        foreach (var t in G.Players.players)
        {
            t.HealHp(100, true);
            t.SetBaseStats();
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
        public static CardDeck treasureDeck, treasureStash;
    }
    public static Shop shop;
    public static MonsterZone monsterZone;
    public static class Players
    {
        public static List<Player> players;
        public static int playerTurnId;
        public static int priorId;
        public static Player activePlayer => players[playerTurnId];
        public static Player priorPlayer => players[playerTurnId]; 
        public static int GetPlayerId(Player p)
        {
            for(int i = 0; i < players.Count; i++)
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
