using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Unity.VisualScripting;
using Sirenix.OdinInspector;

public class Player : MonoBehaviour
{
    [field: SerializeField, HorizontalGroup("HP")] public int hp { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int attack { get; private set; }
    private int hpMax;
    [field: SerializeField, HorizontalGroup("Resources")] public int coins { get; private set; }
    public int lootCount { get => hand.cards.Count;}
    [field: SerializeField, HorizontalGroup("Resources")] public int souls { get; private set; } //Надо заменить на свою колоду душ
    
    [field: SerializeField, HorizontalGroup("PlayCount")] public int lootPlayCount { get; set; }
    [field: SerializeField, HorizontalGroup("PlayCount")] public int buyCount { get; set; }
    [field: SerializeField, HorizontalGroup("PlayCount")] public int attackCount { get; set; }
    [field: SerializeField, HorizontalGroup("Max")] public int lootPlayMax { get; set; } = 1;
    [field: SerializeField, HorizontalGroup("Max")] public int buyMax { get; set; } = 1;
    [field: SerializeField, HorizontalGroup("Max")] public int attackMax { get; set; } = 1;
    public int shopPrice = 10;
    //Объекты
    public CharacterCard CharacterCard;
    public Hand hand;
    public List<Card> Items = new List<Card>();
    public List<Card> Curses = new List<Card>();
    //Всякие события
    public event Action onTurnStart;  
    public event Action onTurnEnd;

    public void Init(CharacterCard c, ItemCard i, Hand hand)
    {
        c.transform.parent = transform;
        i.transform.parent = transform;

        CharacterCard = c;
        AddItem(i);
        this.hand = hand;
        
        CharacterCardData ccd = CharacterCard.data as CharacterCardData;
        hpMax = ccd.hp;
        hp = ccd.hp;
        attack = ccd.attack;
        coins = 0;
        lootPlayCount = 0;
        souls = ccd.startSouls;
    }
    public void TakeOneCard(LootCard card)
    {
        hand.AddCard(card);
    }
    public void StartTurn()
    {
        SetBaseStats();
        SetPassiveItems();

        RechargeAllCards();
        lootPlayCount = lootPlayMax;

        onTurnStart?.Invoke();
        hand.AddCard(Card.CreateCard<LootCard>(GameMaster.inst.lootDeck.TakeOneCard(),true));

        UIOnDeck.inst.UpdateTexts();
    }
    public void EndTurn()
    {
        onTurnEnd?.Invoke();
    }
    public void RechargeAllCards()
    {
        CharacterCard.Recharge();
        for(int i = 0; i < Items.Count; i++)
        {
            (Items[i] as ItemCard)?.Recharge();
        }
    }
    public void PlayCard(LootCard c)
    {
        if(lootPlayCount > 0) 
        {
            hand.PlayCard(c);
            lootPlayCount--;
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
                Items[i] = c;
                c.transform.parent = transform;
                c.transform.DOMove(CardPlaces.inst.playersPos[GameMaster.inst.turnManager.priorId][i+1].position, GameMaster.CARDSPEED);
                return;
            }
        }
        Items.Add(c);
        c.transform.parent = transform;
        c.transform.DOMove(CardPlaces.inst.playersPos[GameMaster.inst.turnManager.priorId][Items.Count].position, GameMaster.CARDSPEED);
        c.transform.localScale = CardPlaces.inst.playersPos[GameMaster.inst.turnManager.priorId][Items.Count+1].lossyScale;
        
        if((c.data as ItemCardData) != null)
        {
            List<ItemEffect> effects = (c.data as ItemCardData).effects;
            for(int j = 0; j < effects.Count; j++)
            {
                if(effects[j].type == ItemEffectType.Passive && effects[j].effect.target == Target.Me)
                {
                    effects[j].effect.result.Invoke();
                    UIOnDeck.inst.UpdateTexts();
                }
            }
        }
        else if((c.data as LootCardData) != null)
        {

        }  
    }
    public void Die()
    {
        //Отменяет все действия, там атаку
        //Вызывает состояние оплаты смерти, игрок выбирает всё-всё
        //Закончить ход
        if(GameMaster.inst.turnManager.activePlayer == this) GameMaster.inst.turnManager.SwitchTurn();
    }
    public void SetBaseStats()
    {
        hp = hpMax;
        shopPrice = 10;

        lootPlayCount = GameMaster.inst.turnManager.activePlayer == this ? lootPlayMax : 0;
        buyCount = GameMaster.inst.turnManager.activePlayer == this ? buyMax : 0;
        attackCount = GameMaster.inst.turnManager.activePlayer == this ? attackMax : 0;
    }
    public void SetPassiveItems()
    {
        List<ItemCardData> datas = new List<ItemCardData>();
        for(int i = 0; i < Items.Count; i++) 
            datas.Add(Items[i].data as ItemCardData);
        
        for(int i = 0; i < datas.Count; i++)
        {
            List<ItemEffect> effects = datas[i]?.effects;
            for(int j = 0; j < effects?.Count; j++)
            {
                if(effects[j]?.type == ItemEffectType.Passive && effects[j]?.effect.target == Target.Me)
                {
                    effects[j]?.effect.result.Invoke();
                    UIOnDeck.inst.UpdateTexts();
                }
            }
        }
    }
    public void Damage(int count)
    {
        if(hp == 0) return;
        
        hp -= count;
        if(hp <= 0) 
        {
            hp = 0;
            Die();
        }
    }
    public bool PermitBuy()
    {
        if(buyCount <= 0 || coins < shopPrice) return false;
        else
        {
            coins -= shopPrice;
            buyCount -= 1;
            return true;
        }
    }
    public void ChangeMoney(int count)
    {
        coins += count;
    }
}
