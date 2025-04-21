using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;

public class Player : MonoBehaviour
{
    public Entity GetMyCard() => characterCard;

    #region [ Init & Turns ]
    public Entity characterCard { get; private set; }
    public Hand hand { get; private set; }
    public async UniTask Init(Hand hand)
    {
        characterCard = G.Decks.characterDeck.TakeOneCard();
        characterCard.MoveTo(G.CardPlaces.playersPos[G.Players.GetPlayerId(this)][0], transform);
        characterCard.GetTag<Tappable>().Tap();

        Entity it = Entity.CreateEntity(characterCard.GetTag<CharacterItemPrefab>().itemPrefab); 
        it.SetActive(true);
        AddItem(it);
        if(it.HasTag<Tappable>()) it.GetTag<Tappable>().Tap();
        
        this.hand = hand;
        characteristics = characterCard.GetTag<Characteristics>();
        attack = characterCard.GetTag<Characteristics>().attack;
        coins = 10;
        lootPlayCount = 0;
        souls = 0;
        addDamageToBattleThrow = new List<int> { 0, 0, 0, 0, 0 };
        AddHp(10);
        await UniTask.Delay(100);
    }
    public void SetBaseStats()
    {
        shopPrice = 10;
        cubeModificator = 0;

        attackThrowCount = 0;
        
        buyCount = 0;
        attackCount = 0;
        
        SetStartStatics();
    }

    private void SetStartStatics()
    {
        return;
    }
    #endregion
    
    #region [ HP, heal, damage & death ]
    public Characteristics characteristics;
    public void AddHp(int count) 
    {
        characteristics.AddHp(count);
        UIOnDeck.inst.UpdateTexts();
    }
    public async UniTask Damage(int count)
    {
        await characteristics.Damage(count);
    }
    public async UniTask HealHp(int count, bool throughDeath = false)
    {
        await characteristics.HealHp(count, throughDeath);
        UIOnDeck.inst.UpdateTexts();
    }
    public async UniTask<bool> PayHp(int count)
    {
        return await characteristics.PayHp(count);
    }
    public void AddPreventHp(int count)
    {
        characteristics.AddPreventHp(count);
        UIOnDeck.inst.UpdateTexts();
    }
    public async UniTask StartDieSubphase()
    {
        await GameMaster.inst.phaseSystem.StartPlayerDie(this);
    }
    #endregion
    
    #region [ Money ]
    [field: SerializeField, HorizontalGroup("Money")] public int coins { get; private set; }
    public void AddMoney(int count)
    {
        coins += count + (GetMyCard().HasTag<PlusOneCoinGain>() ? 1 : 0);
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
    public int lootCount { get => hand.GetCount();}
    [field: SerializeField, HorizontalGroup("Loot")] public int lootPlayCount { get; set; }
    [field: SerializeField, HorizontalGroup("Loot")] public int lootPlayMax { get; set; } = 1;
    public int lootTakeCount { get; set; } = 1;
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
    public async UniTask DiscardCard(Entity c) 
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
        if(coins < shopPrice) return false;
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
    public int attackThrowCount = 0;
    public List<int> addDamageToBattleThrow;
    public void AddAttack(int count)  
    {
        attack += count;
        UIOnDeck.inst.UpdateTexts();
    }

    public int GetDamageCount()
    {
        return attack + (attackThrowCount < 5 ? addDamageToBattleThrow[attackThrowCount] : 0);
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
                CheckStaticEffects(c);
                return;
            }
        }
        Items.Add(c);
        c.MoveTo(G.CardPlaces.playersPos[G.Players.GetPlayerId(this)][Items.Count], transform);
        CheckStaticEffects(c);
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

    public void CheckStaticEffects(Entity c)
    {
        if(c.HasTag<PassiveTrinketEffect>())
        {
            if(c.GetTag<PassiveTrinketEffect>().effect.when == When.Always)
            {
                c.GetTag<PassiveTrinketEffect>().effect.SetTargets(GetMyCard());
                c.GetTag<PassiveTrinketEffect>().effect.PlayEffect();
            }
        }
        else if(c.HasTag<ItemPassiveEffect>())
        {
            if(c.GetTag<ItemPassiveEffect>().effect.when == When.Always)
            {
                c.GetTag<ItemPassiveEffect>().effect.SetTargets(GetMyCard());
                c.GetTag<ItemPassiveEffect>().effect.PlayEffect();
            }
        }
    }
    private void PutCardTrigger(Entity c)
    {
        StackEffect triggeredEffect = null;
        if(c.HasTag<PassiveTrinketEffect>())
        {
            triggeredEffect = new CardStackEffect(c.GetTag<PassiveTrinketEffect>().effect, c);
        }
        else if(c.HasTag<ItemPassiveEffect>())
        {
            triggeredEffect = new CardStackEffect(c.GetTag<ItemPassiveEffect>().effect, c);
        }
        /*
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
    public async UniTask<bool> DestroyCurse(Entity c)
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