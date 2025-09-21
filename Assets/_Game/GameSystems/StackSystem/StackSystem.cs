using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor.Rendering;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

public class StackSystem : MonoBehaviour
{
    public static StackSystem inst;
    public Stack<StackEffect> stack = new Stack<StackEffect>();
    public Entity cardTarget;
    public CardDeck deckTarget;
    public bool prioreNow = false;
    public List<PrimalCardData> primalCards; //заявка на покупку, заявка на атаку, получение урона, смерть
    public List<CubeCardData> cubeCards;
    public void Awake() { inst = this; }
    
    public async UniTask PushEffect(StackEffect effect)
    {
        if (effect is CardStackEffect cse && (cse.effect.type == EffectType.Common) && cse.effect.effectActions[0].subActions[0].actionType == ActionType.none) return;
        //Debug.Log("First: " + (effect is CardStackEffect ce && (ce.effect.type == EffectType.Common)));
        //Debug.Log("Second: " + (effect is CardStackEffect cs && cs.effect.effectActions == null));
        await effect.Init();    
        stack.Push(effect);
        UpdateUI();
        
        if (effect is PrimalStackEffect pse && pse.type == (int)PrimalEffect.Damage && pse.target.GetTag<CardTypeTag>().cardType == CardType.characterCard)
        {
            await TriggersSystem.wouldTakeDamage[0]?.PlayTriggeredEffects()!;
            await TriggersSystem.wouldTakeDamage[1 + G.Players.GetPlayerId(pse.target.GetMyPlayer())]?.PlayTriggeredEffects()!;
        }
        if (effect is PrimalStackEffect pse2 && pse2.type == (int)PrimalEffect.Kill && pse2.target.GetTag<CardTypeTag>().cardType == CardType.characterCard)
        {
            await TriggersSystem.playerWouldDie[0]?.PlayTriggeredEffects()!;
            await TriggersSystem.playerWouldDie[1 + G.Players.GetPlayerId(pse2.target.GetMyPlayer())]?.PlayTriggeredEffects()!;
        }
    }
    public async UniTask AgreeEffect()
    {
        if (stack.Count > 0)
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
    public async UniTask PushPrimalEffect(PrimalEffect type, Entity t, int effCount = 1, bool fight = false)
    {
        StackEffect eff = new PrimalStackEffect(type, t, effCount, fight);
        await PushEffect(eff);
    }
    public async UniTask PushCubeEffect(int value, bool isEnded, bool fight, List<StackEffect> list = null)
    {
        StackEffect eff = new CubeStackEffect(cubeCards[value - 1], isEnded, fight, list);
        await PushEffect(eff);
    }
    public CubeStackEffect GetCubeInStack(bool isEnded)
    {
        StackEffect[] st = stack.ToArray();
        for (int i = st.Length - 1; i >= 0; i--)
        {
            if (st[i] is CubeStackEffect cubeEff)
            {
                return cubeEff.endedValue == isEnded ? cubeEff : null;
            }
        }
        return null;
    }
    public void GivePrior() => prioreNow = true;
    public async UniTask CancelEverythingInStack()
    {
        int count = stack.Count;
        List<StackEffect> sfl = new List<StackEffect>();
        for (int i = 0; i < count; i++)
        {
            sfl.Add(stack.Pop());
        }
        UpdateUI();
        for(int i = 0; i < count; i++)
        {
            if (sfl[i] is CardStackEffect cardEff && cardEff.source.GetTag<CardTypeTag>().cardType == CardType.lootCard /*&& !lootCard.isItem*/) await cardEff.source.DiscardEntity();
            else if (sfl[i] is CardStackEffect cardEff2 && cardEff2.source.GetTag<CardTypeTag>().cardType == CardType.eventCard /*&& !eventCard.isCurse*/) 
            {
                await cardEff2.source.DiscardEntity();
            }
        }
        prioreNow = false;
        
    }
    public async UniTask PushAndAgree(StackEffect effect)
    {
        await effect.Init();
        await effect.PlayStackEffect();
    }

    public PrimalStackEffect GetTopDamage()
    {
        StackEffect[] st = StackSystem.inst.stack.ToArray();
        for(int i = st.Length - 1; i >= 0; i--)
        {
            if(st[i] is PrimalStackEffect pse && pse.type == (int)PrimalEffect.Damage)
            {
                return pse;
            }
        }

        return null;
    }

    public PrimalStackEffect GetTopDeath()
    {
        StackEffect[] st = StackSystem.inst.stack.ToArray();
        for(int i = st.Length - 1; i >= 0; i--)
        {
            if(st[i] is PrimalStackEffect pse && pse.type == (int)PrimalEffect.Kill)
            {
                return pse;
            }
        }

        return null;
    }
}

public abstract class StackEffect
{
    public virtual async UniTask Init() { await UniTask.Yield(); }
    public abstract UniTask PlayStackEffect();
    public abstract Sprite GetSprite(bool sourceSprite);
    public async UniTask RemoveMeFromStack()
    {
        Stack<StackEffect> s = StackSystem.inst.stack;
        StackEffect[] st = s.ToArray();
        int idx = Array.IndexOf(st.ToArray(), this);
        Stack<StackEffect> b = new Stack<StackEffect>();
        for (int i = st.Length -1; i >= 0; i--)
        {
            if (i != idx)
            {
                b.Push(st[i]);
            }
        }

        int slotToFade = st.Length - 1 - idx; 


        StackSystem.inst.stack = b;
        await UniTask.Yield();
        StackSystem.inst.UpdateUI();
    }
}
public class CardStackEffect : StackEffect
{
    public Entity source { get; private set; }
    public Effect effect { get; private set; }
    private int manageEffect = -1;
    public bool triggeredEffect { get; private set; } = false;
    public CardStackEffect(Effect eff, Entity s, bool trigge = false)
    {
        effect = eff;
        source = s;
        triggeredEffect = trigge;
    }
    public override async UniTask Init()
    {
        if (effect.type != EffectType.Roll && effect.type != EffectType.RollEffectCount)
        {
            manageEffect = await EffectSelector.inst.SelectEffect(source.GetTag<CardSpritesData>().front, effect.effectActions.Count);
        }

        if (effect.type == EffectType.RollEffectCount) manageEffect = -2;
        if(effect.effectActions[0].subActions[0].targetCard == null && effect.type != EffectType.Roll)
            await effect.SetTargets(source, manageEffect);
    }
    public override async UniTask PlayStackEffect()
    {
        if (source.GetTag<CardTypeTag>().cardType == CardType.eventCard/* && !eve.isCurse && !triggeredEffect*/)
        {
            //G.monsterZone.RemoveMonster(eve);
            await source.PutCardNearHand(G.Players.activePlayer.hand);
            //if(eve.GetData<EventCardData>().isCurse) eve.TurnIntoCurse();
        }
        await RemoveMeFromStack();
        if (effect.type == EffectType.Roll)
        {
            List<StackEffect> effs = new List<StackEffect>();
            for (int i = 0; i < 6; i++)
            {
                List<EffectAction> temp = new List<EffectAction>
                {
                    effect.effectActions[i]
                };
                effs.Add(new CardStackEffect(new Effect(When.Now, 0, EffectType.Common, temp), source, true));
            }
            await StackSystem.inst.PushCubeEffect(UnityEngine.Random.Range(1, 7), false, false, effs);
        }
        else if (effect.type == EffectType.RollEffectCount)
        {
            List<StackEffect> effs = new List<StackEffect>();
            for (int i = 0; i < 6; i++)
            {
                List<EffectAction> temp = new List<EffectAction>
                {
                    effect.effectActions[i]
                };
                effs.Add(new CardStackEffect(new Effect(When.Now, 0, EffectType.Common, temp), source, true));
            }
            await StackSystem.inst.PushCubeEffect(UnityEngine.Random.Range(1, 7), false, false, effs);
        }
        else if (effect.type == EffectType.YouSelectOne || effect.type == EffectType.Common)
        {
            await effect.PlayEffect(manageEffect);
        }
        if (source.GetTag<CardTypeTag>().cardType == CardType.lootCard)
            if(!source.HasTag<PassiveTrinketEffect>() || !source.GetTag<PassiveTrinketEffect>().turnedIntoTrinket) 
                await source.DiscardEntity();
        if(source.GetTag<CardTypeTag>().cardType == CardType.eventCard/* && !eventCard.isCurse*/) await source.DiscardEntity();
    }
    public override Sprite GetSprite(bool sourceSprite)
    {
        return sourceSprite ? 
            source.GetTag<CardSpritesData>().front :
            effect.effectActions?[manageEffect <= -1 ? 0 : manageEffect]?.subActions[0]?.targetCard?.GetTag<CardSpritesData>().front;
    }
}
public class PrimalStackEffect : StackEffect
{
    public bool fightDamage;
    public PrimalCardData data;
    public Entity target { get; private set; }
    public Sprite effectSprite;
    public int count;
    public int type;
    public PrimalStackEffect(PrimalEffect type, Entity t, int c, bool fight = false)
    {
        this.type = (int)type;
        data = StackSystem.inst.primalCards[(int)type];
        target = t;
        count = c;
        fightDamage = fight;
    }
    public async override UniTask PlayStackEffect()
    {
        if(type == (int)PrimalEffect.Kill) await RemoveMeFromStack();
        for (int i = 0; i < count; i++)
        {
            await data.action.SetTargets(target);
            await data.action.PlaySubActions();
        }
        if(type != (int)PrimalEffect.Kill) await RemoveMeFromStack();
    }
    public override Sprite GetSprite(bool sourceSprite)
    {
        return sourceSprite ? data.face : target?.visual.render.sprite;
    }
}
public enum PrimalEffect { Damage, Kill }
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
    public override async UniTask PlayStackEffect()
    {
        if (!endedValue)
        {
            if (isNewValue)
            {
                isNewValue = false;
                await TriggersSystem.diceWouldRoll[data.value - 1]?.PlayTriggeredEffects()!;
                if (TriggersSystem.diceWouldRoll[data.value - 1].triggeredStackEffects.Count != 0) return;
            }

            endedValue = true;
        }
        else
        {
            if (myDiceTrigger != null) TriggersSystem.diceRolls[data.value - 1].AddStackEffect(myDiceTrigger[data.value - 1]);
            await TriggersSystem.diceRolls[data.value - 1]?.PlayTriggeredEffects()!;
            if (myDiceTrigger != null) TriggersSystem.diceRolls[data.value - 1].RemoveEffect(myDiceTrigger[data.value - 1]);
            await RemoveMeFromStack();
        }
    }
    public void RethrowDice()
    {
        if (!endedValue)
        {
            int val = UnityEngine.Random.Range(1, 7) - 1;
            data = StackSystem.inst.cubeCards[val];
            isNewValue = true;
            StackSystem.inst.UpdateUI();
        }
    }
    public void ChangeCubeCount(int count)
    {
        int result = data.value;
        result += count;
        if (result < 1) result = 1;
        if (result > 6) result = 6;
        data = StackSystem.inst.cubeCards[result - 1];
        isNewValue = true;
        StackSystem.inst.UpdateUI();
    }
    public void ChangeToCount(int count)
    {
        data = StackSystem.inst.cubeCards[count - 1];
        isNewValue = true;
        StackSystem.inst.UpdateUI();
    }
    public override Sprite GetSprite(bool sourceSprite)
    {
        return sourceSprite ? !endedValue ? data.face : data.back : null;//myDiceTrigger?[0].source?.GetData<CardData>().face;
    }
}