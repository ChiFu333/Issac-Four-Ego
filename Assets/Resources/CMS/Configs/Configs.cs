using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable]
public class ConfigMain : EntityComponentDefinition
{
#if UNITY_EDITOR
    [Header("Fade settings")] 
    public bool showFading = true;
    [Header("Localization settings")] 
    public bool showLocOnStart = true;
    public bool RewriteLocWithRus = false;
#else
        [HideInInspector] public readonly bool showFading = true;
        [HideInInspector] public readonly bool showLocOnStart = true;
        [HideInInspector] public readonly bool RewriteLocWithRus = false;
#endif
}
[Serializable]
public class ConfigLight : EntityComponentDefinition
{
    [Header("Times")] 
    public float timeToChangeIntensity = 0.4f;
    public float timeToChangeColor = 0.4f;
    [Header("Intensity")] 
    public float NORMAL_INTENSITY = 1.4f;
    public float NORMAL_LIGHTOUT = 0.4f;
    public float DIM_LIGHTOUT = 0.2f;
    [Header("Colors")] 
    public Color START_PHASE_COLOR;
    public Color NORMAL_ACTION_COLOR;
    public Color SHOP_BUING_COLOR;
    public Color END_PHASE_COLOR;
}
[Serializable]
public class ConfigVisualEffects : EntityComponentDefinition
{
    [Header("Outline")] 
    public float outlineTimeToChange = 0.25f;
    public float outlineAlphaParam = 0.85f;
    public float outlineWidth = 0.07f;
    [Header("Glow")] 
    public float glowTimeToChange = 0.25f;
    public float glowBaseValue = 0.3f;
    [Header("Lit")] 
    public float litTimeToChange = 0.25f;
    public float litBaseValue = 0;
}