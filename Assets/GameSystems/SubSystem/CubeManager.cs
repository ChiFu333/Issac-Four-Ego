using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CubeManager : MonoBehaviour
{
    public static CubeManager Inst;
    public bool AgreeThrow = false;
    public bool Throwing = false;
    public void Awake()
    {
        Inst = this;
    }
    public async Task<int> ThrowDice() 
    {
        Throwing = true;
        int result = Random.Range(1, 7);
        UIOnDeck.inst.UpdateCubeUI(result);
        while(!AgreeThrow) await Task.Yield();
        AgreeThrow = false;
        Throwing = false;
        UIOnDeck.inst.UpdateCubeUI(0);
        return result;
    }
    public void Agree()
    {
        if(Throwing) AgreeThrow = true;
    }
}
