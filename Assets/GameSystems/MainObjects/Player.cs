using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using UnityEngine.TextCore.Text;

public class Player : MonoBehaviour
{
    public Entity GetMyCard() => characterCard;

    #region [ Init & Turns ]
    public Entity characterCard { get; private set; }
    public Hand hand { get; private set; }
    public async Task Init(Hand hand)
    {
        characterCard = G.Decks.characterDeck.TakeOneCard();
        characterCard.MoveTo(G.CardPlaces.playersPos[G.Players.GetPlayerId(this)][0], transform);
        characterCard.GetTag<Tappable>().Tap();

        Entity it = Entity.CreateEntity(characterCard.GetTag<CharacterItemPrefab>().itemPrefab); 
        it.SetActive(true);
        AddItem(it);
        if(it.HasTag<Tappable>()) it.GetTag<Tappable>().Tap();
        
        this.hand = hand;

        hpMax = characterCard.GetTag<Characteristics>().health;
        attack = characterCard.GetTag<Characteristics>().attack;
        coins = 0;
        lootPlayCount = 0;
        souls = 0;
        await Task.Delay(100);
    }
    public void SetBaseStats()
    {
        HpMax = characterCard.GetTag<Characteristics>().health;
        attack = characterCard.GetTag<Characteristics>().attack;
        preventHp = 0;
        shopPrice = 10;
        cubeModificator = 0;

        lootPlayCount = 0;
        buyCount = 0;
        attackCount = 0;
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
                HealHp(delta);
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
    public bool isDead { get; set; } = false;
    public void AddHp(int count) 
    {
        HpMax += count;
        UIOnDeck.inst.UpdateTexts();
    }
    public async Task Damage(int count)
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
        GetMyCard().GetTag<Characteristics>().ChangeHp(hp);
        if(hp <= 0) 
        {
            hp = 0;
            if(!isDead) await StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, GetMyCard());
        }
        return;
    }
    public void HealHp(int count, bool throughDeath = false)
    {
        if(hp == 0 && !throughDeath) return;
        if(hp + count > HpMax)
            hp = HpMax;
        else
            hp += count;
        UIOnDeck.inst.UpdateTexts();
    }
    public async Task<bool> PayHp(int count)
    {
        if(hp - count < 0)
            return false;
        else
            hp -= count;
        if(hp == 0 && !isDead) await StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, GetMyCard());;
        UIOnDeck.inst.UpdateTexts();
        return true;
    }
    public void AddPreventHp(int count)
    {
        preventHp += count;
        UIOnDeck.inst.UpdateTexts();
    }
    public async Task StartDieSubphase()
    {
        await GameMaster.inst.phaseSystem.StartPlayerDie(this);
    }
    #endregion
    
    #region [ Money ]
    [field: SerializeField, HorizontalGroup("Money")] public int coins { get; private set; }
    public void AddMoney(int count)
    {
        coins += count;
        if(coins < 0) coins = 0;
        UIOnDeck.inst.UpdateTexts(G.Players.GetPlayerId(this));
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
    public void TakeOneLootCard(Entity card)
    {
        hand.AddCard(card);
    }
    public void PlayLootCard(Entity c)
    {
        if(lootPlayCount > 0) 
        {
            _ = hand.PlayCard(c);
            lootPlayCount--;
        }
        else
        {
            Console.WriteText("Ты не можешь играть лут");
        }
    }
    public async Task DiscardCard(Entity c) 
    { 
        await hand.DiscardCard(c); 
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
    public List<Entity> Items = new List<Entity>();
    public void ChangeAllPlayerItemCharge(bool charge)
    {
        if(charge) characterCard.GetTag<Tappable>().Recharge();
        else characterCard.GetTag<Tappable>().Tap();
        for(int i = 0; i < Items.Count; i++)
        {
            if(Items[i].HasTag<Tappable>())
            {
                if(charge)
                    Items[i].GetTag<Tappable>().Recharge();
                else
                    Items[i].GetTag<Tappable>().Tap();
            }
        }
    }
    public void AddItem(Entity c)
    {
        for(int i = 0; i < Items.Count; i++)
        {
            if(Items[i] == null) 
            {
                Items[i] = c;
                c.MoveTo(G.CardPlaces.playersPos[G.Players.GetPlayerId(this)][i+1], transform);
                PutCardTrigger(c);
                return;
            }
        }
        Items.Add(c);
        c.MoveTo(G.CardPlaces.playersPos[G.Players.GetPlayerId(this)][Items.Count], transform);
        PutCardTrigger(c);
    }
    public void SetPassiveItems()
    {
        for(int i = 0; i < Items.Count; i++)
        {
            if(Items[i] != null) PutCardTrigger(Items[i]);
        }
        for(int i = 0; i < Curses.Count; i++)
        {
            if(Curses[i] != null) PutCardTrigger(Curses[i]);
        }
    }
    private void PutCardTrigger(Entity c)
    {
        StackEffect triggeredEffect = null;
        /*
        if(c is LootCard lootItem)
        {
            triggeredEffect = new CardStackEffect(lootItem.GetData<LootCardData>().GetTrinketEffect(), c);
        }
        else if(c is ItemCard itemCard && itemCard.GetData<ItemCardData>().GetPassiveEffect() != null)
        {
            triggeredEffect = new CardStackEffect(itemCard.GetData<ItemCardData>().GetPassiveEffect(), c);
        }
        else if(c is EventCard eventCard && eventCard.GetData<EventCardData>().GetPassiveEffect() != null)
        {
            triggeredEffect = new CardStackEffect(eventCard.GetData<EventCardData>().GetPassiveEffect(), c);
        }
        */
        TriggersSystem.PutTrigger(triggeredEffect, G.Players.GetPlayerId(this) + 1);
    }
    #endregion
    
    #region [ Curses ]
    public List<Entity> Curses = new List<Entity>();
    public void AddCurse(Entity c)
    {
        for(int i = 0; i < Curses.Count; i++)
        {
            if(Curses[i] == null) 
            {
                Curses[i] = c;
                c.MoveTo(G.CardPlaces.playersCurses[G.Players.GetPlayerId(this)][i], transform);
                PutCardTrigger(c);
                return;
            }
        }
        Curses.Add(c);
        c.MoveTo(G.CardPlaces.playersCurses[G.Players.GetPlayerId(this)][Curses.Count-1], transform);
        PutCardTrigger(c);
    }
    public async Task<bool> DestroyCurse(Entity c)
    {
        bool t = false;
        for(int i = 0; i < Curses.Count; i++)
        {
            if(Curses[i] == c) 
            {
                Curses[i] = null;
                t = true;
                break;
            }
        }
        if(t)
        {
            await c.DiscardEntity();
            return true;
        }
        return false;
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