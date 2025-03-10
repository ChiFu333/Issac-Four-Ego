using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New LootData", menuName = "Cards/Cube", order = 51)]
public class CubeCardData : CardData
{
    [field: SerializeField] public int value { get; private set; }
}
