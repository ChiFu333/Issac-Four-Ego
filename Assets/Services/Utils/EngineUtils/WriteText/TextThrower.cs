using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Febucci.UI;
using System.Threading;
using TMPro;

public class TextThrower : MonoBehaviour
{
    private TextAnimatorPlayer _textAnimator;
    private CancellationTokenSource _token;
    private void Awake()
    {
        _textAnimator = GetComponent<TextAnimatorPlayer>();
    }


    public async UniTask ThrowText(LocString text, VoiceSO voice)
    {
        if(_token != null) _token.Cancel();

        GetComponent<TMP_Text>().font = voice.font;
        GetComponent<TMP_Text>().color = voice.color;
        GetComponent<TMP_Text>().material = voice.textMaterial;
        _textAnimator.ShowText("");
        
        _textAnimator.ShowText(text.ToString());
        
        _token = new CancellationTokenSource();
        _ = PlayTypingSoundRepeatedly(voice.deltaSound, _token.Token, voice.voice);
        
        while (!_textAnimator.textAnimator.allLettersShown)
        {
            await UniTask.Yield(); // Ждём следующего кадра
        }
        _token.Cancel();
        
        //await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
        await UniTask.Delay(1000);
    }
    private async UniTask PlayTypingSoundRepeatedly(int intervalMs, CancellationToken cancellationToken, AudioClip sample)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            sample.PlayAsSoundRandomPitch(0.15f);
            // Ждём указанный интервал
            await UniTask.Delay(intervalMs, cancellationToken: cancellationToken);
        }
    }
}
