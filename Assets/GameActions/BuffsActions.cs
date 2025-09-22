using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

[AddTypeMenu(ActionNames.StatName + "1. PlusHP")]
[Serializable]
public class PlusMaxHP : IGameAction
{
    public BuffDuration duration;
    public int count;

    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        Player player = container.ConvertToPlayer();
        UniTask lastTask = UniTask.CompletedTask;

        await player.Get<TagBasePlayerData>().characterCard.Get<TagTemporaryBuff>()
            .AddTempBuff(
                new TempBuff((card) => card.Get<TagCharacteristics>().AddHp(count),
                    (card) => card.Get<TagCharacteristics>().AddHp(-count),
                    duration
                ));

        await lastTask;
        return true;
    }
}
[AddTypeMenu(ActionNames.StatName + "2. PlusAttack")]
[Serializable]
public class PlusMaxAttack : IGameAction
{
    public BuffDuration duration;
    public int count;

    public async UniTask<bool> Execute(Effect source, List<ISelectableTarget> container)
    {
        Player player = container.ConvertToPlayer();
        UniTask lastTask = UniTask.CompletedTask;

        await player.Get<TagBasePlayerData>().characterCard.Get<TagTemporaryBuff>()
            .AddTempBuff(
                new TempBuff((card) => card.Get<TagCharacteristics>().AddAttack(count),
                    (card) => card.Get<TagCharacteristics>().AddAttack(-count),
                    duration
                ));

        await lastTask;
        return true;
    }
}
