using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
[CreateAssetMenu(fileName = "New DeckList", menuName = "Cards/DeckList", order = 51)]
public class DeckList : ScriptableObject
{
    [field: SerializeField] public List<CardInstance> cardsList { get; private set; }
}
[Serializable] public class CardInstance
{
    [field: SerializeField, HorizontalGroup("CardInst")] public CardData data { get; private set; }
    [field: SerializeField, HorizontalGroup("CardInst")] public bool willAdd { get; private set; } = true;
    [field: SerializeField, HorizontalGroup("CardInst")] public int count { get; private set; } = 1;
}