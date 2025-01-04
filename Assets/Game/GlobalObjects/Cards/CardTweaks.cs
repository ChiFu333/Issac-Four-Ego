using UnityEngine;
[CreateAssetMenu(fileName = "Tweaks", menuName = "Cards/Tweaks", order = 51)]
public class CardTweaks : ScriptableObject
{
    public void AddCoins(int count)
    {
        TurnManager.Inst.GetPriorPlayer().Coins += count;
        UIOnDeck.Inst.UpdateTexts();
    }
    public void AddLootCard(int count)
    {
        for(int i = 0; i < count; i++) 
        {
            CardData d = GameMaster.Inst.LootDeck.TakeOneCard();
            TurnManager.Inst.GetPriorPlayer().Hand.AddCard(Card.CreateCard<LootCard>(d, TurnManager.Inst.ActivePlayer == TurnManager.Inst.Players[0]));
        }
    }
    public void HealPerson(int count)
    {
        TurnManager.Inst.GetPriorPlayer().Hp += count;
        UIOnDeck.Inst.UpdateTexts();
    }
    public void GetDiscount(int count)
    {
        TurnManager.Inst.GetPriorPlayer().ShopPrice -= count;
        UIOnDeck.Inst.UpdateTexts();
    }
    public void GetAdditionalLootPlay(int count)
    {
        TurnManager.Inst.GetPriorPlayer().LootPlayMax += count;
    }
    public void Buy()
    {
        if(!SubSystems.Inst.IsSelectingSomething) Shop.Inst.Buy();
    }
    public void CancelSelect()
    {
        if(SubSystems.Inst.IsSelectingSomething) SubSystems.Inst.CancelSelecting();
    }
}
