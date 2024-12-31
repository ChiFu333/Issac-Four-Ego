using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations;
using DG.Tweening;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    //Базовые характеристики
    public int Hp, Attack;
    public int Coins;
    public int LootCount { get => Hand.Cards.Count;}
    public int Souls; //Надо заменить на свою колоду душ
    private int BaseHP;
    public int LootPlayMax = 1;
    public int LootCanPlay;
    //Объекты
    public CharacterCard CharacterCard;
    public ItemCard CharacterItemCard;
    public Hand Hand;
    public List<ItemCard> Items = new List<ItemCard>();
    //Всякие события
    public event Action OnTurnStart;  
    public event Action OnTurnEnd;

    public void Init(CharacterCard c, ItemCard i, Hand hand)
    {
        c.transform.parent = transform;
        i.transform.parent = transform;

        CharacterCard = c;
        CharacterItemCard = i;
        Hand = hand;
        
        CharacterCardData ccd = CharacterCard.data as CharacterCardData;
        BaseHP = ccd.Hp;
        Hp = ccd.Hp;
        Attack = ccd.Attack;
        Coins = 0;
        LootCanPlay = 0;
        Souls = ccd.StartSouls;
    }
    public void TakeOneCard(LootCard card)
    {
        Hand.AddCard(card);
    }
    public void StartTurn()
    {
        RechargeAllCards();
        OnTurnStart?.Invoke();
        Hand.AddCard(Card.CreateCard<LootCard>(GameMaster.Inst.LootDeck.TakeOneCard(),TurnManager.Inst.ActivePlayer == TurnManager.Inst.Players[0]));
        LootCanPlay = LootPlayMax;
    }
    public void EndTurn()
    {
        OnTurnEnd?.Invoke();
    }
    public void RechargeAllCards()
    {
        CharacterCard.Recharge();
        CharacterItemCard.Recharge();
        for(int i = 0; i < Items.Count; i++)
        {
            Items[i]?.Recharge();
        }
    }
    public void PlayCard(LootCard c)
    {
        if(LootCanPlay > 0) 
        {
            Hand.PlayCard(c);
            LootCanPlay--;
        }
        else
        {
            Console.WriteText("Ты уже разыграл лут");
        }
    }
    public void AddItem(Card c)
    {
        for(int i = 0; i < Items.Count; i++)
        {
            if(Items[i] == null) 
            {
                Items[i] = c as ItemCard;
                c.transform.parent = transform;
                c.transform.DOMove(CardPlaces.Inst.PlayersPos[TurnManager.Inst.PriorId][i+2].position, GameMaster.CARDSPEED);
                return;
            }
        }
        Items.Add(c as ItemCard);
        c.transform.parent = transform;
        c.transform.DOMove(CardPlaces.Inst.PlayersPos[TurnManager.Inst.PriorId][Items.Count+1].position, GameMaster.CARDSPEED);
        c.transform.localScale = CardPlaces.Inst.PlayersPos[TurnManager.Inst.PriorId][Items.Count+1].lossyScale;
    }
    public void Death()
    {
        //Отменяет все действия, там атаку
        //Вызывает состояние оплаты смерти, игрок выбирает всё-всё
        //Закончить ход
    }
    public Player GetMyPlayer()
    {
        return GetComponentInParent<Player>();
    }
}
