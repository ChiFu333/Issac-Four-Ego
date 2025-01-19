using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using UnityEngine.TextCore.Text;

public class Player : MonoBehaviour
{
    public event Action onTurnStart;  
    public event Action onTurnEnd;
    public Card GetMyCard() => characterCard;

    #region [ Init & Turns ]
    public CharacterCard characterCard { get; private set; }
    public Hand hand { get; private set; }
    public void Init(Hand hand)
    {
        characterCard = Card.CreateCard<CharacterCard>((CharacterCardData)GameMaster.inst.characterDeck.TakeOneCard());
        characterCard.MoveTo(CardPlaces.inst.playersPos[GameMaster.inst.turnManager.GetMyId(this)][0], transform);
        characterCard.Flip();

        ItemCard it = Card.CreateCard<ItemCard>(characterCard.GetData<CharacterCardData>().characterItemData); 
        AddItem(it);
        it.Flip();

        
        this.hand = hand;

        HpMax = characterCard.GetData<CharacterCardData>().hp;
        attack = characterCard.GetData<CharacterCardData>().attack;
        coins = 0;
        lootPlayCount = 0;
        souls = characterCard.GetData<CharacterCardData>().startSouls;
    }
    public void StartTurn()
    {
        SetBaseStats();
        SetPassiveItems();

        ChangeAllPlayerItemCharge(true);
        lootPlayCount = lootPlayMax;

        onTurnStart?.Invoke();
        hand.AddCard(Card.CreateCard<LootCard>(GameMaster.inst.lootDeck.TakeOneCard(),true));

        UIOnDeck.inst.UpdateTexts();
    }
    public async Task EndTurn()
    {
        onTurnEnd?.Invoke();
        if(lootCount > 10)
        {
            for(int i = 0; i < lootCount - 10; i++) 
            {
                Console.WriteText("Сбрось до 10 карт лута");
                LootCard c = await SubSystems.inst.SelectCardByType<LootCard>("MyHand"); 
                DiscardCard(c);
            }
        }
    }
    public void SetBaseStats()
    {
        HpMax = characterCard.GetData<CharacterCardData>().hp;
        preventHp = 0;
        shopPrice = 10;
        cubeModificator = 0;

        lootPlayCount = GameMaster.inst.turnManager.activePlayer == this ? lootPlayMax : 0;
        buyCount = GameMaster.inst.turnManager.activePlayer == this ? buyMax : 0;
        attackCount = GameMaster.inst.turnManager.activePlayer == this ? attackMax : 0;
    }
    
    #endregion
    
    #region [ HP, heal, damage & death ]
    [field: SerializeField, HorizontalGroup("HP")] public int hp { get; private set; }
    public int HpMax
    { 
        get => hpMax;
        set
        {
            int delta = value - hpMax;
            hpMax += delta;
            if(delta > 0)
            {
                hp += delta;
            }
            else if(delta < 0)
            {
                hp = hpMax < hp ? hpMax : hp;
            }
            UIOnDeck.inst.UpdateTexts();
        }
    }
    private int hpMax;
    [field: SerializeField, HorizontalGroup("HP")] public int preventHp { get; private set; }
    public void AddHp(int count) 
    {
        HpMax += count;
        UIOnDeck.inst.UpdateTexts();
    }
    public void Damage(int count)
    {
        int damageCount = count;
        if(hp == 0) return;
        
        if(damageCount > preventHp)
        {
            damageCount -= preventHp;
            preventHp = 0;
            hp -= damageCount;
        }
        else
        {
            preventHp -= damageCount;
        }

        if(hp <= 0) 
        {
            hp = 0;
            Die();
        }
        return;
    }
    public void HealHp(int count)
    {
        if(hp + count > HpMax)
            hp = HpMax;
        else
            hp += count;
        UIOnDeck.inst.UpdateTexts();
    }
    public bool PayHp(int count)
    {
        if(hp - count < 0)
            return false;
        else
            hp -= count;
        if(hp == 0) Die();
        UIOnDeck.inst.UpdateTexts();
        return true;
    }
    public void AddPreventHp(int count)
    {
        preventHp += count;
        UIOnDeck.inst.UpdateTexts();
    }
    public void Die()
    {
        //Отменяет все действия, там атаку
        //Вызывает состояние оплаты смерти, игрок выбирает всё-всё
        //Закончить ход;
        hp = 0;
        UIOnDeck.inst.UpdateTexts();
        Console.WriteText("Игрок умер");
        //if(GameMaster.inst.turnManager.activePlayer == this) GameMaster.inst.monsterZone.CancelAttack();
        characterCard.Flip();
        ChangeAllPlayerItemCharge(false);
        if(GameMaster.inst.turnManager.activePlayer == this) GameMaster.inst.turnManager.SwitchTurn();
    }
    #endregion
    
    #region [ Money ]
    [field: SerializeField, HorizontalGroup("Money")] public int coins { get; private set; }
    public void AddMoney(int count)
    {
        coins += count;
        if(coins < 0) coins = 0;
        UIOnDeck.inst.UpdateTexts();
    }
    public void StealMoney(int count, Player victim)
    {
        int stealCount = victim.coins > count ? count : victim.coins;
        victim.AddMoney(-stealCount);
        AddMoney(stealCount);
    }
    #endregion
    
    #region [ LootCards ]
    public int lootCount { get => hand.cards.Count;}
    [field: SerializeField, HorizontalGroup("Loot")] public int lootPlayCount { get; set; }
    [field: SerializeField, HorizontalGroup("Loot")] public int lootPlayMax { get; set; } = 1;
    public void TakeOneCard(LootCard card)
    {
        hand.AddCard(card);
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
            Console.WriteText("Ты не можешь играть лут");
        }
    }
    public void DiscardCard(LootCard c) 
    { 
        hand.DiscardCard(c); 
    }
    #endregion
    
    #region [ Shop & Purchase ]
    public int shopPrice = 10;
    [field: SerializeField, HorizontalGroup("Shop")] public int buyCount { get; set; }
    [field: SerializeField, HorizontalGroup("Shop")] public int buyMax { get; set; } = 1;
    public bool PermitBuy()
    {
        
        if(buyCount <= 0 || coins < shopPrice) return false;
        else
        {
            coins -= shopPrice;
            return true;
        }
    }
    #endregion

    #region [ Attack ]
    [field: SerializeField, HorizontalGroup("Attack")] public int attack { get; private set; }
    [field: SerializeField, HorizontalGroup("Attack")] public int attackCount { get; set; }
    [field: SerializeField, HorizontalGroup("Attack")] public int attackMax { get; set; } = 1;
    public int attackCubeModificator { get; set; } = 0;
    public void AddAttack(int count)  
    {
        attack += count;
        UIOnDeck.inst.UpdateTexts();
    }
    #endregion
    
    #region [ Items ]
    public List<Card> Items = new List<Card>();
    public void ChangeAllPlayerItemCharge(bool charge)
    {
        if(charge) characterCard.Recharge();
        else characterCard.Flip();
        for(int i = 0; i < Items.Count; i++)
        {
            if(Items[i] is ItemCard it && it.IsFlippable)
            {
                if(charge)
                    it.Recharge();
                else
                    it.Flip();
            }
        }
    }
    public void AddItem(Card c)
    { 
        for(int i = 0; i < Items.Count; i++)
        {
            if(Items[i] == null) 
            {
                Items[i] = c;
                c.MoveTo(CardPlaces.inst.playersPos[GameMaster.inst.turnManager.GetMyId(this)][i+1], transform);
                return;
            }
        }
        Items.Add(c);
        c.MoveTo(CardPlaces.inst.playersPos[GameMaster.inst.turnManager.GetMyId(this)][Items.Count], transform);
    }
    public void SetPassiveItems()
    {

    }
    #endregion
    
    #region [ Curses ]
    public List<Card> Curses = new List<Card>();
    public void AddCurse(Card c)
    {
        for(int i = 0; i < Curses.Count; i++)
        {
            if(Curses[i] == null) 
            {
                Curses[i] = c;
                c.MoveTo(CardPlaces.inst.playersCurses[GameMaster.inst.turnManager.GetMyId(this)][i], transform);
                return;
            }
        }
        Curses.Add(c);
        c.MoveTo(CardPlaces.inst.playersCurses[GameMaster.inst.turnManager.GetMyId(this)][Curses.Count-1], transform);
    }
    #endregion
    
    #region [ Cube Modificators ]
    public int cubeModificator { get; set; } = 0;
    public void AddCubeModificator(int count) => cubeModificator += count;
    #endregion
    
    #region [ Souls ]
    [field: SerializeField, HorizontalGroup("Souls")] public int souls { get; private set; } //Надо заменить на свою колоду душ
    #endregion
}