using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

[Serializable]
public class MainInitSettings : EntityComponentDefinition
{
    [Header("Players & Loots")] 
    public int playerCount;
    public int startLootCount;
    [Header("Show Loot")] 
    public bool ShowOthersLootCards;
    [Header("Override Start Cards")]
    public CMSEntityPfb DebugOverrideStartCharacter;
    public List<CMSEntityPfb> DebugGiveThisItemFromStart;
}
[Serializable]
public class DeckListConfig : EntityComponentDefinition
{
    public GameObject CardView;
    public Material cardMaterial;
    public GameObject CubeView;
    public List<CardInDeckInfo> characterDeckData;
    public List<CardInDeckInfo> lootDeckData;
    public List<CardInDeckInfo> treasureDeckData;
    public List<CardInDeckInfo> monsterDeckData;
}

[Serializable]
public class CardInDeckInfo
{
    [SerializeField]  
    public CMSEntityPfb entity;
    public int count = 1;
}
public class GameMain : MonoBehaviour
{
    public MainInitSettings MainInitSettings;

    public Player ActivePlayer;
    public List<Player> Players;
    public List<Card> AllCards = new();
    
    public Decks Decks;
    public Shop Shop;

    [HideInInspector] public CardPlaces CardPlaces;
    [HideInInspector] public PhaseController PhaseController;
    [HideInInspector] public TurnManager TurnManager;
    [HideInInspector] public StackSystem StackSystem;
    [HideInInspector] public TriggeredEffectsSystem TriggeredEffectsSystem;
    
    [HideInInspector] public TableUI TableUI;
        
    [HideInInspector] public ActionChecker ActionChecker;
    [HideInInspector] public CardSelector CardSelector;
    [HideInInspector] public CubeThrower CubeThrower;
    [HideInInspector] public EffectSelector EffectSelector;
    [HideInInspector] public CardWatcher CardWatcher;
    [HideInInspector] public GameZones GameZones;

    private void Awake()
    {
        Init();
    }

    public async void Init()
    {
        //Take Configs
        DeckListConfig config = CMS.GetOnlyOneComponent<DeckListConfig>();
        MainInitSettings = CMS.GetOnlyOneComponent<MainInitSettings>();
        
        //Init
        TableUI = FindFirstObjectByType<TableUI>();
        CardPlaces = FindFirstObjectByType<CardPlaces>();
        StackSystem = FindFirstObjectByType<StackSystem>();
        ActionChecker = new ActionChecker();
        CardSelector = new CardSelector();
        PhaseController = new PhaseController();
        CubeThrower = FindFirstObjectByType<CubeThrower>();
        EffectSelector = FindFirstObjectByType<EffectSelector>();
        CardWatcher = FindFirstObjectByType<CardWatcher>(); 
        GameZones = FindFirstObjectByType<GameZones>();
        
        CardPlaces.Init();
        
        //Init Game Objects
        InitAllDecks(config);
        InitAllPlayers(MainInitSettings.playerCount);
        Shop = new Shop();
        Shop.Init();
        await UniTask.Delay(200);
        
        
        UniTask task = UniTask.CompletedTask;
        for (int j = 0; j < MainInitSettings.playerCount; j++)
        {
            task = Players[j].Init(j, MainInitSettings);
        }
        
        GainLootCardsAction LootGainAction = new GainLootCardsAction { count = MainInitSettings.startLootCount };
        LootGainAction.Execute(null, Players.PackPlayers()).Forget();
        await task;
        await UniTask.Delay(500); 
        
        if (MainInitSettings.DebugGiveThisItemFromStart != null)
        {
            foreach (var c in MainInitSettings.DebugGiveThisItemFromStart)
            {
                await Players[0].AddItem(G.Main.Decks.treasureDeck.FindAndGetCard(c.GetSprite()));
            }
        }
        PhaseController.StartStartTurn();
    }
    private void InitAllDecks(DeckListConfig config)
    {
        GameObject g = new GameObject("Deck_Holder");
        Decks = new Decks
        {
            characterDeck = CreateDeck(new Vector3(-10, 0, 0), "Deck - CharacterDeck"),
            lootDeck = CreateDeck( CardPlaces.lootDeck.position, "Deck - Loot_Deck"),
            lootStash = CreateDeck(CardPlaces.lootStash.position, "Stash - LootStash"),
            treasureDeck = CreateDeck(CardPlaces.treasureDeck.position, "Deck - TreasureDeck"),
            treasureStash = CreateDeck(CardPlaces.treasureStash.position, "Stash - TreasureStash"),
            monsterDeck = CreateDeck(CardPlaces.monsterDeck.position, "Deck - MonsterDeck"),
            monsterStash = CreateDeck(CardPlaces.monsterStash.position, "Stash - MonsterStash")
        };
        
        Decks.characterDeck.InitDeck(config.characterDeckData, false); 
        Decks.lootDeck.InitDeck(config.lootDeckData, false);
        Decks.lootStash.InitDeck(null, true);
        Decks.treasureDeck.InitDeck(config.treasureDeckData, false);
        Decks.treasureStash.InitDeck(null, true);
        Decks.monsterDeck.InitDeck(config.monsterDeckData, false);
        Decks.monsterStash.InitDeck(null, true);
    }
    private Deck CreateDeck(Vector3 position, string n)
    {
        GameObject g = new GameObject(n)
        {
            transform =
            {
                position = position,
                localScale = Vector3.one * Card.CARDSIZE,
                parent = GameObject.Find("Deck_Holder").transform
            }
        };
        
        Deck deck = g.AddComponent<Deck>();
        return deck;
    }
    private void InitAllPlayers(int playerCount)
    {
        List<Player> playersList = new List<Player>();
        for(int j = playerCount; j < 4; j++)
        {
            CardPlaces.playersTransformToDeconstruct[j].gameObject.SetActive(false);
        }
        for(int i = 0; i < playerCount; i++)
        {
            GameObject p = new GameObject();
            if (i != 0) p.transform.localScale = Vector3.one * 0.64f;
            Player player = p.AddComponent<Player>();
            player.statsText = CardPlaces.statsTexts[i];
            playersList.Add(player);
            p.name = "Player - " + i;  
            Hand h = InitHand(i, p.transform);
            
            player.Get<TagBasePlayerData>().hand = h;
        }
        Players = playersList;
    } 
    private Hand InitHand(int i, Transform p)
    {
        GameObject h = new GameObject("Hand");
        Hand ha = h.AddComponent<Hand>();
        ha.transform.position = CardPlaces.hands[i].position;
        h.transform.parent = p.transform;
        ha.transform.localScale = Vector3.one;
        ha.Init(6.5f * Math.Abs(h.transform.lossyScale.x));
        return ha;
    }
}
public class Decks
{
    public Deck characterDeck;
    public Deck lootDeck, lootStash;
    public Deck monsterDeck, monsterStash;
    public Deck treasureDeck, treasureStash;
    public List<Deck> GetNormalDecks()
    {
        return new List<Deck>() { lootDeck, monsterDeck, treasureDeck };
    }

    public List<Deck> GetAllDecks()
    {
        return new List<Deck>() { lootDeck, monsterDeck, treasureDeck, characterDeck, lootStash, monsterStash, treasureStash };
    }

    public void ChangeColliderActive(bool b)
    {
        GetAllDecks().ForEach(x => x.collider.enabled = b);
    }
}
public static class ActionTime
{
    public static float animSpeed = 1;
    public static float cardSpeed = 30f * animSpeed;
    public static float timeToFlipCard = 0.2f * animSpeed;
    public static float timeToChangeScaleCard = 0.35f * animSpeed;
}