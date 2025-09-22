using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class StackUnitIconVisual : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private SpriteRenderer _backRenderer;
    private void Awake()
    {
        _renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _backRenderer = GetComponent<SpriteRenderer>();
    }

    public async UniTask SetupIcon(StackUnitIconData data, int order)
    {
        transform.localPosition = data.offset;
        _renderer.color = data.color;
        _backRenderer.color = data.subColor;
        _renderer.sprite = data.icon;
        ChangeOrder(order);
        transform.GetChild(0).localScale = Vector3.one * data.localScale;
        await transform.DOScale(Vector3.one * 2.2f, 0.2f).From(Vector3.zero).AsyncWaitForCompletion();
    }

    public void ChangeOrder(int order)
    {
        _renderer.sortingOrder = order + 1;
        _backRenderer.sortingOrder = order;
    }

    public void RemoveIcon()
    {
        _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, 0);
        _backRenderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, 0);
    }
}

[Serializable]
public class StackUnitIconData
{
    public Vector2 offset;
    public float localScale;
    public Sprite icon;
    public Color color;
    public Color subColor;
}

[Serializable]
public class TagStackUnitIcons : EntityComponentDefinition
{
    public StackUnitIconData lootplayEffect;
    public StackUnitIconData activateEffect;
    public StackUnitIconData payEffect;
    public StackUnitIconData triggeredEffect;
    public List<StackUnitIconData> cubeEffects;
}