using UnityEngine;
using Sirenix.OdinInspector;
public class CardData : ScriptableObject
{   
    [field: SerializeField] public string title { get; private set; }
    [field: SerializeField, HorizontalGroup("Face")] public Sprite face { get; private set; }
    [field: SerializeField, HorizontalGroup("Face")] public Sprite back { get; private set; }
}