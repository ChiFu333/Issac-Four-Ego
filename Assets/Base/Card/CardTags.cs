using UnityEngine;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using System.Linq;
public interface IInitable { void Init(Card c); }
public interface IOnMouseDown { UniTask OnMouseDown(); }
public interface IOnMouseEnter { UniTask OnMouseEnter(); }
public interface IOnMouseExit { UniTask OnMouseExit(); }
public interface IRemovable { void Remove(); }
public interface IHaveUI
{
    public void ShowUI();
    public void HideUI();
}
public interface ITapEffect { UniTask<bool> OnTap(); }

[Serializable] public class TagCharacteristics : EntityComponentDefinition, IInitable, IHaveUI
{
    private Card card;
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
    
    public void Init(Card c) {
        card = c;
        health = healthMax;
        
        textWithDodge = card.GetVisualObject<TagCharacteristics>().transform.GetChild(1).gameObject;
        textWithoutDodge = card.GetVisualObject<TagCharacteristics>().transform.GetChild(0).gameObject;
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
        if(card.isFaceUp) 
            ShowUI();
        else
            HideUI();
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
            await card.HitEntity();
            
            List<IOnTakeDamage> onTakeDamages = 
                G.Main.AllCards
                    .Where(card => card.visual.enabled && card.Is<TagIsItem>())  
                    .SelectMany(card => card.state.OfType<IOnTakeDamage>()).ToList();
            foreach (var variable in onTakeDamages)
            {
                await variable.OnTakeDamage(card);
            }
        }
        else
        {
            healthPrevent -= damageCount;
        }
        if(health <= 0) 
        {
            health = 0;
            //if(!isDead) await StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, entity);
        }
        UpdateUI();
    }
    public async UniTask AddHp(int count) 
    {
        HealthMax += count;
        UpdateUI();
        
        if(count > 0) await card.CreateParcicle("CMS/Buffs/HpUp");
        if(count < 0) await card.CreateParcicle("CMS/Buffs/HpDown");
    }
    public async UniTask HealHp(int count, bool throughDeath = false)
    {
        if((health == 0 && !throughDeath) || health == HealthMax) return;
        if(health + count > healthMax)
            health = healthMax;
        else
            health += count;
        UpdateUI();
        //await EntityEffects.CreateParcicle(entity, R.EffectSprites.heal, R.Audio.heal);
    }
    public async UniTask<bool> PayHp(int count)
    {
        if(health - count < 0)
            return false;
        else
            health -= count;
        UpdateUI();
        //if(health == 0 && !isDead) await StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, entity);
        return true;
    }
    public void AddPreventHp(int count)
    {
        healthPrevent += count;
        UpdateUI();
    }
    public async UniTask AddAttack(int count)
    {
        attack += count;
        UpdateUI();
        if(count > 0) await card.CreateParcicle("CMS/Buffs/AttackUp");
        if(count < 0) await card.CreateParcicle("CMS/Buffs/AttackDown");
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

[Serializable] public class TagInHand : EntityComponentDefinition, IInitable, IOnMouseDown, IOnMouseEnter, IOnMouseExit, IRemovable
{   
    private Card card;
    public void Init(Card c) {
        card = c;
    }
    public UniTask OnMouseDown()
    {
        //entity.GetMyPlayer().PlayLootCard(entity);
        return UniTask.CompletedTask;
    }

    public UniTask OnMouseEnter()
    {
        G.AudioManager.PlaySound(R.Audio.MouseHoverLoot, card.GetMyPlayer().Get<TagBasePlayerData>().hand.handZone.cards.FindIndex(x => x == card) * 0.03f);
        card.visual.transform.parent.DOLocalMoveY(1f, 0.3f);
        card.visual.sortingOrder = 500;
        
        card.visual.transform.parent.DOScale(1.5f, .15f).SetEase(Ease.OutBack);

        DOTween.Kill(2, true);
        card.visual.transform.parent.DOPunchRotation(Vector3.forward * 5, .15f, 20, 1).SetId(2);
        
        return UniTask.CompletedTask;
    }

    public UniTask OnMouseExit()
    {
        card.visual.transform.parent.DOLocalMoveY(0, 0.3f);
        card.visual.sortingOrder = card.GetMyPlayer().Get<TagBasePlayerData>().hand.GetMySortingOrder(card);
        card.visual.transform.parent.DOScale(1, .15f).SetEase(Ease.OutBack);
        return UniTask.CompletedTask;
    }

    public void Remove()
    {
        card.visual.transform.parent.DOLocalMoveY(0, 0.1f);
        card.visual.transform.DOLocalMoveY(0, 0.1f);
        card.visual.sortingOrder = 1000;
        card.visual.transform.parent.DOScale(1, .15f).SetEase(Ease.OutBack);
    }
}
[Serializable] public class TagPlayEffect : EntityComponentDefinition, IInitable
{
    public Effect effect;
    public void Init(Card c)
    {
        effect = effect.DeepCopy();
    }
}
[Serializable] public class TapBalatro : EntityComponentDefinition, IInitable, IOnMouseDown, IRemovable
{
    private Card card;
    public bool isTapped;
    public void Init(Card card) {
        this.card = card;
        isTapped = false;
    }
    
    public UniTask OnMouseDown()
    {
        isTapped = !isTapped;
        card.visual.transform.DOLocalMoveY(isTapped ? 1f : 0, 0.1f);
        
        return UniTask.CompletedTask;
    }
    public void Remove()
    {
        card.visual.transform.DOLocalMoveY(0, 0.05f);
    }
}

