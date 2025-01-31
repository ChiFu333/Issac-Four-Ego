using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New LootData", menuName = "Cards/Cube", order = 51)]
public class CubeCardData : ScriptableObject
{
    [field: SerializeField] public Sprite face { get; private set; }
    [field: SerializeField] public Sprite end { get; private set; }
    [field: SerializeField] public int value { get; private set; }
}
