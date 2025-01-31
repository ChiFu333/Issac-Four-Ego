using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Rendering;
public delegate Task DoAndWait(StackEffect eff);
public class TriggersSystem : MonoBehaviour
{
    //public static List<DoAndWait> withStartTurnReplacement = new List<DoAndWait>();
    public static List<TriggeredEvent> withStartTurn = new List<TriggeredEvent>();
    //public static List<DoAndWait> withEndTurnReplacement = new List<DoAndWait>();
    public static List<TriggeredEvent> withEndTurn = new List<TriggeredEvent>();

    public static TriggeredEvent dicePuttedInStack = new TriggeredEvent();
    public static List<TriggeredEvent> diceWouldRoll = new List<TriggeredEvent>();
    public static List<TriggeredEvent> diceRolls = new List<TriggeredEvent>();
    public void Awake()
    {
        withStartTurn = new List<TriggeredEvent>();
        withEndTurn = new List<TriggeredEvent>();
        diceWouldRoll = new List<TriggeredEvent>();
        diceRolls = new List<TriggeredEvent>();

        //for (int i = 0; i < 5; i++) withStartTurnReplacement.Add(new DoAndWait(() => Task.CompletedTask));
        for (int i = 0; i < 5; i++) withStartTurn.Add(new TriggeredEvent());
        for (int i = 0; i < 5; i++) withEndTurn.Add(new TriggeredEvent());
        //for (int i = 0; i < 5; i++) withEndTurnReplacement.Add(new DoAndWait(() => Task.CompletedTask));
        for (int i = 0; i < 6; i++) diceWouldRoll.Add(new TriggeredEvent());
        for (int i = 0; i < 6; i++) diceRolls.Add(new TriggeredEvent());
    }
    public static void CleanAll()
    {
        for (int i = 0; i < 5; i++) withStartTurn[i].Clean();
        for (int i = 0; i < 5; i++) withEndTurn[i].Clean();

        dicePuttedInStack.Clean();
        for (int i = 0; i < 5; i++) diceWouldRoll[i].Clean();
        for (int i = 0; i < 5; i++) diceRolls[i].Clean();
    }
    public static void PutTrigger(StackEffect eff, int id = 0)
    {
        if(eff == null) return;
        CardStackEffect cardStack = eff as CardStackEffect;
        switch(cardStack.effect.when)
        {
            case When.AtTheStartOfTurn:
            {
                withStartTurn[0].AddStackEffect(eff);
            }
            break;
            case When.AtTheStartOfMyTurn:
            {
                withStartTurn[id].AddStackEffect(eff);
            }
            break;
            case When.AtTheEndOfTurn:
            {
                withEndTurn[0].AddStackEffect(eff);
            }
            break;
            case When.AtTheEndOfMyTurn:
            {
                withEndTurn[id].AddStackEffect(eff);
            }
            break;
            case When.AtDicePuttedInStack:
            {
                dicePuttedInStack.AddStackEffect(eff);
            }
            break;
            case When.AtDiceWouldRoll:
            {
                diceWouldRoll[cardStack.effect.diceValue-1].AddStackEffect(eff);
            }
            break;
            case When.AtDiceRolls:
            {
                diceRolls[cardStack.effect.diceValue-1].AddStackEffect(eff);
            }
            break;
        }
    }
}
public class TriggeredEvent
{
    //private bool replacementEffects = false;
    public List<StackEffect> triggeredStackEffects = new List<StackEffect>();
    public void Clean()
    {
        triggeredStackEffects = new List<StackEffect>();
    } 
    public void AddStackEffect(StackEffect eff)
    {
        triggeredStackEffects.Add(eff);
    }
    public async Task PlayTriggeredEffects()
    {
        if(triggeredStackEffects.Count == 0)
        {
            return;
        }
        for(int i = 0; i < triggeredStackEffects.Count; i++)
        {
            await triggeredStackEffects[i].Init();
            StackSystem.inst.PushEffect(triggeredStackEffects[i]);
        }
    }
    public void RemoveEffect(StackEffect eff)
    {
        triggeredStackEffects.Remove(eff);
    }
}