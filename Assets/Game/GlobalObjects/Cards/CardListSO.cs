using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New List", menuName = "Cards/CardList", order = 51)]
public class CardListSO : ScriptableObject
{
    public List<CardData> list;
}
