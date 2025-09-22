using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
public class Shop
{
    public int activeSlotsCount = 2;
    private List<Card> itemsInSlots = new List<Card>();
    public GameObject itemsHolder;
    public Shop()
    {
        itemsHolder = new GameObject("Shop");
    }

    public async UniTask Init()
    {
        await RestockSlots();
    }
    public async UniTask RestockSlots()
    {
        UniTask task = UniTask.CompletedTask;
        for(int i = 0; i < activeSlotsCount; i++)
        {
            if(itemsInSlots.Count < activeSlotsCount) itemsInSlots.Add(null);
            if (itemsInSlots[i] == null)
            {
                Card c = G.Main.Decks.treasureDeck.TakeOneCard(i, true);
                c.AddTag(new TagInShop());
                itemsInSlots[i] = c;
                task = itemsInSlots[i].MoveTo(G.Main.CardPlaces.shopSlots[activeSlotsCount-i-1].position, 0);
                itemsInSlots[i].transform.parent = itemsHolder.transform;
                await UniTask.Delay(250);
            }
        }

        await task;
    }
    public void IncreaseShop(int count)
    {
        activeSlotsCount = Mathf.Min(activeSlotsCount + count, 4); 
        RestockSlots();
    }
    /*
    public void StartShopSubPhase()
    {
        if(G.Players.activePlayer.buyCount <= 0) return;
        G.Players.activePlayer.buyCount -= 1;
        _ = GameMaster.inst.phaseSystem.StartBuying();   
    }
    public void InstBuy(Entity itemToBuy)
    {
        Console.WriteText("Куплен предмет");
        if (itemToBuy.HasTag<InShop>()) 
            itemToBuy.RemoveTag(itemToBuy.GetTag<InShop>());
        itemsInSlots[itemsInSlots.IndexOf(itemToBuy)] = null;
        G.Players.activePlayer.AddItem(itemToBuy);
        RestockSlots();
        UIOnDeck.inst.UpdateTexts();
    }
    */
    public void RemoveMeFromShop(Card c)
    {
        if(c.Is<TagInShop>()) c.RemoveTag(c.Get<TagInShop>());
        itemsInSlots[itemsInSlots.IndexOf(c)] = null;
    }
}
