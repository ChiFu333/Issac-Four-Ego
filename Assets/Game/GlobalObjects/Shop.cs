using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public static Shop Inst;
    public int ActiveSlotsCounts = 2;
    public List<Card> ItemsInSlots = new List<Card>();
    public void Awake()
    {
        Inst = this;

    }
    public void Init()
    {
        RestockSlots();
    }
    public void RestockSlots()
    {
        int t = ItemsInSlots.Count;
        for(int j = 0; j < ActiveSlotsCounts - t; j++)
        {
            Card c = Card.CreateCard<ItemCard>(GameMaster.Inst.ShopDeck.TakeOneCard());
            c.transform.parent = transform;
            ItemsInSlots.Add(c);
        }
        for(int i = ItemsInSlots.Count-1; i >= 0; i--)
        {
            ItemsInSlots[i].transform.DOMove(CardPlaces.Inst.ShopSlots[ActiveSlotsCounts-i-1].position,GameMaster.CARDSPEED);
        }
    }
    public async void Buy()
    {
        if(TurnManager.Inst.ActivePlayer.BuyCount > 0)
        {
            Card c = await SubSystems.Inst.SelectCardByType<ItemCard>("Shop");
        if(c != null)
        {
            if(TurnManager.Inst.GetPriorPlayer().Coins >= TurnManager.Inst.ActivePlayer.ShopPrice)
            {
                Console.WriteText("Куплен предмет");
                TurnManager.Inst.GetPriorPlayer().AddItem(c);
                TurnManager.Inst.GetPriorPlayer().Coins -= TurnManager.Inst.ActivePlayer.ShopPrice;
                TurnManager.Inst.GetPriorPlayer().BuyCount -= 1;
                ItemsInSlots.Remove(c);
                RestockSlots();
                UIOnDeck.Inst.UpdateTexts();
            }
            else
            {
                Console.WriteText("Нехватает деньги");
            }
        }
        } 
    }
}
