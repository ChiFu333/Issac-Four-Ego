using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using Unity.VisualScripting;
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
        GameMaster.inst.turnManager.priorPlayer().StealMoney(count, StackSystem.inst.cardTarget.GetMyPlayer());
        UIOnDeck.inst.UpdateTexts();
        return Task.FromResult(true);
    }
    public static async Task<bool> AddLootCard(int count) 
    { 
        for(int i = 0; i < count; i++) 
        {
            await Task.Delay(1000/count < 200 ? 1000/count : 200); 
            StackSystem.inst.cardTarget.GetMyPlayer().hand.AddCard(Card.CreateCard<LootCard>(GameMaster.inst.lootDeck.TakeOneCard(), true));
            
        }
        return true;
    }
    public static async Task<bool> DiscardLootCard(int count)
    {
        if(StackSystem.inst.cardTarget.GetMyPlayer().lootCount == 0) return false;
        for(int i = 0; i < count; i++) 
        {
            if(StackSystem.inst.cardTarget.GetMyPlayer().lootCount == 0) break;
            LootCard c = await SubSystems.inst.SelectCardByType<LootCard>("MyHand"); 
            StackSystem.inst.cardTarget.GetMyPlayer().DiscardCard(c);
        }
        return true;
    }
    public static Task<bool> AddAttack(int count)
    {
        if(StackSystem.inst.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().AddAttack(count);
        }
        else if(StackSystem.inst.cardTarget is MonsterCard monster)
        {
            monster.AddAttack(count);
        }
        return Task.FromResult(true);
    }
    public static Task<bool> AddTreasure(int count)
    {
        for(int i = 0; i < count; i++) 
        {
            CardData d = GameMaster.inst.shopDeck.TakeOneCard();
            StackSystem.inst.cardTarget.GetMyPlayer().AddItem(Card.CreateCard<ItemCard>(d, true));
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
        if(StackSystem.inst.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().AddHp(count);
        }
        else if(StackSystem.inst.cardTarget is MonsterCard monster)
        {
            monster.ChangePreventHp(count);
        }
        return Task.FromResult(true);
    }
    public static Task<bool> AddPreventHp(int count)
    {
        if(StackSystem.inst.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().AddPreventHp(count);
        }
        else if(StackSystem.inst.cardTarget is MonsterCard monster)
        {
            monster.ChangePreventHp(count);
        }
        UIOnDeck.inst.UpdateTexts();
        UIOnDeck.inst.UpdateMonsterUI();
        return Task.FromResult(true);
    }
    public static Task<bool> Damage(int count)
    {
        if(StackSystem.inst.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().Damage(count);
        }
        else if(StackSystem.inst.cardTarget is MonsterCard monster)
        {
            monster.Damage(count);
        }
        UIOnDeck.inst.UpdateTexts();
        UIOnDeck.inst.UpdateMonsterUI();
        return Task.FromResult(true);
    }
    public static async Task<bool> Kill(int count)
    {
        if(StackSystem.inst.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().PayHp(player.GetMyPlayer().HpMax);
        }
        else if(StackSystem.inst.cardTarget is MonsterCard monster)
        {
            await monster.StartMonsterDieSubphase();
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
        GameMaster.inst.shop.StartShopSubPhase();
    }
    public void RequestAttack()
    {
        GameMaster.inst.monsterZone.StartAttackSubPhase();
    }
    public static async Task<bool> AcceptDeath(int no)
    {
        if(StackSystem.inst.cardTarget is CharacterCard player)
        {
            await player.GetMyPlayer().StartDieSubphase();
        }
        else if(StackSystem.inst.cardTarget is MonsterCard monster)
        {
            monster.StartMonsterDieSubphase();
        }
        return true;
    }
    public static Task<bool> RequestDamage(int count)
    {
        StackSystem.inst.PushPrimalEffect(PrimalEffect.Damage, StackSystem.inst.cardTarget, count);
        return Task.FromResult(true);
    }
    public static Task<bool> RequestKill(int count)
    {
        StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, StackSystem.inst.cardTarget);
        return Task.FromResult(true);;
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
        StackSystem.inst.AgreeEffect();
    }
    public static Task<bool> Buy(int count)
    {
        if(!SubSystems.inst.isSelectingSomething) 
        {
            GameMaster.inst.shop.StartShopSubPhase();
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
    public static Task<bool> Attack(int count)
    {
        GameMaster.inst.monsterZone.StartAttackSubPhase();
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
        ItemCard card = (ItemCard)StackSystem.inst.cardTarget;
        card.Recharge();
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
    public static Task<bool> DestroyCurse(int count)
    {
        StackSystem.inst.cardTarget.GetMyPlayer().DestroyCurse(StackSystem.inst.cardTarget);
        return Task.FromResult(true);
    }
    public static async Task<bool> StashMonster(int count)
    {
        await GameMaster.inst.monsterZone.StashSlot(StackSystem.inst.cardTarget);
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
        LootCard loot = StackSystem.inst.cardTarget as LootCard;
        loot.TurnIntoItem();
        StackSystem.inst.cardTarget.GetMyPlayer().AddItem(StackSystem.inst.cardTarget);
        return Task.FromResult(true);
    }
    public static Task<bool> IncreaseShop(int count)
    {
        GameMaster.inst.shop.IncreaseShop(count);
        return Task.FromResult(true);
    }
    public static Task<bool> IncreaseMonsterZone(int count)
    {
        GameMaster.inst.monsterZone.IncreaseZone(count);
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
    CancelEverythingInStack = 33
}
