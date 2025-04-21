using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    
    public static List<TriggeredEvent> wouldTakeDamage = new List<TriggeredEvent>();
    public static List<TriggeredEvent> takeDamage = new List<TriggeredEvent>();
    
    public static List<TriggeredEvent> playerWouldDie = new List<TriggeredEvent>();
    public static List<TriggeredEvent> playerDie = new List<TriggeredEvent>();
    
    public static List<TriggeredEvent> atFirstBattleThrow = new List<TriggeredEvent>();
    
    public static List<TriggeredEvent> atAttackDeclaration = new List<TriggeredEvent>();
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
        
        for (int i = 0; i < 5; i++) wouldTakeDamage.Add(new TriggeredEvent());
        for (int i = 0; i < 5; i++) takeDamage.Add(new TriggeredEvent());
        
        for (int i = 0; i < 5; i++) playerWouldDie.Add(new TriggeredEvent());
        for (int i = 0; i < 5; i++) playerDie.Add(new TriggeredEvent());
        
        for (int i = 0; i < 5; i++) atFirstBattleThrow.Add(new TriggeredEvent());
        
        for (int i = 0; i < 5; i++) atAttackDeclaration.Add(new TriggeredEvent());
    }
    public static void CleanAll()
    {
        for (int i = 0; i < 5; i++) withStartTurn[i].Clean();
        for (int i = 0; i < 5; i++) withEndTurn[i].Clean();

        dicePuttedInStack.Clean();
        
        for (int i = 0; i < 5; i++) diceWouldRoll[i].Clean();
        for (int i = 0; i < 5; i++) diceRolls[i].Clean();
        
        for (int i = 0; i < 5; i++) wouldTakeDamage[i].Clean();
        for (int i = 0; i < 5; i++) takeDamage[i].Clean();
        
        for (int i = 0; i < 5; i++) playerWouldDie[i].Clean();
        for (int i = 0; i < 5; i++) playerDie[i].Clean();
        
        for (int i = 0; i < 5; i++) atFirstBattleThrow[i].Clean();
        
        for (int i = 0; i < 5; i++) atAttackDeclaration[i].Clean();
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
            case When.AtDamageWouldTaken:
            { 
                wouldTakeDamage[0].AddStackEffect(eff);
            } 
            break;
            case When.AtDamageWouldTakenByMe:
            {
                wouldTakeDamage[id].AddStackEffect(eff);
            } 
            break;
            case When.AtDamageTaken:
            { 
                takeDamage[0].AddStackEffect(eff);
            } 
            break;
            case When.AtDamageTakenByMe:
            {
                takeDamage[id].AddStackEffect(eff);
            } 
            break;
            case When.AtPlayerWouldDie:
            { 
                playerWouldDie[0].AddStackEffect(eff);
            } 
            break;
            case When.AtPlayerWouldDieMe:
            {
                playerWouldDie[id].AddStackEffect(eff);
            } 
            break;
            case When.AtPlayerDie:
            { 
                playerDie[0].AddStackEffect(eff);
            } 
            break;
            case When.AtPlayerDieMe:
            {
                playerDie[id].AddStackEffect(eff);
            } 
            break;
            case When.AtFirstBattleThrow:
            {
                atFirstBattleThrow[id].AddStackEffect(eff);
            } 
            break;
            case When.AtAttackDeclare:
            { 
                atAttackDeclaration[0].AddStackEffect(eff);
            } 
                break;
            case When.AtAttackDeclareMe:
            {
                atAttackDeclaration[id].AddStackEffect(eff);
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
        int c = triggeredStackEffects.Count;
        for(int i = 0; i < c; i++)
        {
            await StackSystem.inst.PushEffect(triggeredStackEffects[i]);
        }
    }
    public void RemoveEffect(StackEffect eff)
    {
        triggeredStackEffects.Remove(eff);
    }
}