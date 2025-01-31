using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Rendering;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

public class StackSystem : MonoBehaviour
{
    public static StackSystem inst;
    public Stack<StackEffect> stack = new Stack<StackEffect>();
    public Card cardTarget;             
    public bool prioreNow = false;
    public List<PrimalCardData> primalCards; //заявка на покупку, заявка на атаку, получение урона, смерть
    public List<CubeCardData> cubeCards;
    public void Awake() { inst = this;}
    public void PushEffect(StackEffect effect)
    {
        stack.Push(effect);
        UpdateUI();      
    }
    public async Task AgreeEffect()
    {
        if(stack.Count > 0)
        {
            StackEffect effect = stack.Peek();
            await effect.PlayStackEffect();
            
            UpdateUI();
            //if(stack.Count > 0 && stack.Peek() is CubeStackEffect cubeEff && cubeEff.endedValue) AgreeEffect();
        }
    }
    public void UpdateUI()
    {
        UIOnDeck.inst.UpdateStack();
    }
    public void PushPrimalEffect(PrimalEffect type, Card t, int effCount = 1, bool fight = false)
    {
        StackEffect eff = new PrimalStackEffect(type, t, effCount, fight);
        PushEffect(eff);
    }
    public void PushCubeEffect(int value, bool isEnded, bool fight, List<StackEffect> list = null)
    {
        StackEffect eff = new CubeStackEffect(cubeCards[value-1], isEnded, fight, list);
        PushEffect(eff);
    }
    public CubeStackEffect GetCubeInStack(bool isEnded)
    {
        StackEffect[] st = stack.ToArray();
        for(int i = st.Length - 1; i >= 0; i--)
        {
            if(st[i] is CubeStackEffect cubeEff)
            {
                return cubeEff.endedValue == isEnded ? cubeEff : null;
            }
        }
        return null;
    }
    public void GivePrior() => prioreNow = true;
    public async Task CancelEverythingInStack()
    {
        for(int i = 0; i < stack.Count; i++)
        {
            StackEffect effect = stack.Pop();
            await effect.RemoveMeFromStack();
        }
        prioreNow = false;
    }
    public async Task PushAndAgree(StackEffect effect)
    {
        await effect.Init();
        await effect.PlayStackEffect();
    }
}

public abstract class StackEffect
{
    public async virtual Task Init() { await Task.Yield();}
    public abstract Task PlayStackEffect();
    public abstract Sprite GetSprite(bool sourceSprite);
    public Task RemoveMeFromStack()
    {
        StackEffect[] st = StackSystem.inst.stack.ToArray();
        st.Reverse();
        int idx = Array.IndexOf(st.ToArray(), this);
        Stack<StackEffect> b = new Stack<StackEffect>();
        for (int i = 0; i < st.Length; i++)
        {
            if(i != idx)
            {
                b.Push(st[i]);
            }
        }
        StackSystem.inst.stack = b;
        StackSystem.inst.UpdateUI();
        return Task.CompletedTask;
    }
}
public class CardStackEffect : StackEffect
{
    public Card source {get; private set;}
    public Effect effect {get; private set;}
    private int manageEffect = -1;
    public bool triggeredEffect {get; private set;} = false;
    public CardStackEffect(Effect eff, Card s, bool trigge = false)
    {
        effect = eff;
        source = s;
        triggeredEffect = trigge;
    }
    public async override Task Init()
    {
        if(effect.type != EffectType.Roll)
        {
            manageEffect = await EffectSelector.inst.SelectEffect(source.GetData<CardData>().face, effect.effectActions.Count);
        }
        await effect.SetTargets(source, manageEffect);
    }
    public async override Task PlayStackEffect()
    {
        await RemoveMeFromStack();
        if(effect.type == EffectType.Roll)
        {
            List<StackEffect> effs = new List<StackEffect>();
            for(int i = 0; i < 6; i++)
            {
                List<EffectAction> temp = new List<EffectAction>
                {
                    effect.effectActions[i]
                };
                effs.Add(new CardStackEffect(new Effect(When.Now, 0, EffectType.Common, temp),source, true));
            }
            StackSystem.inst.PushCubeEffect(CubeManager.inst.ThrowDice(), false, false, effs);
        }
        else if(effect.type == EffectType.YouSelectOne || effect.type == EffectType.Common)
        {
            await effect.PlayEffect(manageEffect);
        }
        if(!triggeredEffect)
        {
            if(source is LootCard lootCard && !lootCard.isItem)
            {
                lootCard.DiscardCard();
            }
        }
    }
    public override Sprite GetSprite(bool sourceSprite)
    {
        return sourceSprite ? source.GetData<CardData>().face : effect.effectActions?[manageEffect == -1 ? 0 : manageEffect]?.subActions[0]?.targetCard?.GetData<CardData>().face;
    }
}
public class PrimalStackEffect : StackEffect
{
    public bool fightDamage;
    public PrimalCardData data;
    private Card target;
    public Sprite effectSprite;
    public int count;
    public int type;
    public PrimalStackEffect(PrimalEffect type, Card t, int c, bool fight = false)
    {
        this.type = (int)type;
        data = StackSystem.inst.primalCards[(int)type];
        target = t;
        count = c;
        fightDamage = fight;
    }
    public async override Task PlayStackEffect()
    {
        await RemoveMeFromStack();
        for(int i = 0; i < count; i++)
        {
            await data.action.SetTargets(target);
            await data.action.PlaySubActions();
        }
    }
    public override Sprite GetSprite(bool sourceSprite)
    {
        return sourceSprite ? data.face : target?.render.sprite;
    }
}
public enum PrimalEffect { BuyRequest, AttackRequest, Damage, Kill}
public class CubeStackEffect : StackEffect
{ 
    public bool fightCube;
    public bool endedValue;
    private CubeCardData data;
    private bool isNewValue = true;
    public List<StackEffect> myDiceTrigger = new List<StackEffect>();
    public CubeStackEffect(CubeCardData d, bool ended, bool fight, List<StackEffect> additionTrigger = null)
    {
        data = d;
        endedValue = ended;
        fightCube = fight;
        isNewValue = true;
        myDiceTrigger = additionTrigger;
    }
    public override async Task PlayStackEffect()
    {
        if(!endedValue)
        {
            if(isNewValue)
            {
                isNewValue = false;
                await TriggersSystem.diceWouldRoll[data.value - 1]?.PlayTriggeredEffects();
                if(TriggersSystem.diceWouldRoll[data.value - 1].triggeredStackEffects.Count != 0) return;
            }
            
            endedValue = true;
        }
        else
        {
            if(myDiceTrigger != null) TriggersSystem.diceRolls[data.value - 1].AddStackEffect(myDiceTrigger[data.value -1]);
            await TriggersSystem.diceRolls[data.value - 1]?.PlayTriggeredEffects();
            if(myDiceTrigger != null) TriggersSystem.diceRolls[data.value - 1].RemoveEffect(myDiceTrigger[data.value -1]);
            await RemoveMeFromStack();
        }
    }
    public void RethrowDice()
    {
        if(!endedValue)
        {
            data = StackSystem.inst.cubeCards[CubeManager.inst.ThrowDice()-1];
            isNewValue = true;
            StackSystem.inst.UpdateUI();
        }
    }
    public void ChangeCubeCount(int count)
    {
        int result = data.value;
        result += count;
        if(result < 1) result = 1;
        if(result > 6) result = 6;
        data = StackSystem.inst.cubeCards[result-1];
        isNewValue = true;
        StackSystem.inst.UpdateUI();
    }
    public void ChangeToCount(int count) 
    {
        data = StackSystem.inst.cubeCards[count-1];
        isNewValue = true;
        StackSystem.inst.UpdateUI();
    }
    public override Sprite GetSprite(bool sourceSprite)
    {
        return sourceSprite ? !endedValue ? data.face : data.end : null;//myDiceTrigger?[0].source?.GetData<CardData>().face;
    }
}