using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int playerActiveID = 0;
    public Player playerActive => G.Main.Players[playerActiveID];
    
    public int GetPlayerId(Player p)
    {
        for(int i = 0; i < G.Main.Players.Count; i++)
        {
            if(G.Main.Players[i] == p) return i;
        }
        return -1;
    }
    public bool IsMyTurn()
    {
        return playerActiveID == 0;
    }
    public void SwitchTurn()
    {
        playerActiveID++;
        playerActiveID %= G.Main.Players.Count;
    }
}
