using System;
using System.Threading.Tasks;
using UnityEngine;
[CreateAssetMenu(fileName = "Tweaks", menuName = "Cards/Tweaks", order = 51)]
public class CardTweaks : ScriptableObject
{
    public void AddCoins(int count)
    {
        GameMaster.inst.turnManager.cardTarget.GetMyPlayer().AddMoney(count);
    }
    public void AddLootCard(int count)
    {
        for(int i = 0; i < count; i++) 
        {
            CardData d = GameMaster.inst.lootDeck.TakeOneCard();
            GameMaster.inst.turnManager.cardTarget.GetMyPlayer().hand.AddCard(Card.CreateCard<LootCard>(d, true));
        }
    }
    public async void DiscardLootCard(int count)
    {
        for(int i = 0; i < count; i++) 
        {
            if(GameMaster.inst.turnManager.cardTarget.GetMyPlayer().lootCount == 0) return;
            LootCard c = await SubSystems.inst.SelectCardByType<LootCard>("MyHand"); 
            GameMaster.inst.turnManager.cardTarget.GetMyPlayer().DiscardCard(c);
        }
    }
    public void AddPreventHP(int count)
    {
        if(GameMaster.inst.turnManager.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().AddPreventHp(count);
        }
        else if(GameMaster.inst.turnManager.cardTarget is MonsterCard monster)
        {
            monster.ChangePreventHp(count);
        }
        UIOnDeck.inst.UpdateTexts();
        UIOnDeck.inst.UpdateMonsterUI();
    }
    public void GetDiscount(int count)
    {
        GameMaster.inst.turnManager.cardTarget.GetMyPlayer().shopPrice -= count;
        UIOnDeck.inst.UpdateTexts();
    }
    public void GetAdditionalLootPlay(int count)
    {
        GameMaster.inst.turnManager.cardTarget.GetMyPlayer().lootPlayMax += count;
    }
    public void Buy()
    {
        if(!SubSystems.inst.isSelectingSomething) GameMaster.inst.shop.Buy();
    }
    public void CancelSelect()
    {
        if(SubSystems.inst.isSelectingSomething) SubSystems.inst.CancelSelecting();
    }
    public void Agree()
    {
        CubeManager.inst.Agree();
    }
    public void Attack()
    {
        GameMaster.inst.monsterZone.Attack();
    }
    public void Damage(int count)
    {
        if(GameMaster.inst.turnManager.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().Damage(count);
        }
        else if(GameMaster.inst.turnManager.cardTarget is MonsterCard monster)
        {
            monster.Damage(count);
        }
        UIOnDeck.inst.UpdateTexts();
        UIOnDeck.inst.UpdateMonsterUI();
    }
    public void RethrowDice()
    {
        CubeManager.inst.RethrowDice();
    }
    public void RechargeItem()
    {
        ItemCard card = (ItemCard)GameMaster.inst.turnManager.cardTarget;
        card.Recharge();
    }
    public void ChangeAllPlayerItemCharge(bool charge)
    {
        GameMaster.inst.turnManager.cardTarget.GetMyPlayer().ChangeAllPlayerItemCharge(charge);
    }
    public void ChangeCubeCount(int count) => CubeManager.inst.ChangeCount(count);
    public void AddHp(int count)
    {
        if(GameMaster.inst.turnManager.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().AddHp(count);
        }
        else if(GameMaster.inst.turnManager.cardTarget is MonsterCard monster)
        {
            monster.ChangePreventHp(count);
        }
    }
    public void AddAttack(int count)
    {
        if(GameMaster.inst.turnManager.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().AddAttack(count);
        }
        else if(GameMaster.inst.turnManager.cardTarget is MonsterCard monster)
        {
            monster.AddAttack(count);
        }
    }
    public void ChangeToCubeCount(int count) => CubeManager.inst.ChangeToCount(count);
    public void AddCubeModificator(int count) => GameMaster.inst.turnManager.cardTarget.GetMyPlayer().AddCubeModificator(count);
    public void AddTreasure(int count)
    {
        for(int i = 0; i < count; i++) 
        {
            CardData d = GameMaster.inst.shopDeck.TakeOneCard();
            GameMaster.inst.turnManager.cardTarget.GetMyPlayer().AddItem(Card.CreateCard<ItemCard>(d, true));
        }
    }
    public void ChangeAttackCount(int count)
    {
        GameMaster.inst.turnManager.cardTarget.GetMyPlayer().attackCount += count;
        UIOnDeck.inst.UpdateTexts();
    }
    public void Kill()
    {
        if(GameMaster.inst.turnManager.cardTarget is CharacterCard player)
        {
            player.GetMyPlayer().Die();
        }
        else if(GameMaster.inst.turnManager.cardTarget is MonsterCard monster)
        {
            monster.Die();
        }
    }
    public void StealFromHim(int count)
    {
        GameMaster.inst.turnManager.priorPlayer.StealMoney(count, GameMaster.inst.turnManager.cardTarget.GetMyPlayer());
        UIOnDeck.inst.UpdateTexts();
    }
}
