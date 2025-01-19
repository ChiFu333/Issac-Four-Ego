using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public int activeSlotsCount = 2;
    private List<Card> itemsInSlots = new List<Card>();
    public void Init()
    {
        for(int j = 0; j < activeSlotsCount; j++)
        {
            ItemCard c = Card.CreateCard<ItemCard>(GameMaster.inst.shopDeck.TakeOneCard());
            itemsInSlots.Add(c);
        }

        RestockSlots();
    }
    public void RestockSlots()
    {
        for(int i = 0; i < activeSlotsCount; i++)
        {
            if(itemsInSlots[i] == null) itemsInSlots[i] = Card.CreateCard<ItemCard>(GameMaster.inst.shopDeck.TakeOneCard());
        }
        for(int i = itemsInSlots.Count-1; i >= 0; i--)
        {
            itemsInSlots[i].MoveTo(CardPlaces.inst.shopSlots[activeSlotsCount-i-1], transform);
        }
    }
    public async void Buy()
    {
        if(GameMaster.inst.turnManager.activePlayer.buyCount <= 0) return;

        Card c = await SubSystems.inst.SelectCardByType<ItemCard>("Shop");
        if(c == null) return;

        if(GameMaster.inst.turnManager.activePlayer.PermitBuy())
        {
            GameMaster.inst.turnManager.activePlayer.buyCount -= 1;
            UIOnDeck.inst.UpdateAddInfo();
            InstBuy(c);
        }
        else
        {
            Console.WriteText("Нехватает денег");
        }
    }
    private void InstBuy(Card itemToBuy)
    {
        Console.WriteText("Куплен предмет");
        itemsInSlots[itemsInSlots.IndexOf(itemToBuy)] = null;
        GameMaster.inst.turnManager.activePlayer.AddItem(itemToBuy);
        RestockSlots();
        UIOnDeck.inst.UpdateTexts();
    }
}
