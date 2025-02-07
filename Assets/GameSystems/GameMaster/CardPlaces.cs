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
    public List<Transform> hands { get; private set; }
    [field: SerializeField] public List<List<Transform>> playersPos { get; private set; }
    [field: SerializeField] public List<List<Transform>> playersCurses { get; private set; }
    [field: SerializeField] public List<Transform> playersTransformToDeconstruct { get; private set; }
    public void Awake()
    {
        inst = this;
    }
    public void Init()
    {
        
        hands = new List<Transform>()
        {
            playersTransformToDeconstruct[0].GetChild(4),
            playersTransformToDeconstruct[1].GetChild(4),
            playersTransformToDeconstruct[2].GetChild(4),
            playersTransformToDeconstruct[3].GetChild(4),
        };
        playersPos = new List<List<Transform>>()
        {
            GetChildList(playersTransformToDeconstruct[0].GetChild(0)),
            GetChildList(playersTransformToDeconstruct[1].GetChild(0)),
            GetChildList(playersTransformToDeconstruct[2].GetChild(0)),
            GetChildList(playersTransformToDeconstruct[3].GetChild(0)),
        };
        playersCurses = new List<List<Transform>>()
        {
            GetChildList(playersTransformToDeconstruct[0].GetChild(1)),
            GetChildList(playersTransformToDeconstruct[1].GetChild(1)),
            GetChildList(playersTransformToDeconstruct[2].GetChild(1)),
            GetChildList(playersTransformToDeconstruct[3].GetChild(1)),
        };

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
