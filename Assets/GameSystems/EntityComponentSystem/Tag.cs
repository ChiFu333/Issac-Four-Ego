using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.PlayerLoop;
public interface ITag 
{
    public void Init(Entity entity);
}
public interface IHaveUI
{
    public void ShowUI();
    public void HideUI();
}
[Serializable] public class CardSpritesData : ITag  //isTapped
{
    private Entity entity;
    [SerializeField] public bool isFlipped = false;
    [SerializeField] public Sprite front;
    [SerializeField] public Sprite back;
    public void Init(Entity entity)
    {
        this.entity = entity;
    }
    public void Flip(bool Flip)
    {
        entity.GetComponent<SpriteRenderer>().sprite = Flip ? entity.GetTag<CardSpritesData>().front : entity.GetTag<CardSpritesData>().back;
    }
}
[Serializable] public class CardTypeTag : ITag
{
    public CardType cardType = CardType.none;
    public void Init(Entity entity) {}
}
public enum CardType {none, characterCard, lootCard, monsterCard, treasureCard, eventCard, soulCard};
[Serializable] public class Characteristics : ITag, IHaveUI
{
    private Entity entity;
    private GameObject textWithDodge, textWithoutDodge;
    public int HealthMax 
    { 
        get => healthMax;
        set
        {
            int delta = value - healthMax;
            healthMax += delta;
            if(delta > 0)
            {
                HealHp(delta);
            }
            else if(delta < 0)
            {
                health = healthMax < health ? healthMax : health;
            }
        }
    }
    [SerializeField] private int healthMax;
    public int health { get; private set; }  = 0;
    [HideInInspector] public int healthPrevent  = 0;
    public int dodge = 0;
    public int attack = 0;
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
    public async Task Damage(int count)
    {
        int damageCount = count;
        if(health == 0) return;
        
        EntityEffects.HitEntity(entity);
        if(damageCount > healthPrevent)
        {
            damageCount -= healthPrevent;
            healthPrevent = 0;
            health -= damageCount;
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
        return;
    }
    public void AddHp(int count) 
    {
        HealthMax += count;
        UpdateUI();
    }
    public void HealHp(int count, bool throughDeath = false)
    {
        if(health == 0 && !throughDeath) return;
        if(health + count > healthMax)
            health = healthMax;
        else
            health += count;
        UpdateUI();
    }
    public async Task<bool> PayHp(int count)
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
    public void ChangeAttack(int to)
    {
        attack = to;
        UpdateUI();
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
    Task OnMouseDown();
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
[Serializable]
public class PlayFromHand : ITag, IOnMouseDown
{
    private Entity entity;
    public void Init(Entity entity) {
        this.entity = entity;
    }

    public Task OnMouseDown()
    {
        entity.GetMyPlayer().PlayLootCard(entity);
        return Task.CompletedTask;
    }
}

public interface IFlag { }
[Serializable] public class IsItem : ITag, IFlag
{
    public void Init(Entity entity)
    {
        EntityEffects.MakeShine(entity);
    }
}