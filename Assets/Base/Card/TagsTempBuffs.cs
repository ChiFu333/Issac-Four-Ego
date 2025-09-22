using UnityEngine;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using System.Linq;

[Serializable]
public class TagTemporaryBuff : EntityComponentDefinition, IInitable, IOnRealTurnEnd
{
    private Card card;
    public List<TempBuff> buffs = new List<TempBuff>();
    public async UniTask AddTempBuff(TempBuff buff)
    {
        await buff.StartFunc.Invoke(card);
        buffs.Add(buff);
    }

    public void Init(Card c)
    {
        card = c;
    }

    public async UniTask OnRealTurnEnd(Player p)
    {
        foreach (var buff in buffs.Where(buff => buff.duration == BuffDuration.tillEndOfTurn).ToList())
        {
            await buff.DisposeFunc(card);
            buffs.Remove(buff);
        }
    }
}

public enum BuffDuration
{
    always = 0,
    tillEndOfTurn = 1,
}
public class TempBuff
{
    public BuffDuration duration;
    public Func<Card, UniTask> StartFunc;
    public Func<Card, UniTask> DisposeFunc;

    public TempBuff(Func<Card, UniTask> sf, Func<Card, UniTask> df, BuffDuration bd)
    {
        StartFunc = sf;
        DisposeFunc = df;
        duration = bd;
    }
}
[Serializable]
public class TagAudioHolder : EntityComponentDefinition
{
    public AudioClip clip;
}