using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class CardPlaces : MonoBehaviour
{
    [field: SerializeField] private Vector3 otherPos = new Vector3(-10, 0, 0);
    [Header("Loot")]
    [field: SerializeField] private Transform lootDeck;
    [field: SerializeField] private Transform lootStash;
    [Header("Monster")]
    [field: SerializeField] private Transform monsterDeck;
    [field: SerializeField] private Transform monsterStash;
    [field: SerializeField] private List<Transform> monsterSlots = new List<Transform>(4);
    [Header("Shop")]
    [field: SerializeField] private Transform shopDeck;
    [field: SerializeField] private Transform shopStash;
    [field: SerializeField] private List<Transform> shopSlots = new List<Transform>(4);
    [Header("Players")]
    [field: SerializeField] private List<Transform> playersTransformToDeconstruct;
    public void Init()
    {
        G.CardPlaces.lootDeck = lootDeck;
        G.CardPlaces.lootStash = lootStash;
        G.CardPlaces.monsterDeck = monsterDeck;
        G.CardPlaces.monsterStash = monsterStash;
        G.CardPlaces.monsterSlots = monsterSlots;
        G.CardPlaces.shopDeck = shopDeck;
        G.CardPlaces.shopStash = shopStash;
        G.CardPlaces.shopSlots = shopSlots;
        G.CardPlaces.hands = new List<Transform>()
        {
            playersTransformToDeconstruct[0].GetChild(4),
            playersTransformToDeconstruct[1].GetChild(4),
            playersTransformToDeconstruct[2].GetChild(4),
            playersTransformToDeconstruct[3].GetChild(4),
        };
        G.CardPlaces.playersPos = new List<List<Transform>>()
        {
            GetChildList(playersTransformToDeconstruct[0].GetChild(0)),
            GetChildList(playersTransformToDeconstruct[1].GetChild(0)),
            GetChildList(playersTransformToDeconstruct[2].GetChild(0)),
            GetChildList(playersTransformToDeconstruct[3].GetChild(0)),
        };
        G.CardPlaces.playersCurses = new List<List<Transform>>()
        {
            GetChildList(playersTransformToDeconstruct[0].GetChild(1)),
            GetChildList(playersTransformToDeconstruct[1].GetChild(1)),
            GetChildList(playersTransformToDeconstruct[2].GetChild(1)),
            GetChildList(playersTransformToDeconstruct[3].GetChild(1)),
        };
        G.CardPlaces.playersTransformToDeconstruct = playersTransformToDeconstruct;
    }
    private List<Transform> GetChildList(Transform parent)
    {
        List<Transform> childList = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            childList.Add(parent.GetChild(i));
        }
        return childList;
    }
}

public static partial class G
{
    public static class CardPlaces
    {
        public static Vector3 otherPos = new Vector3(-10, 0, 0);
        public static Transform lootDeck;
        public static Transform lootStash;
        public static Transform monsterDeck;
        public static Transform monsterStash;
        public static List<Transform> monsterSlots;
        public static Transform shopDeck;
        public static Transform shopStash;
        public static List<Transform> shopSlots;
        public static List<Transform> hands;
        public static List<List<Transform>> playersPos;
        public static List<List<Transform>> playersCurses;
        public static List<Transform> playersTransformToDeconstruct;
    }
}