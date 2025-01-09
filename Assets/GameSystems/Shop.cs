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
            c.transform.parent = transform;
            itemsInSlots.Add(c);
        }

        RestockSlots();
    }
    public void RestockSlots()
    {
        for(int i = 0; i < activeSlotsCount; i++)
        {
            if(itemsInSlots[i] == null)
            {
                ItemCard c = Card.CreateCard<ItemCard>(GameMaster.inst.shopDeck.TakeOneCard());
                c.transform.parent = transform;
                itemsInSlots[i] = c;
            }
        }
        for(int i = itemsInSlots.Count-1; i >= 0; i--)
        {
            itemsInSlots[i].transform.DOMove(CardPlaces.inst.shopSlots[activeSlotsCount-i-1].position,GameMaster.CARDSPEED);
        }
    }
    public async void Buy()
    {
        if(GameMaster.inst.turnManager.activePlayer.buyCount <= 0) return;

        Card c = await SubSystems.Inst.SelectCardByType<ItemCard>("Shop");
        if(c == null) return;

        if(GameMaster.inst.turnManager.activePlayer.PermitBuy())
        {
            InstBuy(c);
        }
        else
        {
            Console.WriteText("Нехватает деньги");
        }
    }
    private void InstBuy(Card itemToBuy)
    {
        Console.WriteText("Куплен предмет");
        GameMaster.inst.turnManager.activePlayer.AddItem(itemToBuy);
        
        itemsInSlots[itemsInSlots.IndexOf(itemToBuy)] = null;
        RestockSlots();
        UIOnDeck.inst.UpdateTexts();
    }
}
