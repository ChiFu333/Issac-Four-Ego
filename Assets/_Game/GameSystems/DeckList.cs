using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
[CreateAssetMenu(fileName = "New DeckList", menuName = "Cards/DeckList", order = 51)]
public class DeckList : ScriptableObject
{
    [field: SerializeField] public List<CardInstance> CardsList { get; private set; }
    public List<GameObject> GetList()
    {
        List<GameObject> list = new List<GameObject>();
        for(int i = 0; i < CardsList.Count; i++)
        {
            if(CardsList[i].willAdd)
            {
                for(int j = 0; j < CardsList[i].count; j++)
                {
                    list.Add(CardsList[i].data);
                }
            }
        }
        return list;
    }
}
[Serializable] public class CardInstance
{
    [field: SerializeField, HorizontalGroup("CardInst")] public GameObject data { get; private set; }
    [field: SerializeField, HorizontalGroup("CardInst")] public bool willAdd { get; private set; } = true;
    [field: SerializeField, HorizontalGroup("CardInst")] public int count { get; private set; } = 1;
}