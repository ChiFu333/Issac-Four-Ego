using UnityEngine;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.Serialization;

public interface IOnRealTurnEnd { UniTask OnRealTurnEnd(Player p); }
public interface IOnTurnStart { UniTask OnTurnStart(Player p); }
public interface IOnTurnEnd { UniTask OnTurnEnd(Player p); }
public interface IOnTakeDamage { UniTask OnTakeDamage(Card c); }
public interface IOnEnterGame { UniTask OnEnterGame(); }
[Serializable] public class TagTappableEffect : EntityComponentDefinition, IInitable, ITapEffect
{
    public Effect effect;
    private Card card;
    public void Init(Card c)
    {
        card = c;
        effect = effect.DeepCopy();
    }

    public async UniTask<bool> OnTap()
    {
        bool b = await effect.SetTargets(card.GetMyPlayer(), card);
        if (!b) return false;
        StackUnitTriggeredEffect su = new StackUnitTriggeredEffect(effect, StackCardUnitType.ActivateValue, null);
        await G.Main.StackSystem.PutStackUnit(su);
        return true;
    }
}

[Serializable]
public class TagOnTurnStartEffect : EntityComponentDefinition, IInitable, IOnTurnStart
{
    public bool onMyTurnStart;
    public Effect effect;
    private Card card;
    public void Init(Card c)
    {
        card = c;
        effect = effect.DeepCopy();
    }
    public async UniTask OnTurnStart(Player p)
    {
        if (!onMyTurnStart || card.GetMyPlayer() == p)
        {
            bool b = await effect.SetTargets(card.GetMyPlayer(), card);
            if (!b) return;
            StackUnitTriggeredEffect su = new StackUnitTriggeredEffect(effect, StackCardUnitType.TriggeredEffect, null);
            await G.Main.StackSystem.PutStackUnit(su);
        }
    }
}
[Serializable]
public class TagOnTurnEndEffect : EntityComponentDefinition, IInitable, IOnTurnEnd
{
    public bool onMyTurnStart;
    public Effect effect;
    private Card card;
    public void Init(Card c)
    {
        card = c;
        effect = effect.DeepCopy();
    }
    public async UniTask OnTurnEnd(Player p)
    {
        if (!onMyTurnStart || card.GetMyPlayer() == p)
        {
            bool b = await effect.SetTargets(card.GetMyPlayer(), card);
            if (!b) return;
            StackUnitTriggeredEffect su = new StackUnitTriggeredEffect(effect, StackCardUnitType.TriggeredEffect, null);
            await G.Main.StackSystem.PutStackUnit(su);
        }
    }
}
[Serializable]
public class TagOnTakeDamageEffect : EntityComponentDefinition, IInitable, IOnTakeDamage
{
    public bool isMyPlayerTakeDamage;
    public Effect effect;
    private Card card;
    public void Init(Card c)
    {
        card = c;
        effect = effect.DeepCopy();
    }
    public async UniTask OnTakeDamage(Card c)
    {
        if (!isMyPlayerTakeDamage || (card.GetMyPlayer() == c.GetMyPlayer() && c.GetMyPlayer().Get<TagBasePlayerData>().characterCard == c))
        {
            bool b = await effect.SetTargets(card.GetMyPlayer(), card);
            if (!b) return;
            StackUnitTriggeredEffect su = new StackUnitTriggeredEffect(effect, StackCardUnitType.TriggeredEffect, null);
            await G.Main.StackSystem.PutStackUnit(su);
        }
    }
}
[Serializable]
public class TagOnEnterGameEffect : EntityComponentDefinition, IInitable, IOnEnterGame
{
    public Effect effect;
    private Card card;
    public void Init(Card c)
    {
        card = c;
        effect = effect.DeepCopy();
    }

    public async UniTask OnEnterGame()
    {
        bool b = await effect.SetTargets(card.GetMyPlayer(), card);
        if (!b) return;
        StackUnitTriggeredEffect su = new StackUnitTriggeredEffect(effect, StackCardUnitType.TriggeredEffect, null);
        await G.Main.StackSystem.PutStackUnit(su);
    }
}
[Serializable]
public class TagOnDiceRolledTriggerEffect : EntityComponentDefinition, IInitable
{
    public bool myRoll;
    [FormerlySerializedAs("isWhenShouldRool")] public bool isWhenShouldRoll;
    public int diceValueToTrigger;
    
    public Effect effect;
    private Card card;
    public void Init(Card c)
    {
        card = c;
        effect = effect.DeepCopy();
    }

    public async UniTask TriggerDiceRolledEffect(StackUnit suSource)
    {
        card.stackUnitCreatedMe = suSource as StackUnitCube;
        bool b = await effect.SetTargets(card.GetMyPlayer(), card);
        if (!b) return;
        StackUnitTriggeredEffect su = new StackUnitTriggeredEffect(effect, diceValueToTrigger.GetUnitTypeOfDiceByInt(), suSource);
        await G.Main.StackSystem.PutStackUnit(su);
    }
}