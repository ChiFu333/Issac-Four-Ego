using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor.Rendering.LookDev;
using Random = UnityEngine.Random;

public enum CardType {none, characterCard, lootCard, monsterCard, treasureCard, eventCard, soulCard};

public interface ITag 
{
    public void Init(Entity entity);
}
public interface IHaveUI
{
    public void ShowUI();
    public void HideUI();
}

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
        bool trigger = false;
        entity.transform.DORotate(new Vector3(0, 90, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await UniTask.Yield();
        
        entity.GetComponent<SpriteRenderer>().sprite = Flip ? entity.GetTag<CardSpritesData>().front : entity.GetTag<CardSpritesData>().back;
        
        trigger = false;
        entity.transform.DORotate(new Vector3(0, 90, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await UniTask.Yield();

    }
}

[Serializable] public class CardTypeTag : ITag
{
    public CardType cardType = CardType.none;
    public void Init(Entity entity) {}
}

[Serializable] public class Characteristics : ITag, IHaveUI
{
    private Entity entity;
    private GameObject textWithDodge, textWithoutDodge;
    
    [SerializeField] private int healthMax;
    public int healthPrevent  = 0;
    public int dodge = 0;
    public int attack = 0;
    
    public int HealthMax 
    { 
        get => healthMax;
        set
        {
            int delta = value - healthMax;
            healthMax += delta;
            if(delta > 0)
            {
                health += delta;
            }
            else if(delta < 0)
            {
                health = healthMax < health ? healthMax : health;
            }
        }
    }
    public int health { get; private set; }  = 0;
    
    public bool isDead { get; set; } = false;
    
    public void Init(Entity entity) {
        this.entity = entity;
        health = healthMax;
        entity.visualTags[typeof(Characteristics)].SetActive(true);
        textWithDodge = entity.visualTags[typeof(Characteristics)].transform.GetChild(1).gameObject;
        textWithoutDodge = entity.visualTags[typeof(Characteristics)].transform.GetChild(0).gameObject;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        textWithDodge.SetActive(dodge != 0);
        textWithoutDodge.SetActive(dodge == 0);
        if(dodge == 0)
        {
            textWithoutDodge.transform.GetChild(0).GetComponent<TMP_Text>().text = health.ToString();
            textWithoutDodge.transform.GetChild(1).GetComponent<TMP_Text>().text = attack.ToString();
        }
        else
        {
            textWithDodge.transform.GetChild(0).GetComponent<TMP_Text>().text = health.ToString();
            textWithDodge.transform.GetChild(1).GetComponent<TMP_Text>().text = dodge.ToString();
            textWithDodge.transform.GetChild(2).GetComponent<TMP_Text>().text = attack.ToString();
        }    
    }
    
    public async UniTask Damage(int count)
    {
        int damageCount = count;
        if(health == 0) return;
        if(damageCount > healthPrevent)
        {
            damageCount -= healthPrevent;
            healthPrevent = 0;
            health -= damageCount;
            await EntityEffects.HitEntity(entity);
            if (entity.GetTag<CardTypeTag>().cardType == CardType.characterCard)
            {
                await TriggersSystem.takeDamage[0]?.PlayTriggeredEffects()!;
                await TriggersSystem.takeDamage[1 + G.Players.GetPlayerId(entity.GetMyPlayer())]?.PlayTriggeredEffects()!;
            }
        }
        else
        {
            healthPrevent -= damageCount;
        }
        if(health <= 0) 
        {
            health = 0;
            if(!isDead) await StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, entity);
        }
        UpdateUI();
    }
    public async UniTask AddHp(int count) 
    {
        HealthMax += count;
        UpdateUI();
        
        if(count > 0) await EntityEffects.CreateParcicle(entity, R.EffectSprites.healthUp, R.Audio.statUp);
    }
    public async UniTask HealHp(int count, bool throughDeath = false)
    {
        if((health == 0 && !throughDeath) || health == HealthMax) return;
        if(health + count > healthMax)
            health = healthMax;
        else
            health += count;
        UpdateUI();
        await EntityEffects.CreateParcicle(entity, R.EffectSprites.heal, R.Audio.heal);
    }
    public async UniTask<bool> PayHp(int count)
    {
        if(health - count < 0)
            return false;
        else
            health -= count;
        UpdateUI();
        if(health == 0 && !isDead) await StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, entity);
        return true;
    }
    public void AddPreventHp(int count)
    {
        healthPrevent += count;
        UpdateUI();
    }
    public async UniTask AddAttack(int to)
    {
        attack += to;
        UpdateUI();
        if (to > 0)
        {
            await EntityEffects.CreateParcicle(entity, R.EffectSprites.attackUp, R.Audio.statUp);
        }
    }
    
    public void ShowUI()
    {
        textWithDodge.SetActive(dodge != 0);
        textWithoutDodge.SetActive(dodge == 0);
    }
    public void HideUI()
    {
        textWithDodge.SetActive(false);
        textWithoutDodge.SetActive(false);
    }
}
[Serializable] public class CharacterItemPrefab : ITag
{
    public GameObject itemPrefab;
    public void Init(Entity entity) {}
}

public interface IOnMouseDown {
    UniTask OnMouseDown();
}
public interface IOnMouseEnter {
    UniTask OnMouseEnter();
}
public interface IOnMouseExit {
    UniTask OnMouseExit();
}

public interface IRemovable {
    void Remove();
}

[Serializable] public class Tappable : ITag
{
    private Entity entity;
    public bool tapped = false;
    public void Init(Entity entity) {
        this.entity = entity;
    }
    public void Tap()
    {
        tapped = true;
    }
    public void Recharge()
    {
        tapped = false;
    }
    /*
    public void OnMouseDown()
    {
        entity.transform.DORotate(new Vector3(0,0,-90), 0.3f);
    }*/
}

[Serializable] public class PlayEffect : ITag
{
    public Effect effect;
    public void Init(Entity entity) {}
}
[Serializable] public class ItemPassiveEffect : ITag
{
    public Effect effect;
    public void Init(Entity entity) {}
}
[Serializable] public class PassiveTrinketEffect : ITag
{
    public bool turnedIntoTrinket = false;
    public Effect effect;
    public void Init(Entity entity) {}
}
[Serializable] public class PlayFromHand : ITag, IOnMouseDown, IOnMouseEnter, IOnMouseExit, IRemovable
{   
    private Entity entity;
    public void Init(Entity entity) {
        this.entity = entity;
    }
    
    public UniTask OnMouseDown()
    {
        entity.GetMyPlayer().PlayLootCard(entity);
        return UniTask.CompletedTask;
    }

    public UniTask OnMouseEnter()
    {
        AudioManager.inst.Play(R.Audio.cardMouseOnLootInHands[Random.Range(0, R.Audio.cardMouseOnLootInHands.Count)]);
        entity.visual.transform.DOLocalMoveY(0.4f + entity.GetMyPlayer().hand.GetDeltaYInHand(entity), 0.3f);
        entity.visual.render.sortingOrder = 1000;
        
        entity.visual.transform.DOScale(1.15f, .15f).SetEase(Ease.OutBack);

        DOTween.Kill(2, true);
        entity.visual.transform.DOPunchRotation(Vector3.forward * 5, .15f, 20, 1).SetId(2);
        
        return UniTask.CompletedTask;
    }

    public UniTask OnMouseExit()
    {
        entity.visual.transform.DOLocalMoveY(0 + entity.GetMyPlayer().hand.GetDeltaYInHand(entity), 0.3f);
        entity.visual.render.sortingOrder = entity.GetMyPlayer().hand.GetMySortingOrder(entity);
        entity.visual.transform.DOScale(1, .15f).SetEase(Ease.OutBack);
        return UniTask.CompletedTask;
    }

    public void Remove()
    {
        entity.visual.transform.DOLocalMoveY(0, 0.1f);
        entity.visual.render.sortingOrder = 1000;
        entity.visual.transform.DOScale(1, .15f).SetEase(Ease.OutBack);
    }
}

[Serializable] public class IsItem : ITag
{
    public void Init(Entity entity)
    {
        
    }
}
[Serializable] public class TapBalatro : ITag, IOnMouseDown
{
    private Entity entity;
    public bool isTapped;
    public void Init(Entity entity) {
        this.entity = entity;
        isTapped = false;
    }
    
    public UniTask OnMouseDown()
    {
        isTapped = !isTapped;
        entity.visual.transform.DOLocalMoveY(isTapped ? 1f : 0, 0.1f);
        
        return UniTask.CompletedTask;
    }
}