using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using Runtime;
using Unity.Mathematics;

public static class CardsVisualEffects
{
    public static void AddOutline(this Card card, Color color)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        Material mat = card.visual.material;

        mat.SetColor("_OutlineColor", color);
        DOTween.To(
            () => mat.GetFloat("_OutlineAlpha"), 
            x => mat.SetFloat("_OutlineAlpha", x),
            config.outlineAlphaParam,                         
            config.outlineTimeToChange                                
        );
        DOTween.To(
            () => mat.GetFloat("_OutlineWidth"), 
            x => mat.SetFloat("_OutlineWidth", x),
            config.outlineWidth,                         
            config.outlineTimeToChange                                
        );
    }
    public static void RemoveOutline(this Card card)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        Material mat = card.visual.material;
        
        DOTween.To(
            () => mat.GetFloat("_OutlineAlpha"), 
            x => mat.SetFloat("_OutlineAlpha", x),
            0,                         
            config.outlineTimeToChange                                
        );
        DOTween.To(
            () => mat.GetFloat("_OutlineWidth"), 
            x => mat.SetFloat("_OutlineWidth", x),
            0,                         
            config.outlineTimeToChange                                
        );
    }
    public static void AddGreyScale(this Card card)
    {
        Material mat = card.visual.material;
        DOTween.To(
            () => mat.GetFloat("_GreyscaleBlend"), 
            x => mat.SetFloat("_GreyscaleBlend", x),
            0.55f,                                  
            0.25f                                
        );
    }
    public static void RemoveGreyScale(this Card card)
    {
        Material mat = card.visual.material;
        DOTween.To(
            () => mat.GetFloat("_GreyscaleBlend"), 
            x => mat.SetFloat("_GreyscaleBlend", x),
            0f,                                  
            0.25f                                
        );
    }
    public static void AddGlow(this Card card)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        List<Material> mats = GetAllMaterialsInChildren(card);
        
        foreach (var mat in mats)
        {
            DOTween.To(
                () => mat.GetFloat("_Glow"), 
                x => mat.SetFloat("_Glow", x),
                config.glowBaseValue,                         
                config.glowTimeToChange                                
            );
        }
    }
    public static void RemoveGlow(this Card card)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        List<Material> mats = GetAllMaterialsInChildren(card);
        foreach (var mat in mats)
        {
            DOTween.To(
                () => mat.GetFloat("_Glow"),
                x => mat.SetFloat("_Glow", x),
                0f,
                config.glowTimeToChange     
            );
        }
    }
    public static void RemoveLit(this Card card)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        List<Material> mats = GetAllMaterialsInChildren(card);
        
        foreach (var mat in mats)
        {
            DOTween.To(
                () => mat.GetFloat("_LitAmount"), 
                x => mat.SetFloat("_LitAmount", x),
                config.litBaseValue,                         
                config.litTimeToChange                                
            );
        }
    }
    public static void RestoreLit(this Card card)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        List<Material> mats = GetAllMaterialsInChildren(card);
        
        foreach (var mat in mats)
        {
            DOTween.To(
                () => mat.GetFloat("_LitAmount"), 
                x => mat.SetFloat("_LitAmount", x),
                1,                         
                config.litTimeToChange                                
            );
        }
    }
    
    public static async UniTask HitEntity(this Card card)
    {
        Material mat = card.visual.material;
        Camera.main.GetComponent<CameraShake>().Shake();
        VFXConfig config = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<VFXConfig>())!.Get<VFXConfig>();
        GameObject g = GameObject.Instantiate(config.hitEffect, card.transform.position, quaternion.identity);
        g.transform.localScale = card.transform.lossyScale * 1.8f;
        float time = 0.2f;
        card.visual.transform.DOPunchScale(Vector3.one * -0.45f, time, elasticity: 0f, vibrato: 0);
        
        G.AudioManager.PlaySound(card.Get<TagCardType>().cardType == CardType.characterCard ? R.Audio.hitSound : R.Audio.hitSound, 0);
        
        mat.SetFloat("_HitEffectBlend", 0.5f);
        await DOTween.To(
            () => mat.GetFloat("_HitEffectBlend"),
            x => mat.SetFloat("_HitEffectBlend", x),
            0,
            time * 1.75f
        ).AsyncWaitForCompletion().AsUniTask();
    }
    public static async UniTask BombCard(this Card card)
    {
        Material mat = card.visual.material;
        Camera.main.GetComponent<CameraShake>().Shake();
        VFXConfig config = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<VFXConfig>())!.Get<VFXConfig>();
        GameObject g = GameObject.Instantiate(config.boomVFX, card.transform.position, quaternion.identity);
        g.transform.localScale = card.transform.lossyScale * 2f;
        float time = 0.35f;
        card.visual.transform.DOPunchScale(Vector3.one * -0.45f, time, elasticity: 0f, vibrato: 0);
        
        G.AudioManager.PlaySound(R.Audio.DestroyItem, -0.2f);
        
        mat.SetFloat("_HitEffectBlend", 0.5f);
        await DOTween.To(
            () => mat.GetFloat("_HitEffectBlend"),
            x => mat.SetFloat("_HitEffectBlend", x),
            0,
            time * 1.5f
        ).AsyncWaitForCompletion().AsUniTask();
    }
    public static async UniTask SmokeCard(this Card card)
    {
        Material mat = card.visual.material;
        VFXConfig config = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<VFXConfig>())!.Get<VFXConfig>();
        GameObject g = GameObject.Instantiate(config.smokeVFX, card.transform.position, quaternion.identity);
        g.transform.localScale = card.transform.lossyScale * 1.75f;
        float time = 0.35f;
        card.visual.transform.DOPunchScale(Vector3.one * -0.45f, time, elasticity: 0f, vibrato: 0);
        
        G.AudioManager.PlaySound(R.Audio.SmokeSound, 0);

        await UniTask.Delay(850);
    }
    public static async UniTask TurnIntoItemEffect(this Card card)
    {
        Material mat = card.visual.material;
        VFXConfig config = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<VFXConfig>())!.Get<VFXConfig>();
        GameObject g = GameObject.Instantiate(config.upgradeVFX, card.transform.position, quaternion.identity);
        g.transform.localScale = card.transform.lossyScale * 3f;
        float time = 0.45f;
        card.visual.transform.DOPunchScale(Vector3.one * -0.2f, time, elasticity: 0f, vibrato: 0);
        G.AudioManager.PlaySound(R.Audio.TurnIntoItem, 0);
        await card.visual.transform.parent.DORotate(new Vector3(0,0,720), 0.5f,RotateMode.FastBeyond360).SetEase(Ease.InOutBack).AsyncWaitForCompletion().AsUniTask();
        await UniTask.Delay(400);
    }
    public static async UniTask TurnIntoSoulEffect(this Card card)
    {
        Material mat = card.visual.material;
        VFXConfig config = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<VFXConfig>())!.Get<VFXConfig>();
        GameObject g = GameObject.Instantiate(config.upgradeVFX, card.transform.position, quaternion.identity);
        g.transform.localScale = card.transform.lossyScale * 3f;
        float time = 0.45f;
        card.visual.transform.DOPunchScale(Vector3.one * -0.2f, time, elasticity: 0f, vibrato: 0);
        G.AudioManager.PlaySound(R.Audio.superholy, 0);
        await card.visual.transform.DORotate(new Vector3(0,0,360*3), 1f,RotateMode.FastBeyond360).SetEase(Ease.InOutBack).AsyncWaitForCompletion().AsUniTask();
        await UniTask.Delay(400);
    }
    public static async UniTask GetCoinVisual(this ISelectableTarget target, Vector3 pos, Action AddCoin)
    {
        float FlightDuration = 0.8f;
        float fadeOut = 0.2f;
        VFXConfig config = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<VFXConfig>())!.Get<VFXConfig>();
        GameObject coinVFX = config.coinVFX;
   
        var particleObj = GameObject.Instantiate(
            coinVFX, 
            pos, 
            Quaternion.identity);
        
        var particleSystem = particleObj.GetComponent<ParticleSystem>();
        var particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        
        // Настраиваем анимацию
        var sequence = DOTween.Sequence();

        particleObj.transform.DOScale(Vector3.one * 0.55f, fadeOut).From(Vector3.zero).SetEase(Ease.OutQuad);

        if (target is Card card)
        {
            sequence.Join(particleObj.transform.DOMove(card.transform.position, FlightDuration)
                .SetEase(Ease.InOutBack, 0.6f));
        }
        else if (target is Deck deck)
        {
            sequence.Join(particleObj.transform.DOMove(deck.transform.position, FlightDuration)
                .SetEase(Ease.InOutBack, 0.6f));
        }
        else if (target is Player p)
        {
            sequence.Join(particleObj.transform.DOMove(p.Get<TagBasePlayerData>().characterCard.transform.position, FlightDuration)
                .SetEase(Ease.InOutBack, 0.6f));
        }
        

        // Ждем завершения анимации
        await sequence.AsyncWaitForCompletion();
        
        AddCoin.Invoke();
        particleObj.transform.DOScale(Vector3.zero, fadeOut).SetEase(Ease.InQuad).OnComplete(() =>
        {
            GameObject.Destroy(particleObj);
        });
        
    }
    public static async UniTask CreateParcicle(this Card card, string id)
    {
        float particleScale = 1.1f / Card.CARDSIZE;
        float deltaY = 0.5f;
        
        CMSEntity entity = CMS.Get<CMSEntity>(id);
        Sprite spriteBuff = entity.Get<TagSprite>().sprite; 
        AudioClip sound = entity.Get<TagAudioHolder>().clip;
            GameObject g = new GameObject("Particle")
        {
            transform =
            {
                position = card.transform.position + Vector3.down * deltaY / 1.5f,
                localScale = Vector3.zero
            }
        };
        SpriteRenderer render = g.AddComponent<SpriteRenderer>();
        render.sortingOrder = 10000;
        render.sprite = spriteBuff;
        
        if(sound != null) G.AudioManager.PlaySound(sound, 0);
        
        bool trigger = false;
        g.transform.DOMove(card.transform.position, 0.25f)
            .SetEase(Ease.InQuad);
        g.transform.DOScale(Vector3.one * particleScale * card.transform.lossyScale.x, 0.25f)
            .SetEase(Ease.InQuad)
            .onComplete = () => trigger = true;
            

        while (!trigger) await UniTask.Yield();
        trigger = false;

        g.transform.DOMove(g.transform.position + Vector3.up * deltaY, 0.5f);
        render.DOFade(0, 0.5f)
            .onComplete = () => trigger = true;

        while (!trigger) await UniTask.Yield();
        GameObject.Destroy(g);
    }
    private static List<Material> GetAllMaterialsInChildren(this Card card)
    {
        List<Material> materials = new List<Material>();
        Queue<GameObject> queue = new Queue<GameObject>();
        queue.Enqueue(card.gameObject);

        while (queue.Count > 0)
        {
            GameObject current = queue.Dequeue();

            Renderer renderer = current.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                materials.Add(renderer.material);
            }

            foreach (Transform child in current.transform)
            {
                queue.Enqueue(child.gameObject);
            }
        }

        return materials;
    }
}
public static class DecksVisualEffects
{
    public static void AddOutline(this Deck deck, Color color)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        Material mat = deck.faceRenderer.material;

        mat.SetColor("_OutlineColor", color);
        DOTween.To(
            () => mat.GetFloat("_OutlineAlpha"), 
            x => mat.SetFloat("_OutlineAlpha", x),
            config.outlineAlphaParam,                         
            config.outlineTimeToChange                                
        );
        DOTween.To(
            () => mat.GetFloat("_OutlineWidth"), 
            x => mat.SetFloat("_OutlineWidth", x),
            config.outlineWidth,                         
            config.outlineTimeToChange                                
        );
    }
    public static void RemoveOutline(this Deck deck)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        Material mat = deck.faceRenderer.material;
        
        DOTween.To(
            () => mat.GetFloat("_OutlineAlpha"), 
            x => mat.SetFloat("_OutlineAlpha", x),
            0,                         
            config.outlineTimeToChange                                
        );
        DOTween.To(
            () => mat.GetFloat("_OutlineWidth"), 
            x => mat.SetFloat("_OutlineWidth", x),
            0,                         
            config.outlineTimeToChange                                
        );
    }
    public static void AddGlow(this Deck deck)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        Material mat = deck.faceRenderer.material;
        
        DOTween.To(
            () => mat.GetFloat("_Glow"), 
            x => mat.SetFloat("_Glow", x),
            config.glowBaseValue,                         
            config.glowTimeToChange                                
        );
    }
    public static void RemoveGlow(this Deck deck)
    {
        ConfigVisualEffects config = CMS.GetOnlyOneComponent<ConfigVisualEffects>();
        Material mat = deck.faceRenderer.material;
        
        DOTween.To(
            () => mat.GetFloat("_Glow"), 
            x => mat.SetFloat("_Glow", x),
            0f,                                  
            config.glowTimeToChange                                
        );
    }
}
[Serializable]
public class VFXConfig : EntityComponentDefinition
{
    public GameObject hitEffect;
    public GameObject coinVFX;
    public GameObject boomVFX;
    public GameObject smokeVFX;
    public GameObject upgradeVFX;
}
