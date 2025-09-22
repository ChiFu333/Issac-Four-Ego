using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class LightController : MonoBehaviour, IService
{
    public Light2D globalLight;
    public ConfigLight config;
    public void Init()
    {
        config = CMS.GetOnlyOneComponent<ConfigLight>();
        SetupLight();
    }
    public void SetupLight()
    {
        globalLight = FindObjectsByType<Light2D>(FindObjectsSortMode.None)
            .FirstOrDefault(l => l.gameObject.name == "Global Light 2D");
        SetLight(config.NORMAL_INTENSITY).Forget();
    }
    public async UniTask SetLight(float intensity)
    {
        await DOTween.To(
            () => globalLight.intensity,
            x => globalLight.intensity = x,
            intensity,                                  
            config.timeToChangeIntensity                                
        ).ToUniTask(cancellationToken: this.destroyCancellationToken);
    }
    public async UniTask RestoreLight()
    {
        await DOTween.To(
            () => globalLight.intensity,
            x => globalLight.intensity = x,
            config.NORMAL_INTENSITY,
            config.timeToChangeIntensity
        ).ToUniTask(cancellationToken: this.destroyCancellationToken);
    }
    public async UniTask SetColor(Color color)
    {
        await DOTween.To(
            () => globalLight.color, 
            x => globalLight.color = x,
            color,                                  
            config.timeToChangeColor                               
        ).ToUniTask(cancellationToken: this.destroyCancellationToken);
    }
}