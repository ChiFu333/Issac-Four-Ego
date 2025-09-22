using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Animation", menuName = "Animations/AnimationDataSO")]
public class AnimationDataSO : ScriptableObject {
    [field: SerializeField] public float framerate { get; private set; }
    [field: SerializeField] public List<Sprite> frames { get; private set; } = new List<Sprite>();
    [field: SerializeField] public Vector2 animationOffset { get; private set; }
}
