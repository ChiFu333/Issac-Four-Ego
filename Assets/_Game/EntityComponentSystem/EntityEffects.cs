using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Linq;

public static class EntityEffects
{

    public static async UniTask HitEntity(Entity entity)
    {
        List<Material> mats = entity.visual.GetAllMaterialsInChildren();

        float time = 0.15f;
        entity.visual.transform.DOPunchScale(Vector3.one * -0.15f, time, elasticity: 0f, vibrato: 0);
        
        AudioManager.inst.Play(entity.GetTag<CardTypeTag>().cardType == CardType.characterCard
            ? R.Audio.playerTakeDamage
            : R.Audio.enemyTakeDamage);
        
        List<bool> triggers = new List<bool>();
        for (var index = 0; index < mats.Count; index++)
        {
            int t = index;
            triggers.Add(false);
            var material = mats[index];
            material.SetFloat("_HitEffectBlend", 0.5f);
            DOTween.To(
                () => material.GetFloat("_HitEffectBlend"),
                x => material.SetFloat("_HitEffectBlend", x),
                0,
                time * 1.75f
            ).onComplete = () => triggers[t] = true;
        }
        while(!CheckTriggers(triggers)) await UniTask.Yield();
    }
    
    public static async UniTask TurnDead(Entity entity)
    {
        List<Material> mats = entity.visual.GetAllMaterialsInChildren();

        if(entity.GetTag<CardTypeTag>().cardType == CardType.characterCard)
            AudioManager.inst.Play(R.Audio.playerDie);
        //else
            //AudioManager.inst.Play(R.Audio.enemyTakeDamage);

        float timeToSwap = 0.5f;
        bool trigger = false;
        entity.transform.DORotate(new Vector3(0, 90, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await UniTask.Yield();

        foreach(var material in mats)
        {
            material.SetFloat("_GreyscaleBlend", 1);
            material.SetFloat("_OverlayBlend", 1);
        }

        entity.transform.DORotate(new Vector3(0, 0, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await UniTask.Yield();
    }
    
    public static async UniTask TurnAlive(Entity entity)
    {
        List<Material> mats = entity.visual.GetAllMaterialsInChildren();

        float timeToSwap = 0.5f;
        bool trigger = false;
        entity.transform.DORotate(new Vector3(0, 90, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await UniTask.Yield();

        foreach(var material in mats)
        {
            material.SetFloat("_GreyscaleBlend", 0);
            material.SetFloat("_OverlayBlend", 0);
        }

        entity.transform.DORotate(new Vector3(0, 0, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await UniTask.Yield();
    }
    
    public static async UniTask CreateParcicle(Entity entity, Sprite spriteBuff, AudioClip sound = null)
    {
        float particleScale = 1.1f / Entity.CARDSIZE;
        float deltaY = 0.5f;
        GameObject g = new GameObject("Particle")
        {
            transform =
            {
                position = entity.transform.position + Vector3.down * deltaY / 1.5f,
                localScale = Vector3.zero
            }
        };
        SpriteRenderer render = g.AddComponent<SpriteRenderer>();
        render.sortingOrder = 10000;
        render.sprite = spriteBuff;
        
        if(sound != null) AudioManager.inst.Play(sound);
        
        bool trigger = false;
        g.transform.DOMove(entity.transform.position, 0.25f)
            .SetEase(Ease.InQuad);
        g.transform.DOScale(Vector3.one * particleScale * entity.transform.lossyScale.x, 0.25f)
            .SetEase(Ease.InQuad)
            .onComplete = () => trigger = true;
            

        while (!trigger) await UniTask.Yield();
        trigger = false;

        g.transform.DOMove(g.transform.position + Vector3.up * deltaY, 0.5f);
        render.DOFade(0, 0.5f)
            .onComplete = () => trigger = true;

        while (!trigger) await UniTask.Yield();
        Object.Destroy(g);
    }
    private static bool CheckTriggers(List<bool> ts)
    {
        foreach (var b in ts)
            if(!b) return false;
        return true;
    }    
}
