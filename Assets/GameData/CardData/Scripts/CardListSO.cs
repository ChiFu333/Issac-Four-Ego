using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New List", menuName = "Cards/CardList", order = 51)]
public class CardListSO : ScriptableObject
{
    [field: SerializeField] public List<CardData> list { get; private set; }
}
