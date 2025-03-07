using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using System.Threading.Tasks;

public class Shop : MonoBehaviour
{
    public int activeSlotsCount = 2;
    private List<Entity> itemsInSlots = new List<Entity>();
    public void Init()
    {
        for(int j = 0; j < activeSlotsCount; j++)
        {
            Entity c = G.Decks.shopDeck.TakeOneCard();
            itemsInSlots.Add(c);
        }

        RestockSlots();
    }
    public void RestockSlots()
    {
        for(int i = 0; i < activeSlotsCount; i++)
        {
            if(itemsInSlots[i] == null) itemsInSlots[i] = G.Decks.shopDeck.TakeOneCard();
        }
        for(int i = itemsInSlots.Count-1; i >= 0; i--)
        {
            itemsInSlots[i].MoveTo(G.CardPlaces.shopSlots[activeSlotsCount-i-1], transform);
        }
    }
    public void IncreaseShop(int count)
    {
        activeSlotsCount = math.min(activeSlotsCount + count, 4); 
        RestockSlots();
    }
    public void StartShopSubPhase()
    {
        if(G.Players.activePlayer.buyCount <= 0) return;
        G.Players.activePlayer.buyCount -= 1;
        _ = GameMaster.inst.phaseSystem.StartBuying();   
    }
    public void InstBuy(Entity itemToBuy)
    {
        Console.WriteText("Куплен предмет");
        itemsInSlots[itemsInSlots.IndexOf(itemToBuy)] = null;
        G.Players.activePlayer.AddItem(itemToBuy);
        RestockSlots();
        UIOnDeck.inst.UpdateTexts();
    }
}