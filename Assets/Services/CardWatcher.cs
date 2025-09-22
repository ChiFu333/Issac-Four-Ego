using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class CardWatcher : MonoBehaviour
{
    [SerializeField] private  Button button;
    [SerializeField] private SlotZone zone;
    [SerializeField] private TMP_Text description;

    private Vector3 deckPos = new Vector3(6.5f, 0, 0);
    private float scale = 1.5f;
    private bool trigger;
    private Func<bool> funcToChoose;
    public async void Start()
    {
        button.onClick.AddListener(End);
    }
    private async UniTask Setup(string descripion, Deck deck = null)
    {
        await ChangeSetup(true, deck);
        
        description.text = descripion;
    }
    private async UniTask Dispose(Deck deck = null)
    {
        await ChangeSetup(false, deck);
        zone.transform.localPosition = Vector3.zero;

    }
    private async UniTask ChangeSetup(bool turnOn, Deck deck = null)
    {
        trigger = false;
        
        G.Main.ActionChecker.isWatchingCards = turnOn;
        G.Main.TableUI.ChangeButtonsActive(!turnOn);
        G.Main.Decks.ChangeColliderActive(!turnOn);
        G.Main.GameZones.ChangeActive(!turnOn);
        
        //description.gameObject.SetActive(turnOn);
        G.LightController.SetLight(turnOn ? G.LightController.config.DIM_LIGHTOUT : G.LightController.config.NORMAL_INTENSITY).Forget();
        description.transform.DOScale(turnOn ? Vector3.one : (Vector3.one * 0), 0.4f).SetEase(Ease.InOutBack)
            .OnUpdate(() =>
            {
                var s = description.transform.localScale;
                if (s.x < 0 || s.y < 0 || s.z < 0)
                {
                    description.transform.localScale = Vector3.zero;
                }
            });
        if (turnOn)
        {
            button.gameObject.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.InBack)
                .OnUpdate(() =>
                {
                    var scale = button.gameObject.transform.localScale;
                    if (scale.x < 0 || scale.y < 0 || scale.z < 0)
                    {
                        button.gameObject.transform.localScale = Vector3.zero;
                    }
                });;
        }
        if (deck != null)
        {
            deck.SetLit(turnOn ? 0.1f : 1f);
            if(turnOn)
                await deck.MoveTo(deckPos, scale, 1000, false);
            else
                await deck.ReturnToBasePos();
            
        }
        
    }
    public async UniTask WatchHand(Player p)
    {
        await Setup("Это карты игрока:\n" + p.name);
        zone.enableDrag = false;
        funcToChoose = () => true;

        SlotZone handsZone = p.Get<TagBasePlayerData>().hand.handZone;
        zone.ReserveSlots(handsZone.cards.Count);
        
        UniTask task = UniTask.CompletedTask;
        List<Card> tempList = new List<Card>();
        foreach (var c in handsZone.cards)
        {
            Card tempCard = c.CloneCard();
            tempCard.visual.sortingOrder = 500;
            tempCard.SetActive(true);
            tempCard.RemoveLit();
            tempCard.transform.position = c.transform.position;
            tempCard.transform.DOScale(c.transform.lossyScale, 0.2f).From(Vector3.zero);
            tempList.Add(tempCard);
            await UniTask.Delay(50);
        }

        await UniTask.Delay(100);
        foreach (var card in tempList)
        {
            if(!card.isFaceUp) card.Flip(true);
            task = zone.AddCard(card);
            await UniTask.Delay(100);
        }
        await task;
        
        button.gameObject.SetActive(true);
        await UniTask.WaitUntil(() => trigger);
        
        UniTask tas = UniTask.CompletedTask;
        for(int i = 0; i < handsZone.cards.Count; i++)
        {
            tas = tempList[i].MoveTo(handsZone.cards[i].transform.position);
            tempList[i].transform.parent = null;
            if(!handsZone.cards[i].isFaceUp) tempList[i].Flip(false);
            tempList[i].transform.DOScale(handsZone.cards[i].transform.lossyScale, 0.2f);
            await UniTask.Delay(50);
        }
        await tas;
        foreach (var ent in tempList.ToList())
        {
            Destroy(ent.gameObject);
        }
        zone.cards.Clear();
        zone.enableDrag = false;
        await Dispose();
    }
    public async UniTask WatchTopAndCanDiscard(Deck deck)
    {
        await Setup("Эта карта лежит вверху колоды.\nВыбери её, если хочешь сбросить.", deck);
        funcToChoose = () => true;
        await TakeCardsFromDeck(deck, 1, true);
        
        button.gameObject.SetActive(true);
        await UniTask.WaitUntil(() => trigger);

        foreach (var card in zone.cards)
        {
            if (card.Is<TapBalatro>() && !card.Get<TapBalatro>().isTapped)
            {
                zone.cards.Remove(card);
                card.transform.parent = null;
                await card.PutOnDeck(deck);
                break;
            }
            else
            {
                card.RemoveTag(card.Get<TapBalatro>());
            }
        }
        await ReturnAllCardsToDeck(deck, true);

        await Dispose(deck);
    }
    /*
    public async UniTask WatchCardsAndPutInAnyOrder(CardDeck deck, int count)
    {
        Setup("Расставь карты в любом порядке.\nПосле положи их обратно в колоду.");
        funcToChoose = () => true;

        await PutCardsFromDeck(deck, count);
        button.SetActive(true);
        await UniTask.WaitUntil(() => trigger);

        await PutAllCardsToDeck(deck);

        Dispose();
    }
    */
    public async UniTask WatchXPutOneUp(Deck deck, int count)
    {
        await Setup("Выбери карту, которая останется наверху колоды\nОстальное уйдёт вниз колоды.", deck);
        funcToChoose = () =>
        {
            int count = zone.cards.Count(card => card.Is<TapBalatro>() && card.Get<TapBalatro>().isTapped);
            return count == 1;
        };
        await TakeCardsFromDeck(deck, count, true);
        
        button.gameObject.SetActive(true);
        await UniTask.WaitUntil(() => trigger);
        
        foreach (var card in zone.cards)
        {
            if (card.Is<TapBalatro>() && card.Get<TapBalatro>().isTapped)
            {
                card.RemoveTag(card.Get<TapBalatro>());
                zone.cards.Remove(card);
                card.transform.parent = null;
                await card.PutOnDeck(deck);
                break;
            }
        }
        await ReturnAllCardsToDeck(deck, true);

        await Dispose(deck);
    }

    

    private async UniTask TakeCardsFromDeck(Deck deck, int count, bool tappable = false)
    {
        zone.ReserveSlots(count);
        for (int i = 0; i < count; i++)
        {
            Card card = deck.TakeOneCard(i, true);
            card.visual.sortingOrder = 1000;
            card.RemoveLit();
            if(tappable) card.AddTag(new TapBalatro());
            _ = zone.AddCard(card);
            await UniTask.Delay(200);
        }
    }
    private async UniTask ReturnAllCardsToDeck(Deck deck, bool toUnderDeck = false)
    {
        UniTask task = UniTask.CompletedTask;
        List<Card> r = zone.cards.ToList();
        r.Reverse();
        zone.cards.Clear();
        foreach (var card in r.ToList())
        {
            _ = zone.RemoveCard(card);
            card.transform.parent = null;
            if (toUnderDeck)
                task = card.PutUnderDeck(deck);
            else
                task = card.PutOnDeck(deck);
            await UniTask.Delay(450);
        }

        await task;
        await UniTask.Delay(300);
    }
    /*
    private async UniTask PutAllCardsToStash(CardDeck deck)
    {
        if (deck == G.Decks.lootDeck)
            await PutAllCardsToDeck(G.Decks.lootStash);
        if (deck == G.Decks.monsterDeck)
            await PutAllCardsToDeck(G.Decks.monsterStash);
        if (deck == G.Decks.treasureDeck)
            await PutAllCardsToDeck(G.Decks.treasureStash);
    }
    */
    
    private void End()
    {
        if (funcToChoose != null && funcToChoose.Invoke())
        {
            trigger = true;
            button.gameObject.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnUpdate(() =>
            {
                var scale = button.gameObject.transform.localScale;
                if (scale.x < 0 || scale.y < 0 || scale.z < 0)
                {
                    button.gameObject.transform.localScale = Vector3.zero;
                }
            });;
        }
    }

    
}