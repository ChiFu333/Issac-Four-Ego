using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class SubSystems : MonoBehaviour
{
    public static SubSystems inst;
    public bool isSelectingSomething = false;
    private bool ForceQuit = false;
    public void Awake()
    {
        inst = this;
    }
    public async Task<T> SelectCardByType<T>(string zoneName) where T : Card
    {
        Player initiator = GameMaster.inst.turnManager.priorPlayer();
        isSelectingSomething = true;
        UIOnDeck.inst.ChangeButtonsActive();
        while (true)
        {
            await Task.Yield();
            if(ForceQuit)
            {
                QuitSelecting();
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
                            {"MyHand", hit.collider.transform.parent.parent == initiator?.transform},
                            {"Shop", hit.collider.transform.parent == GameMaster.inst.shop.transform},
                            {"MonsterZone", hit.collider.transform.parent == GameMaster.inst.monsterZone.transform}
                        };

                        if(rightBool[zoneName])
                        {
                            QuitSelecting();
                            UIOnDeck.inst.ChangeButtonsActive();
                            return hit.collider.GetComponent<T>();
                        }
                    }
                }
            }
        }
    }
    public async Task<Card> SelectCardByTypes<T1,T2>(string zoneName) where T1 : Card where T2 : Card
    {
        Player initiator = GameMaster.inst.turnManager.priorPlayer();
        isSelectingSomething = true;
        UIOnDeck.inst.ChangeButtonsActive();
        while (true)
        {
            await Task.Yield();
            if(ForceQuit)
            {
                QuitSelecting();
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
                    if(hit.collider.GetComponent<T1>() != null || hit.collider.GetComponent<T2>() != null)
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
                            QuitSelecting();
                            UIOnDeck.inst.ChangeButtonsActive();
                            return hit.collider.GetComponent<Card>();
                        }
                    }
                }
            }
        }
    }
    private void QuitSelecting()
    {
        isSelectingSomething = false;
        ForceQuit = false;
        GameMaster.inst.turnManager.RestorePrior();
    }
    public void CancelSelecting()
    {
        if(isSelectingSomething) ForceQuit = true;
    }
}
