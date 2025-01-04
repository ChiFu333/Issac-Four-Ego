using System.Collections.Generic;
using UnityEngine;

public class CardPlaces : MonoBehaviour
{
    public static CardPlaces Inst;
    public Vector3 OtherPos = new Vector3(-10, 0, 0);
    [Header("Loot")]
    public Transform LootDeck;
    public Transform LootStash;
    [Header("Monster")]
    public Transform MonsterDeck;
    public Transform MonsterStash;
    public List<Transform> MonsterSlots = new List<Transform>(3);
    [Header("Shop")]
    public Transform ShopDeck;
    public Transform ShopStash;
    public List<Transform> ShopSlots = new List<Transform>(3);
    [Header("Players")]
    public List<Transform> Hands;
    public List<Transform> MePlayer;
    public List<Transform> SecondPlayer;
    public List<Transform> ThirdPlayer;
    public List<Transform> FourthPlayer;
    public List<List<Transform>> PlayersPos;
    public List<Transform> PlayersCurses;
    public void Awake()
    {
        Inst = this;
        PlayersPos = new List<List<Transform>>()
        {
            CardPlaces.Inst.MePlayer,
            CardPlaces.Inst.SecondPlayer,
            CardPlaces.Inst.ThirdPlayer,
            CardPlaces.Inst.FourthPlayer
        };
    }
}
