using TMPro;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
public static class TextMeshProExtensions
{
    public static Color coinColor = new Color(0.92f, 0.83f, 0.4f);
    public static Color LoseColor = new Color(0.95f, 0.3f, 0.2f);
    
    public static async UniTask ShowDeltaEffectAsync(
        this TMP_Text mainText, 
        string valuePrefix, 
        int delta,
        string symbol,
        Color effectColor, 
        float duration = 1f,
        CancellationToken cancellationToken = default)
    {
        var (startIndex, endIndex) = FindValuePosition(mainText, valuePrefix);
        if (startIndex == -1) return;
        
        var worldPosition = GetTextWorldPosition(mainText, startIndex, endIndex);
        var effectObj = CreateEffectObject(mainText, delta, symbol, effectColor, worldPosition);
        
        effectObj.GetComponent<TMP_Text>().DOFade(0, duration - 0.05f).SetEase(Ease.InQuint);
        await effectObj.transform.DOMoveY(worldPosition.y + 0.6f, duration).SetEase(Ease.OutQuad).AsyncWaitForCompletion().AsUniTask();;
        
       if(effectObj != null) GameObject.Destroy(effectObj);
    }

    private static (int startIndex, int endIndex) FindValuePosition(TMP_Text text, string prefix)
    {
        int startIndex = text.text.IndexOf(prefix);
        if (startIndex == -1) return (-1, -1);

        startIndex += prefix.Length;
        int endIndex = text.text.IndexOf(" ", startIndex);
        if (endIndex == -1) endIndex = text.text.Length;

        return (startIndex, endIndex);
    }

    private static Vector3 GetTextWorldPosition(TMP_Text text, int startIndex, int endIndex)
    {
        text.ForceMeshUpdate();

        if (startIndex >= text.textInfo.characterCount || 
            endIndex > text.textInfo.characterCount)
            return text.transform.position;

        TMP_CharacterInfo firstChar = text.textInfo.characterInfo[startIndex];
        TMP_CharacterInfo lastChar = text.textInfo.characterInfo[endIndex - 1];

        Vector3 centerPos = (firstChar.bottomLeft + lastChar.topRight) / 2f;
        return text.transform.TransformPoint(centerPos);
    }

    private static GameObject CreateEffectObject(
        TMP_Text mainText, 
        int delta,
        string symbol,
        Color color, 
        Vector3 position)
    {
        var effectObj = new GameObject("DeltaEffect");
        var effectText = effectObj.AddComponent<TextMeshPro>();

        effectText.text = $"{(delta > 0 ? "+" : "")}{delta}{symbol}";
        effectText.color = color;
        effectText.fontSize = mainText.fontSize * 0.8f;
        effectText.alignment = mainText.alignment;
        effectText.sortingOrder = mainText.layoutPriority + 1;
        effectText.font = mainText.font;
        effectText.fontMaterial = mainText.fontMaterial;

        effectObj.transform.localScale = Vector3.one * 0.18f;
        effectObj.transform.position = position;

        return effectObj;
    }
}
