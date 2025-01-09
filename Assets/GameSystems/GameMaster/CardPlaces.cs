using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class CardPlaces : MonoBehaviour
{
    public static CardPlaces inst { get; private set; }
    [field: SerializeField] public Vector3 otherPos { get; private set; } = new Vector3(-10, 0, 0);
    [Header("Loot")]
    [field: SerializeField, HorizontalGroup("loot")] public Transform lootDeck { get; private set; }
    [field: SerializeField, HorizontalGroup("loot")] public Transform lootStash { get; private set; }
    [Header("Monster")]
    [field: SerializeField, HorizontalGroup("monster")] public Transform monsterDeck { get; private set; }
    [field: SerializeField, HorizontalGroup("monster")] public Transform monsterStash { get; private set; }
    [field: SerializeField] public List<Transform> monsterSlots { get; private set; } = new List<Transform>(4);
    [Header("Shop")]
    [field: SerializeField, HorizontalGroup("shop")] public Transform shopDeck { get; private set; }
    [field: SerializeField, HorizontalGroup("shop")] public Transform shopStash { get; private set; }
    public List<Transform> shopSlots = new List<Transform>(4);
    [Header("Players")]
    [field: SerializeField] public List<Transform> hands { get; private set; }
    [field: SerializeField] private List<Transform> firstPlayer;
    [field: SerializeField] private List<Transform> secondPlayer;
    [field: SerializeField] private List<Transform> thirdPlayer;
    [field: SerializeField] private List<Transform> fourthPlayer;
    [field: SerializeField] public List<List<Transform>> playersPos { get; private set; }
    [field: SerializeField] private List<Transform> firstPlayerCurses;
    [field: SerializeField] private List<Transform> secondPlayerCurses;
    [field: SerializeField] private List<Transform> thirdPlayerCurses;
    [field: SerializeField] private List<Transform> fourthPlayerCurses;
    [field: SerializeField] public List<List<Transform>> playersCurses { get; private set; }
    public void Awake()
    {
        inst = this;
        playersPos = new List<List<Transform>>()
        {
            firstPlayer,
            secondPlayer,
            thirdPlayer,
            inst.fourthPlayer
        };
        playersCurses = new List<List<Transform>>()
        {
            firstPlayerCurses,
            secondPlayerCurses,
            thirdPlayerCurses,
            fourthPlayerCurses
        };
    }
}
