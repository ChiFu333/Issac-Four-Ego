using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class Shop : MonoBehaviour
{
    public int activeSlotsCount = 2;
    private List<Card> itemsInSlots = new List<Card>();
    public void Init()
    {
        for(int j = 0; j < activeSlotsCount; j++)
        {
            ItemCard c = (ItemCard)GameMaster.inst.shopDeck.TakeOneCard();
            itemsInSlots.Add(c);
        }

        RestockSlots();
    }
    public void RestockSlots()
    {
        for(int i = 0; i < activeSlotsCount; i++)
        {
            if(itemsInSlots[i] == null) itemsInSlots[i] = (ItemCard)GameMaster.inst.shopDeck.TakeOneCard();
        }
        for(int i = itemsInSlots.Count-1; i >= 0; i--)
        {
            itemsInSlots[i].MoveTo(CardPlaces.inst.shopSlots[activeSlotsCount-i-1], transform);
        }
    }
    public void IncreaseShop(int count)
    {
        activeSlotsCount = math.min(activeSlotsCount + count, 4); 
        RestockSlots();
    }
    public void StartShopSubPhase()
    {
        if(GameMaster.inst.turnManager.activePlayer.buyCount <= 0) return;
        GameMaster.inst.turnManager.activePlayer.buyCount -= 1;
        _ = GameMaster.inst.phaseSystem.StartBuying();   
    }
    public void InstBuy(Card itemToBuy)
    {
        Console.WriteText("Куплен предмет");
        itemsInSlots[itemsInSlots.IndexOf(itemToBuy)] = null;
        GameMaster.inst.turnManager.activePlayer.AddItem(itemToBuy);
        RestockSlots();
        UIOnDeck.inst.UpdateTexts();
    }
}
