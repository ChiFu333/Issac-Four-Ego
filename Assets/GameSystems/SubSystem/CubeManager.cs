using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CubeManager : MonoBehaviour
{
    public static CubeManager inst;
    public bool throwing = false;
    public bool agreeThrowTrigger = false;
    public bool rethrowDiceTrigger = false;
    private int result;
    public void Awake()
    {
        inst = this;
    }
    public async Task<int> ThrowDice() 
    {
        throwing = true;
        result = GetCubeNumber();
        while(!agreeThrowTrigger) 
        {
            await Task.Yield();
            if(rethrowDiceTrigger)
            {
                rethrowDiceTrigger = false;
                result = GetCubeNumber();
            }
        }
        agreeThrowTrigger = false;
        throwing = false;
        if(GameMaster.inst.turnManager.priorPlayer.cubeModificator != 0)
        {
            ChangeCount(GameMaster.inst.turnManager.priorPlayer.cubeModificator);
            Console.WriteText("Кубик изменён");
            await Task.Delay(1000);
        }
        UIOnDeck.inst.UpdateCubeUI(0);
        return result;
    }
    private int GetCubeNumber()
    {
        int result = Random.Range(1, 7);
        UIOnDeck.inst.UpdateCubeUI(result);
        return result;
    }
    public void Agree()
    {
        if(throwing) agreeThrowTrigger = true;
    }
    public void RethrowDice()
    {
        rethrowDiceTrigger = true;
    }
    public void ChangeCount(int count)
    {
        result += count;
        if(result < 1) result = 1;
        if(result > 6) result = 6;
        UIOnDeck.inst.UpdateCubeUI(result);
    }
    public void ChangeToCount(int count) 
    { 
        result = count;
        UIOnDeck.inst.UpdateCubeUI(result);
    }
}
