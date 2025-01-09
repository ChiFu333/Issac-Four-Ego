using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class SubSystems : MonoBehaviour
{
    public static SubSystems Inst;
    public bool IsSelectingSomething = false;
    private bool ForceQuit = false;
    public void Awake()
    {
        Inst = this;
    }
    public async Task<T> SelectCardByType<T>(string zoneName) where T : Card
    {
        Player initiator = GameMaster.inst.turnManager.priorPlayer;
        IsSelectingSomething = true;
        Console.WriteText("Выбери карту");
        while (true)
        {
            await Task.Yield();
            if(ForceQuit)
            {
                IsSelectingSomething = false;
                ForceQuit = false;
                Console.WriteText("Действие отменено");
                return null;
            }   
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider != null)
                {
                    if(hit.collider.GetComponent<T>() != null)
                    {
                        Dictionary<string, bool> rightBool = new Dictionary<string, bool>()
                        {
                            {"InPlay", true},
                            {"MyHand", hit.collider.transform.parent.parent == initiator.transform},
                            {"Shop", hit.collider.transform.parent == GameMaster.inst.shop.transform},
                            {"MonsterZone", hit.collider.transform.parent == GameMaster.inst.monsterZone.transform}
                        };

                        if(rightBool[zoneName])
                        {
                            IsSelectingSomething = false;
                            GameMaster.inst.turnManager.RestorePrior();
                            return hit.collider.GetComponent<T>();
                        }
                    }
                }
            }
        }
    }
    public void CancelSelecting()
    {
        ForceQuit = true;
    }
}
