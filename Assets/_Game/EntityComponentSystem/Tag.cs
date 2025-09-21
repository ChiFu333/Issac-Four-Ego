using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;


public interface ITag { public void Init(Entity entity); }

public interface IOnMouseDown { UniTask OnMouseDown(); }
public interface IOnMouseEnter { UniTask OnMouseEnter(); }
public interface IOnMouseExit { UniTask OnMouseExit(); }

public interface IRemovable { void Remove(); }

public interface IHaveUI
{
    public void ShowUI();
    public void HideUI();
}
public interface ITapEffect { UniTask OnTap(); }

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

[Serializable] public class Tappable : ITag, IOnMouseDown
{
    private Entity entity;
    public bool tapped = false;
    public void Init(Entity entity) {
        this.entity = entity;
    }
    public void Tap()
    {
        tapped = true;
        entity.transform.DORotate(new Vector3(0,0,-90), 0.3f);
    }
    public void Recharge()
    {
        tapped = false;
        entity.transform.DORotate(new Vector3(0,0,0), 0.3f);
    }
    public async UniTask OnMouseDown()
    {
        if (entity.HasTag<InShop>()) return;
        if (!tapped)
        {
            Tap();
            foreach (var tag in entity.tags.ToList())
            {
                if(tag is ITapEffect tapEffect) await tapEffect.OnTap();
            }
        }
        return;
    }
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

[Serializable] public class TapEffect : ITag, ITapEffect
{
    private Entity entity;
    public Effect effect;

    public void Init(Entity ent)
    {
        entity = ent;
    }
    public async UniTask OnTap()
    {
        G.Players.SetPrior(entity.GetMyPlayer());
        
        CardStackEffect csf = new CardStackEffect(effect, entity);
        await StackSystem.inst.PushEffect(csf);
        G.Players.RestorePrior();
        Console.WriteText("Использован предмет");
    }
}
[Serializable] public class IsItem : ITag
{
    public void Init(Entity entity)
    {
        
    }
}

[Serializable]
public class InShop : ITag
{
    public void Init(Entity entity)
    {
        
    }
}