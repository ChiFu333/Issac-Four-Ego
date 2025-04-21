using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;

public class CardWatcher : MonoBehaviour
{
    public static CardWatcher inst { get; private set; }
    [SerializeField] private GameObject button;
    [SerializeField] private SlotZone zone;
    [SerializeField] private TMP_Text description;
    private GameObject _system;
    
    private bool triggerEnd = false;
    private Func<bool> funcToChoose;
    private void Awake()
    {
        _system = transform.GetChild(0).gameObject;
        inst = this;
    }

    public async void Start()
    {
        await UniTask.Delay(100);
        //_ = WatchFivePutOneUp(G.Decks.lootDeck);
    }

    public async UniTask WatchHand(Player p)
    {
        Setup("Это карты игрока:\n" + p.GetMyCard().name.Split(' ')[0]);
        zone.enableRightClickDrag = false;
        funcToChoose = () => true;

        SlotZone handsZone = p.hand.handZone;
        List<Entity> list = handsZone.cards.ToList();
        List<Entity> list2 = new List<Entity>();
        for (int i = 0; i < list.Count; i++)
        {
            list2.Add(Entity.CreateEntity(list[i].gameObject, true));
        }
        for (int i = 0; i < list2.Count; i++)
        {
            Entity t = list2[i];
            t.RemoveTag(t.GetTag<PlayFromHand>());
            t.SetActive(true);
            t.gameObject.layer = 8;
            t.transform.position = new Vector3(10, 0, -1);
            t.visual.render.sortingLayerName = "CardWatcher";
            zone.AddCard(t);
            await UniTask.Delay(75);
        }
        await UniTask.Delay(100);
        button.SetActive(true);
        await UniTask.WaitUntil(() => triggerEnd);
        UniTask tas = UniTask.CompletedTask;
        foreach (var ent in list2)
        {
            ent.transform.parent = null;
            tas = ent.MoveToForHand(new Vector3(11, 0, -1), 0, () => Destroy(ent.gameObject));
            await UniTask.Delay(20);
        }
        await tas;

        Dispose();
    }
    public async UniTask WatchTopAndCanDiscard(CardDeck deck)
    {
        Setup("Эта карта лежит вверху колоды.\nВыбери её, если хочешь сбросить.");
        funcToChoose = () => true;
        await PutCardsFromDeck(deck, 1, true);
        button.SetActive(true);
        await UniTask.WaitUntil(() => triggerEnd);
        
        foreach (var card in zone.cards)
        {
            if (card.HasTag<TapBalatro>() && card.GetTag<TapBalatro>().isTapped)
            {
                zone.cards.Remove(card);
                card.transform.parent = null;
                await card.MoveToForHand(new Vector3(-11, 0, -1), 0,() => deck.PutOneCardUnder(card));
                break;
            }
            else
            {
                zone.cards.Remove(card);
                card.transform.parent = null;
                await card.MoveToForHand(new Vector3(11, 0, -1), 0,() => deck.PutOneCardUp(card));
                break;
            }
        }
        
        Dispose();
    }
    public async UniTask WatchCardsAndPutInAnyOrder(CardDeck deck, int count)
    {
        Setup("Расставь карты в любом порядке.\nПосле положи их обратно в колоду.");
        funcToChoose = () => true;
        
        await PutCardsFromDeck(deck, count);
        button.SetActive(true);
        await UniTask.WaitUntil(() => triggerEnd);

        await PutAllCardsToDeck(deck);

        Dispose();
    }
    public async UniTask WatchFivePutOneUp(CardDeck deck)
    {
        Setup("Выбери карту, которая останется наверху колоды\nОстальное уйдёт вниз колоды.");
        funcToChoose = () =>
        {
            int count = 0;
            foreach (var card in zone.cards)
            {
                if (card.HasTag<TapBalatro>() && card.GetTag<TapBalatro>().isTapped)
                {
                    count += 1;
                }
            }

            return count == 1;
        };
        await PutCardsFromDeck(deck, 5, true);
        button.SetActive(true);
        await UniTask.WaitUntil(() => triggerEnd);
        
        foreach (var card in zone.cards)
        {
            if (card.HasTag<TapBalatro>() && card.GetTag<TapBalatro>().isTapped)
            {
                zone.cards.Remove(card);
                card.transform.parent = null;
                card.MoveToForHand(new Vector3(-11, 0, -1), 0,() => deck.PutOneCardUp(card));
                break;
            }
        }
        await PutAllCardsToDeck(deck, true);
        
        Dispose();
    }

    private void Setup(string descripion)
    {
        zone.enableRightClickDrag = true;
        triggerEnd = false;
        zone.cards.Clear();
        _system.SetActive(true);
        description.text = descripion;
        description.gameObject.SetActive(true);
    }

    private async UniTask PutCardsFromDeck(CardDeck deck, int count, bool tappable = false)
    {
        for (int i = 0; i < count; i++)
        {
            Entity t = deck.TakeOneCard();
            t.gameObject.layer = 8;
            if(tappable) t.AddTag(new TapBalatro());
            t.transform.position = new Vector3(10, 0, -1);
            _ = zone.AddCard(t);
            t.visual.render.sortingLayerName = "CardWatcher";
            await UniTask.Delay(75);
        }
        await UniTask.Delay(100);
    }

    private async UniTask PutAllCardsToDeck(CardDeck deck, bool toUnderDeck = false)
    {
        UniTask task = UniTask.CompletedTask;
        List<Entity> r = zone.cards;
        r.Reverse();
        foreach (var ent in r)
        {
            ent.transform.parent = null;
            _ = ent.MoveToForHand(new Vector3(11, 0, -1), 0);
            await UniTask.Delay(20);
        }

        await UniTask.Delay(350);
        foreach (var ent in r)
        {
            if (toUnderDeck)
            {
                deck.PutOneCardUnder(ent);
            }
            else
            {
                deck.PutOneCardUp(ent);
            }
        }
    }

    private async UniTask PutAllCardsToStash(CardDeck deck)
    {
        if (deck == G.Decks.lootDeck)
            await PutAllCardsToDeck(G.Decks.lootStash);
        if (deck == G.Decks.monsterDeck)
            await PutAllCardsToDeck(G.Decks.monsterStash);
        if (deck == G.Decks.treasureDeck)
            await PutAllCardsToDeck(G.Decks.treasureStash);
    }

    private void Dispose()
    {
        description.gameObject.SetActive(false);
        _system.SetActive(false);
    }
    public void End()
    {
        if (funcToChoose != null && funcToChoose.Invoke())
        {
            triggerEnd = true;
            button.SetActive(false);
        }
    }
}
