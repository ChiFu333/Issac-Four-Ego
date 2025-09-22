using System;
using System.Collections.Generic;
using UnityEngine;

public class CubeVisual : MonoBehaviour, ISelectableTarget
{
    public int value;
    [SerializeField] private List<Sprite> numberSprite = new List<Sprite>();
    [SerializeField] private Color red, gray;

    public SpriteRenderer Visual;

    public void Awake()
    {
        Visual = GetComponent<SpriteRenderer>();
    }

    public void SetValue(int v, bool isReadyValue)
    {
        value = v;
        Visual.color = isReadyValue ? gray : red;
        Visual.sprite = numberSprite[v - 1];
    }
}
