using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New LootData", menuName = "Cards/Primal", order = 51)]
public class PrimalCardData : CardData
{
    [field: SerializeField] public EffectAction action { get; private set; }
}
