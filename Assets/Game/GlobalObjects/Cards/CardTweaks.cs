using UnityEngine;
[CreateAssetMenu(fileName = "Tweaks", menuName = "Cards/Tweaks", order = 51)]
public class CardTweaks : ScriptableObject
{
    public void AddMeCoins(int count)
    {
        TurnManager.Inst.GetPriorPlayer().Coins += count;
        UIOnDeck.Inst.UpdateTexts();
    }
    public void AddLootCardActivePlayer()
    {
        TurnManager.Inst.GetPriorPlayer().Hand.AddCard(Card.CreateCard<LootCard>(GameMaster.Inst.LootDeck.TakeOneCard(), TurnManager.Inst.ActivePlayer == TurnManager.Inst.Players[0]));
    }
    public void Buy()
    {
        if(!SubSystems.Inst.IsSelectingSomething) Shop.Inst.Buy();
    }
}
