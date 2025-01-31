using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CubeManager : MonoBehaviour
{
    public static CubeManager inst;
    public void Awake()
    {
        inst = this;
    }
    public int ThrowDice() 
    {
        return GetCubeNumber();
    }
    private int GetCubeNumber()
    {
        int result = Random.Range(1, 7);
        return result;
    }
}
