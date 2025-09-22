using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
public class StackSystem : MonoBehaviour
{
    [HideInInspector] public StackVisual Visual;
    public Stack<StackUnit> Stack = new Stack<StackUnit>();
    private TagStackUnitIcons config;
    private void Awake()
    {
        Visual = GetComponentInChildren<StackVisual>();
    }

    public async UniTask PutStackUnit(StackUnit unit)
    {
        Stack.Push(unit);
        await unit.PutUnitInStack();
        await UniTask.Delay(300);
    }
    public async UniTask ResolveStackUnit(StackUnit unit)
    {
        await unit.PlayStackEffect();
        if (unit is StackUnitCube cube && cube.cube == null) return;
        await RemoveStackUnit(unit);
    }
    public async UniTask RemoveStackUnit(StackUnit unit)
    {
        RemoveElementFromStack(unit);
        await unit.RemoveMeFromStack();
        await UniTask.Delay(300);
    }

    private void RemoveElementFromStack(StackUnit unit)
    {
        Stack<StackUnit> s = Stack;
        StackUnit[] st = s.ToArray();
        int idx = Array.IndexOf(st.ToArray(), unit);
        Stack<StackUnit> b = new Stack<StackUnit>();
        for (int i = st.Length -1; i >= 0; i--)
        {
            if (i != idx)
            {
                b.Push(st[i]);
            }
        }

        Stack = b;
    }
    public async UniTask ResolveTopUnit()
    {
        if (Stack.Count == 0) return;
        await ResolveStackUnit(Stack.Peek());
    }
}

public interface StackUnit : ISelectableTarget
{
    public UniTask PutUnitInStack();
    public UniTask PlayStackEffect();
    public UniTask RemoveMeFromStack();
}

public class StackUnitLootCard : StackUnit
{
    public Card LootCard;
    public StackUnitLootCard(Card lc)
    {
        LootCard = lc;
    }
    public async UniTask PutUnitInStack()
    {
        LootCard.myStackUnit = this;
        await G.Main.StackSystem.Visual.PutLootCard(LootCard);
    }
    public async UniTask PlayStackEffect()
    {
        if (LootCard.Get<TagPlayEffect>() != null)
        {
            Effect eff = LootCard.Get<TagPlayEffect>().effect;
            await G.Main.StackSystem.Visual.PutNearStack(LootCard);
            await eff.PlayEffect();
            await UniTask.Delay(100);
        }
    }
    public async UniTask RemoveMeFromStack()
    {
        LootCard.gameObject.GetComponentInChildren<StackUnitIconVisual>().RemoveIcon();
        LootCard.myStackUnit = null;
        await G.Main.StackSystem.Visual.SortVisual();
        if(!LootCard.Is<TagIsItem>() && !LootCard.Is<TagIsSoul>()) await LootCard.DiscardCard();
        
    }
}

public class StackUnitCube : StackUnit
{
    public CubeVisual cube;
    public int value;
    public bool isReady;
    public StackUnitCube(int v, bool isR)
    {
        value = v;
        isReady = isR;
    }
    public async UniTask PutUnitInStack()
    {
        cube = await G.Main.StackSystem.Visual.CreateAndPutCubeView(value, isReady);
        if (!isReady)
        {
            await FindAndTriggerCubeRolledEffects(value, false);
        }
    }
    public async UniTask PlayStackEffect()
    {
        await G.Main.StackSystem.RemoveStackUnit(this);
        if (!isReady)
        {
            StackUnit su = new StackUnitCube(value, true);
            await G.Main.StackSystem.PutStackUnit(su);
            foreach (var eff in triggeredEffects)
            {
                await G.Main.StackSystem.PutStackUnit(new StackUnitTriggeredEffect(eff, StackCardUnitType.TriggeredEffect, this, value));
            }

            await FindAndTriggerCubeRolledEffects(value, true);
            
            await UniTask.Delay(100);
            await G.Main.StackSystem.RemoveStackUnit(su);
            await UniTask.Delay(100);
        }
    }
    public async UniTask RemoveMeFromStack()
    {
        await G.Main.StackSystem.Visual.RemoveCubeView(cube);
        await G.Main.StackSystem.Visual.SortVisual();
    }

    public List<Effect> triggeredEffects = new List<Effect>();
    public void AddTriggeredEffect(Effect eff)
    {
        triggeredEffects.Add(eff);
    }

    private async UniTask FindAndTriggerCubeRolledEffects(int result, bool isDone)
    {
        foreach (var card in G.Main.AllCards.Where(c => c.ActiveSelf == true && c.Get<TagCardType>().cloneType == StackCardUnitType.None).ToList())
        {
            // Находим все компоненты TagOnDiceRolledTriggerEffect в state
            var triggerComponents = card.state
                .OfType<TagOnDiceRolledTriggerEffect>()
                .ToList();

            foreach (var triggerComponent in triggerComponents)
            {
                if (!card.Is<TagInShop>() && triggerComponent.diceValueToTrigger == result)
                {
                    if (!triggerComponent.isWhenShouldRoll == isDone)
                    {
                        await triggerComponent.TriggerDiceRolledEffect(this);
                    }
                }
            }
        }
    }
    public async UniTask Reroll()
    {
        int result = await G.Main.CubeThrower.ThrowCube();
        ChangeCubeValueTo(result);
        if (!isReady)
        {
            await FindAndTriggerCubeRolledEffects(value, false);
        }
    }
    public void ChangeCubeValueTo(int count)
    {
        value = count;
        if (value > 6) value = 6;
        if (value < 1) value = 1;
        cube.SetValue(value, isReady);
    }
}

public class StackUnitTriggeredEffect : StackUnit
{
    public Effect effectToPlay;
    public int valueToPlay;
    public Card templeCard;
    public StackCardUnitType type;
    public StackUnit source;
    public StackUnitTriggeredEffect(Effect eff, StackCardUnitType t, StackUnit s, int v = -1)
    {
        effectToPlay = eff;
        valueToPlay = v;
        type = t;
        source = s;
    }
    public async UniTask PutUnitInStack()
    {
        templeCard = await G.Main.StackSystem.Visual.CreateTriggeredEffectCard(effectToPlay.effectCardSource, this, source);
        templeCard.myStackUnit = this;
        effectToPlay.OverrideSource(templeCard);
    }
    public async UniTask PlayStackEffect()
    {
        await G.Main.StackSystem.Visual.PutNearStack(templeCard);
        if (valueToPlay != -1)
        {
            await (effectToPlay.effectLines[0] as LineThrowCube).DoResultAction(effectToPlay, valueToPlay);
        }
        else
        {
            await effectToPlay.PlayEffect();
        }
    }
    public async UniTask RemoveMeFromStack()
    {
        templeCard.myStackUnit = null;
        await G.Main.StackSystem.Visual.RemoveTriggeredEffectCard(templeCard);
        await G.Main.StackSystem.Visual.SortVisual();
    }
}

public enum StackCardUnitType
{
    None, ActivateValue, TriggeredEffect, TriggerDice1, TriggerDice2, TriggerDice3, TriggerDice4, TriggerDice5, TriggerDice6
}
public static class DiceExtentions
{
    public static StackCardUnitType GetUnitTypeOfDiceByInt(this int value)
    {
        if (value == 1)
        {
            return StackCardUnitType.TriggerDice1;
        }
        if (value == 2)
        {
            return StackCardUnitType.TriggerDice2;
        }
        if (value == 3)
        {
            return StackCardUnitType.TriggerDice3;
        }
        if (value == 4)
        {
            return StackCardUnitType.TriggerDice4;
        }
        if (value == 5)
        {
            return StackCardUnitType.TriggerDice5;
        }
        if (value == 6)
        {
            return StackCardUnitType.TriggerDice6;
        }

        return StackCardUnitType.None;
    }
    public static int GetIntByStackCardUnitType(this StackCardUnitType value)
    {
        if (value == StackCardUnitType.TriggerDice1)
        {
            return 1;
        }
        if (value == StackCardUnitType.TriggerDice2)
        {
            return 2;
        }
        if (value == StackCardUnitType.TriggerDice3)
        {
            return 3;
        }
        if (value == StackCardUnitType.TriggerDice4)
        {
            return 4;
        }
        if (value == StackCardUnitType.TriggerDice5)
        {
            return 5;
        }
        if (value == StackCardUnitType.TriggerDice6)
        {
            return 6;
        }

        return -1;
    }
}