using System.Linq;
using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.EventSystems;
using Runtime;

public class TagBasePlayerData : EntityComponentDefinition
{
    public int id;
    public Hand hand;
    public Card characterCard;
    public List<Card> items = new ();
    public List<Card> souls = new ();
    public int coins = 20;

    public int lootTakeCount = 1;
    
    public int lootPlayCount;
    public int buyCount;
    public int attackCount;
}
public class Player : MonoBehaviour, GameView, ISelectableTarget
{
    [SerializeReference, SubclassSelector]
    public List<EntityComponentDefinition> state = new List<EntityComponentDefinition>()
    {
        new TagBasePlayerData()
    };

    public async UniTask Init(int id, MainInitSettings config)
    {
        TagBasePlayerData data = Get<TagBasePlayerData>();
        data.id = id;
        if (config.DebugOverrideStartCharacter != null && id == 0)
        {
            data.characterCard = G.Main.Decks.characterDeck.FindAndGetCard(config.DebugOverrideStartCharacter.GetSprite());
        }
        else
        {
            data.characterCard = G.Main.Decks.characterDeck.TakeOneCard(0, true);
        }
        data.characterCard.transform.parent = transform;
        data.characterCard.transform.DOScale(Card.CARDSIZE, ActionTime.timeToChangeScaleCard);
        await data.characterCard.MoveTo(G.Main.CardPlaces.playersPos[id][0].position);
        data.characterCard.Get<TagTappable>().Tap();
        
        Card myItem = Card.CreateCard(Get<TagBasePlayerData>().characterCard.Get<TagCharacterItemPrefab>().item.GetComponent<CMSEntityPfb>().AsEntity().DeepCopy());
        G.Main.AllCards.Add(myItem);

        myItem.visual.sortingOrder = 2;
        myItem.transform.position = data.characterCard.transform.position;
        await AddItem(myItem);
        myItem.visual.sortingOrder = 3;
        /*
        Card myItem2 = Card.CreateCard(Get<TagBasePlayerData>().characterCard.Get<TagCharacterItemPrefab>().item.GetComponent<CMSEntityPfb>().AsEntity().DeepCopy());
        G.Main.AllCards.Add(myItem2);

        myItem2.visual.sortingOrder = 2;
        myItem2.transform.position = data.characterCard.transform.position;
        await AddItem(myItem2);
        myItem2.visual.sortingOrder = 3;

        Card myItem3 = Card.CreateCard(Get<TagBasePlayerData>().characterCard.Get<TagCharacterItemPrefab>().item.GetComponent<CMSEntityPfb>().AsEntity().DeepCopy());
        G.Main.AllCards.Add(myItem3);

        myItem3.visual.sortingOrder = 2;
        myItem3.transform.position = data.characterCard.transform.position;
        await AddItem(myItem3);
        myItem3.visual.sortingOrder = 3;
        */
        //////////////////////
        /*
        Entity it = Entity.CreateEntity(characterCard.GetTag<CharacterItemPrefab>().itemPrefab);
        it.SetActive(true);
        AddItem(it);
        if(it.HasTag<Tappable>()) it.GetTag<Tappable>().Tap();

        this.hand = hand;
        characteristics = characterCard.GetTag<Characteristics>();
        attack = characterCard.GetTag<Characteristics>().attack;
        coins = 10;
        lootPlayCount = 0;
        souls = 0;
        addDamageToBattleThrow = new List<int> { 0, 0, 0, 0, 0 };
        AddHp(10);
        await UniTask.Delay(100);
        */
    }
    public async UniTask AddOneLootCard(Card c)
    {
        UniTask t = Get<TagBasePlayerData>().hand.AddCard(c);
        UpdateStats();
        await t;
    }
    public async UniTask RemoveOneLootCard(Card c)
    {
        UniTask t = Get<TagBasePlayerData>().hand.DiscardCard(c);
        UpdateStats();
        await t;
    }
    public void ChangeCoin(int count)
    {
        Get<TagBasePlayerData>().coins += count;
        UpdateStats();
    }

    public async UniTask AddItem(Card c)
    {
        if(!c.isActiveAndEnabled) c.SetActive(true);
        c.visual.sortingOrder = 10;
        int itemsCount = Get<TagBasePlayerData>().items.Count;
        for(int i = 0; i < itemsCount+1; i++)
        {
            if(Get<TagBasePlayerData>().items.Count <= i) Get<TagBasePlayerData>().items.Add(null);
            if (Get<TagBasePlayerData>().items[i] == null)
            {
                Get<TagBasePlayerData>().items[i] = c;
                c.transform.parent = transform;
                c.transform.localScale = Vector3.one * Card.CARDSIZE;
                c.SetActive(true);

                await c.MoveTo(G.Main.CardPlaces.playersPos[Get<TagBasePlayerData>().id][1+i].position);
                break;
            }
        }
    }

    public void TryRemoveItem(Card c)
    {
        for (int i = 0; i < Get<TagBasePlayerData>().items.Count; i++)
        {
            if (Get<TagBasePlayerData>().items[i] == c)
            {
                Get<TagBasePlayerData>().items[i] = null;
                break;
            }
        }
    }
    public async UniTask AddSoul(Card c)
    {
        int itemsCount = Get<TagBasePlayerData>().items.Count;
        for(int i = 0; i < itemsCount+1; i++)
        {
            if(Get<TagBasePlayerData>().souls.Count <= i) Get<TagBasePlayerData>().souls.Add(null);
            if (Get<TagBasePlayerData>().souls[i] == null)
            {
                Get<TagBasePlayerData>().souls[i] = c;
                c.transform.parent = transform;
                c.transform.DOScale(Vector3.one * Card.CARDSIZE * 0.615f, 0.2f);
                c.SetActive(true);

                await c.MoveTo(G.Main.CardPlaces.playersSouls[Get<TagBasePlayerData>().id][i].position, -90);
                break;
            }
        }
    }
    
    #region  [ Text on Table ]
    public TMP_Text statsText;
    public void UpdateStats()
    {
        TagBasePlayerData myBaseData = state.FirstOrDefault(x => (x is TagBasePlayerData) == true) as TagBasePlayerData;
        TagCharacteristics myChar = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<TagCharacteristics>() == true).Get<TagCharacteristics>();//myBaseData.characterCard.currentState.Get<TagCharacteristics>();
        
        string hp = "<color=#E34444>☻: " + myChar.health;
        string addHp = myChar.healthPrevent == 0 ? "" : "<color=#94EEEE>("+myChar.healthPrevent+")</color>";
        string endHp = "/" + myChar.HealthMax+ "</color>";

        string power = myChar.healthPrevent == 0 ? "  <color=#969696>♥: " + myChar.attack + "</color>" : " <color=#969696>♥: " + myChar.attack + "</color>";
        string money = "  <color=#E3C034>" + myBaseData.coins +"¢</color>";
        string loots = "  <color=#94EEEE>карт лута: " + myBaseData.hand.handZone.cards.Count + "</color>";

        string isCurrent = "";//"  души: " + players[i].souls + (players[i] == G.Players.activePlayer ? " (!)" : "";
        statsText.text = hp + addHp + endHp + power + money + loots + isCurrent;
    }
    #endregion
    
    #region [ Tag's Actions ]
    
    public void AddTag(EntityComponentDefinition t)
    {
        state.Add(t);
    }

    public T Get<T>() where T : EntityComponentDefinition, new()
    {
        return state.Find(m => m is T) as T;
    }
    #endregion
}
