using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class CardPlaces : MonoBehaviour
{
    [Header("Loot")]
    public Transform lootDeck;
    public Transform lootStash;
    [Header("Monster")]
    public Transform monsterDeck;
    public Transform monsterStash;
    public List<Transform> monsterSlots = new List<Transform>(4);
    [Header("Shop")]
    public Transform treasureDeck;
    public Transform treasureStash;
    public List<Transform> shopSlots = new List<Transform>(4);
    [Header("Players")]
    public List<Transform> playersTransformToDeconstruct;

    [HideInInspector] public List<Transform> hands;
    [HideInInspector] public List<List<Transform>> playersPos;
    [HideInInspector] public List<List<Transform>> playersCurses;
    [HideInInspector] public List<List<Transform>> playersSouls;
    [HideInInspector] public Vector3 otherPos = new Vector3(-10, 0, 0);
    [HideInInspector] public List<TMP_Text> statsTexts;
    
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
        playersSouls = new List<List<Transform>>()
        {
            GetChildList(playersTransformToDeconstruct[0].GetChild(2)),
            GetChildList(playersTransformToDeconstruct[1].GetChild(2)),
            GetChildList(playersTransformToDeconstruct[2].GetChild(2)),
            GetChildList(playersTransformToDeconstruct[3].GetChild(2)),
        };
        statsTexts = new List<TMP_Text>(){
            playersTransformToDeconstruct[0].GetComponentInChildren<TMP_Text>(),
            playersTransformToDeconstruct[1].GetComponentInChildren<TMP_Text>(),
            playersTransformToDeconstruct[2].GetComponentInChildren<TMP_Text>(),
            playersTransformToDeconstruct[3].GetComponentInChildren<TMP_Text>(),
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