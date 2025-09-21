using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public enum CardType {none, characterCard, lootCard, monsterCard, treasureCard, eventCard, soulCard};

[Serializable] public class CardSpritesData : ITag 
{
    private Entity entity;
    [SerializeField] public bool isFlipped = false;
    [SerializeField] public Sprite front;
    [SerializeField] public Sprite back;
    public void Init(Entity entity)
    {
        this.entity = entity;
    }
    public async UniTaskVoid Flip(bool Flip)
    {
        float timeToSwap = 0.3f;

        await entity.transform.DORotate(new Vector3(0, 90, 0), timeToSwap).AsyncWaitForCompletion();
        entity.GetComponent<SpriteRenderer>().sprite = Flip ? entity.GetTag<CardSpritesData>().front : entity.GetTag<CardSpritesData>().back;
        await entity.transform.DORotate(new Vector3(0, 0, 0), timeToSwap).AsyncWaitForCompletion();
    }
}

[Serializable] public class CardTypeTag : ITag
{
    public CardType cardType = CardType.none;
    public void Init(Entity entity) {}
}

[Serializable] public class CharacterItemPrefab : ITag
{
    public GameObject itemPrefab;
    public void Init(Entity entity) {}
}