using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;
using System.Linq;
public static class EntityEffects
{
    public static void HitEntity(Entity entity)
    {
        List<Material> mats = entity.visual.GetAllMaterialsInChildren();

        float time = 0.2f;
        entity.transform.DOPunchScale(Vector3.one * -0.10f, time, elasticity: 0f, vibrato: 0);
        foreach(var material in mats)
        {
            material.SetFloat("_HitEffectBlend", 0.5f);
            DOTween.To(
                () => material.GetFloat("_HitEffectBlend"), 
                x => material.SetFloat("_HitEffectBlend", x),
                0,
                time * 1.75f 
            );
        }        

        if(entity.GetTag<CardTypeTag>().cardType == CardType.characterCard)
            AudioManager.inst.Play(R.Audio.playerTakeDamage);
        else
            AudioManager.inst.Play(R.Audio.enemyTakeDamage);
    }
    public async static void TurnDead(Entity entity)
    {
        List<Material> mats = entity.visual.GetAllMaterialsInChildren();

        if(entity.GetTag<CardTypeTag>().cardType == CardType.characterCard)
            AudioManager.inst.Play(R.Audio.playerDie);
        //else
            //AudioManager.inst.Play(R.Audio.enemyTakeDamage);

        float timeToSwap = 0.5f;
        bool trigger = false;
        entity.transform.DORotate(new Vector3(0, 90, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await Task.Yield();

        foreach(var material in mats)
        {
            material.SetFloat("_GreyscaleBlend", 1);
            material.SetFloat("_OverlayBlend", 1);
        }

        entity.transform.DORotate(new Vector3(0, 0, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await Task.Yield();
    }
    public async static void TurnAlive(Entity entity)
    {
        List<Material> mats = entity.visual.GetAllMaterialsInChildren();

        float timeToSwap = 0.5f;
        bool trigger = false;
        entity.transform.DORotate(new Vector3(0, 90, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await Task.Yield();

        foreach(var material in mats)
        {
            material.SetFloat("_GreyscaleBlend", 0);
            material.SetFloat("_OverlayBlend", 0);
        }

        entity.transform.DORotate(new Vector3(0, 0, 0), timeToSwap).onComplete = () => trigger = true;
        while(!trigger) await Task.Yield();
    }
    public async static void MakeShine(Entity entity)
    {
        EntityVisual vis = entity.visual;
        vis.isShine = true;
        Material mat = vis.render.material;

        float time = 4f;

        mat.SetFloat("_ShineLocation", 0f);
        for(int i = 0; i < 10; i++)
        {
            DOTween.To(
            () => mat.GetFloat("_ShineLocation"), 
            x => mat.SetFloat("_ShineLocation", x),
            0.75f,
            time 
            );
            
            await Task.Delay((int)(time * 1000) + 100);
            DOTween.To(
            () => mat.GetFloat("_ShineLocation"), 
            x => mat.SetFloat("_ShineLocation", x),
            0.25f,
            time 
            );
            await Task.Delay(3000);
        }
        
    }
}
