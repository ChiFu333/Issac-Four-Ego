using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public static class SelectableTargetExtentions
{
    public static List<ISelectableTarget> PackPlayer(this Player player)
    {
        return new List<ISelectableTarget> { player.Get<TagBasePlayerData>().characterCard };
    }
    public static List<ISelectableTarget> PackPlayers(this List<Player> players)
    {
        return players
            .Select(target => 
            {
                return target.Get<TagBasePlayerData>().characterCard;  
            })
            .Cast<ISelectableTarget>()
            .ToList();
    }
    public static List<ISelectableTarget> PackCard(this Card card) {
        return new List<ISelectableTarget> { card };
    }
    public static List<ISelectableTarget> PackCards(this List<Card> cards) {
        return cards.Cast<ISelectableTarget>().ToList();
    }

    public static Player ConvertToPlayer(this List<ISelectableTarget> container)
    {
        return (container[0] as Card)!.GetMyPlayer();
    }
    public static List<Player> ConvertToPlayers(this List<ISelectableTarget> container)
    {
        return container
            .Select(target => 
            {
                if (target is Card card && card.Get<TagCardType>().cardType == CardType.characterCard)
                {
                    return card.GetMyPlayer();  
                }
                return null;
            })
            .Where(player => player != null) // Фильтруем null (опционально)
            .ToList();
    }
    public static List<Card> ConvertToCards(this List<ISelectableTarget> container)
    {
        return container.Cast<Card>().ToList();
    }

    public static List<ISelectableTarget> PackDeck(this Deck deck)
    {
        return new List<ISelectableTarget> { deck };
    }
    public static Deck ConvertToDeck(this List<ISelectableTarget> container)
    {
        return (container[0] as Deck);
    }
    public static StackUnit ConvertToMyStackUnit(this List<ISelectableTarget> container) {
        return (container[0] as Card)!.myStackUnit;
    }
    public static StackUnitCube ConvertToStackUnitCube(this List<ISelectableTarget> container) {
        return (container[0] as StackUnitCube);
    }

    public static List<ISelectableTarget> PackDice(this StackUnitCube cube)
    {
        return new List<ISelectableTarget> { cube };
    }
}