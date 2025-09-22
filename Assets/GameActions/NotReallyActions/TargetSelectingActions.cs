using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

[AddTypeMenu(ActionNames.SelectingName + ActionNames.SelectingNamePlayer + "PlayerMe")]
[Serializable]
public class PlayerMe : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        container = source.GetMyPlayer().PackPlayer();
        return UniTask.FromResult(true);
    }
}
[AddTypeMenu(ActionNames.SelectingName +  ActionNames.SelectingNamePlayer + "PlayerYouSelect")]
[Serializable]
public class PlayerYouSelect : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        Card c = null;
        while (c == null)
        {
            c = await G.Main.CardSelector.SelectCardByType<Card>(source.transform, isCancelabale,
                card => 
                    card.Get<TagCardType>().cardType == CardType.characterCard
            );
            if(isCancelabale || c != null) break;
        }
        if (c == null) return false;
        container = c.GetComponentInParent<Player>().PackPlayer();
        return true;
    }
}
[AddTypeMenu(ActionNames.SelectingName +  ActionNames.SelectingNamePlayer + "PlayersAll")]
[Serializable]
public class PlayersAll : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        container = G.Main.Players.PackPlayers();
        return true;
    }
}
[AddTypeMenu(ActionNames.SelectingName +  ActionNames.SelectingNamePlayer + "PlayersAllExceptMe")]
[Serializable]
public class PlayersAllExceptMe : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        List<Player> temp = G.Main.Players.ToList();
        temp.Remove(source.GetMyPlayer());
        container = temp.PackPlayers();
        return true;
    }
}
///ДЕЕЕЕКИИИ
[AddTypeMenu(ActionNames.SelectingName + ActionNames.SelectingNameDeck + "1. LootDeck")]
[Serializable]
public class LootDeckSelect : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        container = G.Main.Decks.lootDeck.PackDeck();
        return true;
    }
}
[AddTypeMenu(ActionNames.SelectingName + ActionNames.SelectingNameDeck + "2. TreasureDeck")]
[Serializable]
public class TreasureDeckSelect : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        container = G.Main.Decks.treasureDeck.PackDeck();
        return true;
    }
}
[AddTypeMenu(ActionNames.SelectingName + ActionNames.SelectingNameDeck + "3. MonsterDeck")]
[Serializable]
public class MonsterDeckSelect : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        container = G.Main.Decks.monsterDeck.PackDeck();
        return true;
    }
}




/// ОСТАЛЬНОЕЕЕЕЕЕЕЕЕЕЕЕЕЕ
[AddTypeMenu(ActionNames.SelectingName + "ActivatingItem")]
[Serializable]
public class ActivatingItem : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        Card c = null;
        while (c == null)
        {
            c = c = await G.Main.CardSelector.SelectCardByType<Card>(source.transform, isCancelabale, 
                card => card.Get<TagCardType>().cardType == CardType.treasureCard && card.Is<TagTappable>());
            if(isCancelabale || c != null) break;
        }
        if (c == null) return false;
        container = c.PackCard();
        return true;
    }
}
[AddTypeMenu(ActionNames.SelectingName + "AnyItem")]
[Serializable]
public class AnyItem : ITargetSelector
{
    public bool containEternal;
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        Card c = null;
        while (c == null)
        {
            c = c = await G.Main.CardSelector.SelectCardByType<Card>(source.transform, isCancelabale, 
                card => card.Is<TagIsItem>() && (containEternal || !card.Is<TagIsEternal>()));
            if(isCancelabale || c != null) break;
        }
        if (c == null) return false;
        container = c.PackCard();
        return true;
    }
}
[AddTypeMenu(ActionNames.SelectingName + "AnyMyItem")]
[Serializable]
public class AnyMyItem : ITargetSelector
{
    public bool containEternal;
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        Card c = null;
        while (c == null)
        {
            c = c = await G.Main.CardSelector.SelectCardByType<Card>(source.transform, isCancelabale, 
                card => card.Is<TagIsItem>() && card.GetMyPlayer() == source.GetMyPlayer() && (containEternal || !card.Is<TagIsEternal>()));
            if(isCancelabale || c != null) break;
        }
        if (c == null) return false;
        container = c.PackCard();
        return true;
    }
}
[AddTypeMenu(ActionNames.SelectingName + "0. IT")]
[Serializable]
public class ITTarget : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        Card c = source;
        container = c.PackCard();
        return true;
    }
}
//ffffffffffffffffffffffffffff
[AddTypeMenu(ActionNames.SelectingName + "99. LootCardInStack")]
[Serializable]
public class LootCardInStackTarget : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        Card c = null;
        while (c == null)
        {
            c = c = await G.Main.CardSelector.SelectCardByType<Card>(source.transform, isCancelabale, 
                card => 
                    (card.Get<TagCardType>().cardType == CardType.lootCard && card.myStackUnit != null && card.Get<TagCardType>().cloneType == StackCardUnitType.None)
                        ||
                        (card.Get<TagCardType>().cardType == CardType.treasureCard && card.myStackUnit != null && card.Get<TagCardType>().cloneType == StackCardUnitType.ActivateValue)
                );
            if(isCancelabale || c != null) break;
        }
        if (c == null) return false;
        container = c.PackCard();
        return true;
    }
}
[AddTypeMenu(ActionNames.SelectingName + "5. Cube, triggered me")]
[Serializable]
public class StackUnitCubeSoucreTarget : ITargetSelector
{
    public List<ISelectableTarget> container { get; set; }
    public async UniTask<bool> SetTarget(Card source, bool isCancelabale = true)
    {
        container = source.stackUnitCreatedMe.PackDice();
        return true;
    }
}