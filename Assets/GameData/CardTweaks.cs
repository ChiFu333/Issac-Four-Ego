using UnityEngine;
[CreateAssetMenu(fileName = "Tweaks", menuName = "Cards/Tweaks", order = 51)]
public class CardTweaks : ScriptableObject
{
    public void AddCoins(int count)
    {
        GameMaster.inst.turnManager.priorPlayer.ChangeMoney(count);
        UIOnDeck.inst.UpdateTexts();
    }
    public void AddLootCard(int count)
    {
        for(int i = 0; i < count; i++) 
        {
            CardData d = GameMaster.inst.lootDeck.TakeOneCard();
            GameMaster.inst.turnManager.priorPlayer.hand.AddCard(Card.CreateCard<LootCard>(d, true));
        }
    }
    public void HealPerson(int count)
    {
        GameMaster.inst.turnManager.priorPlayer.Damage(-count);
        UIOnDeck.inst.UpdateTexts();
    }
    public void GetDiscount(int count)
    {
        GameMaster.inst.turnManager.priorPlayer.shopPrice -= count;
        UIOnDeck.inst.UpdateTexts();
    }
    public void GetAdditionalLootPlay(int count)
    {
        GameMaster.inst.turnManager.priorPlayer.lootPlayMax += count;
    }
    public void Buy()
    {
        if(!SubSystems.Inst.IsSelectingSomething) GameMaster.inst.shop.Buy();
    }
    public void CancelSelect()
    {
        if(SubSystems.Inst.IsSelectingSomething) SubSystems.Inst.CancelSelecting();
    }
    public void Agree()
    {
        CubeManager.Inst.Agree();
    }
    public void Attack()
    {
        GameMaster.inst.monsterZone.Attack();
    }
}
