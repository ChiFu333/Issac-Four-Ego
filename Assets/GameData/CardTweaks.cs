using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.InputSystem.Interactions;
using TreeEditor;
using Unity.Burst.Intrinsics;
[CreateAssetMenu(fileName = "Tweaks", menuName = "Cards/Tweaks", order = 51)]

public class CardTweaks : ScriptableObject
{
    // Изменения ресурсов игроков
    public static Task<bool> AddCoins(int count) 
    { 
        StackSystem.inst.cardTarget.GetMyPlayer().AddMoney(count);
        return Task.FromResult(true);
    }
    public static Task<bool> StealFromHim(int count)
    {
        G.Players.priorPlayer.StealMoney(count, StackSystem.inst.cardTarget.GetMyPlayer());
        UIOnDeck.inst.UpdateTexts();
        return Task.FromResult(true);
    }
    public static async Task<bool> AddLootCard(int count) 
    { 
        for(int i = 0; i < count; i++) 
        {
            await Task.Delay(1000/count < 100 ? 1000/count : 100); 
            await StackSystem.inst.cardTarget.GetMyPlayer().hand.AddCard(G.Decks.lootDeck.TakeOneCard()); 
        }
        return true;
    }
    public static async Task<bool> DiscardLootCard(int count)
    {
        Console.WriteText("Сбрось лут");
        if(StackSystem.inst.cardTarget.GetMyPlayer().lootCount == 0) return false;
        for(int i = 0; i < count; i++) 
        {
            if(StackSystem.inst.cardTarget.GetMyPlayer().lootCount == 0) break;
            Entity c = await SubSystems.inst.SelectCardByType<PlayFromHand>("MyHand"); 
            await StackSystem.inst.cardTarget.GetMyPlayer().DiscardCard(c);
        }
        return true;
    }
    public static Task<bool> AddAttack(int count)
    {
        if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.characterCard)
        {
            StackSystem.inst.cardTarget.GetMyPlayer().AddAttack(count);
        }
        else if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.monsterCard)
        {
           //StackSystem.inst.cardTarget.AddAttack(count);
        }
        return Task.FromResult(true);
    }
    public static Task<bool> AddTreasure(int count)
    {
        for(int i = 0; i < count; i++) 
        {
            Entity c = G.Decks.shopDeck.TakeOneCard();
            StackSystem.inst.cardTarget.GetMyPlayer().AddItem(c);
        }
        return Task.FromResult(true);
    }
    public static Task<bool> AddCubeModificator(int count) 
    {
        StackSystem.inst.cardTarget.GetMyPlayer().AddCubeModificator(count);
        return Task.FromResult(true);
    }
    //Хп, урон и смерти
    public static Task<bool> AddHp(int count)
    {
        if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.characterCard)
        {
            StackSystem.inst.cardTarget.GetMyPlayer().AddHp(count);
        }
        else if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.monsterCard)
        {
            //StackSystem.inst.cardTarget.ChangePreventHp(count);
        }
        return Task.FromResult(true);
    }
    public static Task<bool> AddPreventHp(int count)
    {
        if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.characterCard)
        {
            StackSystem.inst.cardTarget.GetMyPlayer().AddPreventHp(count);
        }
        else if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.monsterCard)
        {
            //StackSystem.inst.cardTarget.monster.ChangePreventHp(count);
        }
        UIOnDeck.inst.UpdateTexts();
        UIOnDeck.inst.UpdateMonsterUI();
        return Task.FromResult(true);
    }
    public static async Task<bool> Damage(int count)
    {
        await StackSystem.inst.cardTarget.GetTag<Characteristics>().Damage(count);
        UIOnDeck.inst.UpdateTexts();
        return (true);
    }
    public static async Task<bool> Kill(int count)
    {
        if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.characterCard)
        {
            await StackSystem.inst.cardTarget.GetMyPlayer().StartDieSubphase();
        }
        else if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.monsterCard)
        {
            //await StackSystem.inst.cardTarget.monster.StartMonsterDieSubphase();
        }
        return (true);
    }
    //Изменения постоянных статов
    public static Task<bool> GetAdditionalLootPlay(int count)
    {
        StackSystem.inst.cardTarget.GetMyPlayer().lootPlayCount += count;
        return Task.FromResult(true);
    }
    public static Task<bool> GetAdditionalAttackCount(int count)
    {
        StackSystem.inst.cardTarget.GetMyPlayer().attackCount += count;
        UIOnDeck.inst.UpdateTexts();
        return Task.FromResult(true);
    }
    //Заявки
    public void RequestBuy()
    {
        G.shop.StartShopSubPhase();
    }
    public void RequestAttack()
    {
        G.monsterZone.StartAttackSubPhase();
    }
    public static async Task<bool> AcceptDeath(int no)
    {
        if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.characterCard)
        {
            EntityEffects.TurnDead(StackSystem.inst.cardTarget);
            await StackSystem.inst.cardTarget.GetMyPlayer().StartDieSubphase();
        }
        else if(StackSystem.inst.cardTarget.GetTag<CardTypeTag>().cardType == CardType.monsterCard)
        {
            EntityEffects.TurnDead(StackSystem.inst.cardTarget);
            //await StackSystem.inst.cardTarget.monster.StartMonsterDieSubphase();
        }
        return true;
    }
    public static async Task<bool> RequestDamage(int count)
    {
        await StackSystem.inst.PushPrimalEffect(PrimalEffect.Damage, StackSystem.inst.cardTarget, count);
        return true;
    }
    public static async Task<bool> RequestKill(int count)
    {
        await StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, StackSystem.inst.cardTarget);
        return true;
    }
    //Кнопки и действия
    public static Task<bool> GetDiscount(int count)
    {
        StackSystem.inst.cardTarget.GetMyPlayer().shopPrice -= count;
        UIOnDeck.inst.UpdateAddInfo();
        return Task.FromResult(true);
    }
    public void Agree()
    {
        if(StackSystem.inst.stack.Count == 0) StackSystem.inst.prioreNow = false;
        _ = StackSystem.inst.AgreeEffect();
    }
    public static Task<bool> Buy(int count)
    {
        if(!SubSystems.inst.isSelectingSomething) 
        {
            G.shop.StartShopSubPhase();
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
    public static Task<bool> Attack(int count)
    {
        G.monsterZone.StartAttackSubPhase();
        return Task.FromResult(true);
    }
    public static void CancelSelect(int count)
    {
        if(SubSystems.inst.isSelectingSomething) SubSystems.inst.CancelSelecting();
    }
    //Кубики
    public static Task<bool> RethrowDice(int count)
    {
        StackSystem.inst.GetCubeInStack(false)?.RethrowDice();
        return Task.FromResult(true);
    }
    public static Task<bool> RechargeItem(int count)
    {
        Entity card = StackSystem.inst.cardTarget;
        card.GetTag<Tappable>().Recharge();
        return Task.FromResult(true);
    }
    public static Task<bool> ChangeAllPlayerItemCharge(int boolLike)
    {
        StackSystem.inst.cardTarget.GetMyPlayer().ChangeAllPlayerItemCharge(boolLike == 1);
        return Task.FromResult(true);
    }
    public static Task<bool> ChangeCubeCount(int count) 
    {
        StackSystem.inst.GetCubeInStack(false)?.ChangeCubeCount(count); 
        return Task.FromResult(true);
    }
    public static Task<bool> ChangeToCubeCount(int count) 
    { 
        StackSystem.inst.GetCubeInStack(false)?.ChangeToCount(count);
        return Task.FromResult(true);
    }
    public async static Task<bool> DestroyCurse(int count)
    {
        bool b = await StackSystem.inst.cardTarget.GetMyPlayer().DestroyCurse(StackSystem.inst.cardTarget);
        return b;
    }
    public static async Task<bool> StashMonster(int count)
    {
        await G.monsterZone.StashSlot(StackSystem.inst.cardTarget);
        return true;
    }
    public static async Task<bool> CancelTopStackEffect(int count)
    {
        StackEffect eff = StackSystem.inst.stack.Peek();
        await eff.RemoveMeFromStack();
        return true;
    }
    public static Task<bool> TurnIntoItem(int no)
    {
        Entity loot = StackSystem.inst.cardTarget;
        //loot.TurnIntoItem();
        StackSystem.inst.cardTarget.GetMyPlayer().AddItem(StackSystem.inst.cardTarget);
        return Task.FromResult(true);
    }
    public async static Task<bool> TurnAndGiveCurse(int no)
    {
        /*
        EventCard eve = StackSystem.inst.cardTarget as EventCard;
        eve.TurnIntoCurse();
        Console.WriteText("Отдай кому-то проклятие");
        CharacterCard t = await SubSystems.inst.SelectCardByType<CharacterCard>("InPlay");
        t.GetMyPlayer().AddCurse(eve);
        return true;*/
        return true;
    }
    public static Task<bool> IncreaseShop(int count)
    {
        G.shop.IncreaseShop(count);
        return Task.FromResult(true);
    }
    public static Task<bool> IncreaseMonsterZone(int count)
    {
        G.monsterZone.IncreaseZone(count);
        return Task.FromResult(true);
    }
    public static Task<bool> EndTurn(int no)
    {
        _ = GameMaster.inst.phaseSystem.StartEndPhase();
        return Task.FromResult(true);
    }
    public static async Task<bool> CancelEverythingInStack(int no)
    {
        await StackSystem.inst.CancelEverythingInStack();
        return true;
    }
    public static Dictionary<ActionType, Func<int, Task<bool>>> GetDelegate = new Dictionary<ActionType, Func<int,Task<bool>>>()
    {
        {ActionType.AddCoins, AddCoins},
        {ActionType.StealCoinsFromPrior, StealFromHim},
        {ActionType.AddLootCard, AddLootCard},
        {ActionType.DiscardLootCard, DiscardLootCard},
        {ActionType.AddAttack, AddAttack},
        {ActionType.AddTreasure, AddTreasure},
        {ActionType.AddCubeModificator, AddCubeModificator},
        {ActionType.AddHp, AddHp},
        {ActionType.AddPreventHp, AddPreventHp},
        {ActionType.Damage, Damage},
        {ActionType.Kill, Kill},
        {ActionType.GetAdditionalLootPlay, GetAdditionalLootPlay},
        {ActionType.GetAdditionalAttackCount, GetAdditionalAttackCount},    
        {ActionType.RequestDamage, RequestDamage},
        {ActionType.RequestKill, RequestKill},
        {ActionType.GetDiscount, GetDiscount},
        {ActionType.Buy, Buy},
        {ActionType.Attack, Attack},
        {ActionType.RethrowDice, RethrowDice},
        {ActionType.RechargeItem, RechargeItem},
        {ActionType.ChangeAllPlayerItemCharge, ChangeAllPlayerItemCharge},
        {ActionType.ChangeCubeCount, ChangeCubeCount},
        {ActionType.ChangeToCubeCount, ChangeToCubeCount},
        {ActionType.DestroyCurse, DestroyCurse},
        {ActionType.StashMonster, StashMonster},
        {ActionType.CancelTopEffect, CancelTopStackEffect},
        {ActionType.TurnIntoItem, TurnIntoItem},
        {ActionType.IncreaseShop, IncreaseShop},
        {ActionType.IncreaseMonsterZone, IncreaseMonsterZone},
        {ActionType.AcceptDeath, AcceptDeath},
        {ActionType.EndTurn, EndTurn},
        {ActionType.CancelEverythingInStack, CancelEverythingInStack},
        {ActionType.TurnIntoCurseAndGive, TurnAndGiveCurse},
    };
}
public enum ActionType
{
    AddCoins = 0,
    StealCoinsFromPrior = 1,
    AddLootCard = 2,
    DiscardLootCard = 3,
    AddAttack = 4,
    AddTreasure = 5,
    AddCubeModificator = 6,
    AddHp = 7,
    AddPreventHp = 8,
    Damage = 9,
    Kill = 10,
    GetAdditionalLootPlay = 11,
    GetAdditionalAttackCount = 12,
    RequestAttack = 14,
    RequestDamage = 15,
    RequestKill = 16,
    GetDiscount = 17,
    Buy = 18,
    Attack = 19,
    RethrowDice = 20,
    RechargeItem = 21,
    ChangeAllPlayerItemCharge = 22,
    ChangeCubeCount = 23,
    ChangeToCubeCount = 24,
    DestroyCurse = 25,
    StashMonster = 26,
    CancelTopEffect = 27,
    TurnIntoItem = 28,
    IncreaseShop = 29,
    IncreaseMonsterZone = 30,
    AcceptDeath = 31,
    EndTurn = 32,
    CancelEverythingInStack = 33,
    TurnIntoCurseAndGive = 34,
}
