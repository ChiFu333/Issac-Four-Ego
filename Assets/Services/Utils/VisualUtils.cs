using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class VisualUtils
{
    public static Sprite GetBackSprite(this CardType type)
    {
        return CMS.GetAll<CMSEntity>().FirstOrDefault(ent => ent.Is<CardBackConfig>())?.Get<CardBackConfig>().GetBackSprite(type);
    }

    public static Vector3 GetDeckTopPos(this CardType type)
    {
        switch (type)
        {
            case CardType.characterCard:
                return G.Main.Decks.characterDeck.GetMyPos(true);
            case CardType.lootCard:
                return G.Main.Decks.lootDeck.GetMyPos(true);
            case CardType.treasureCard:
                return G.Main.Decks.treasureDeck.GetMyPos(true);
            case CardType.monsterCard:
                return G.Main.Decks.monsterDeck.GetMyPos(true);
            case CardType.eventCard:
                return G.Main.Decks.monsterDeck.GetMyPos(true);
        }

        return Vector3.zero;
    }
}
[Serializable]
public class CardBackConfig : EntityComponentDefinition
{
    [Serializable]
    public class BackSpriteByType
    {
        public CardType type;
        public Sprite backSprite;
    }

    public List<BackSpriteByType> data;

    public Sprite GetBackSprite(CardType type)
    {
        return data.FirstOrDefault(x => x.type == type).backSprite;
    }
}

