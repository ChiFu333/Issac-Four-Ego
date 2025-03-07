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
    public async Task<Entity> SelectCardByType(string zoneName)
    {
        Player initiator = G.Players.priorPlayer;
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
                    if(hit.collider.GetComponent<Entity>() != null)
                    {
                        Dictionary<string, bool> rightBool = new Dictionary<string, bool>()
                        {
                            {"InPlay", true},
                            {"MyHand", hit.collider.transform.parent.parent == initiator?.transform},
                            {"Shop", hit.collider.transform.parent == G.shop.transform},
                            {"MonsterZone", hit.collider.transform.parent == G.monsterZone.transform}
                        };

                        if(rightBool[zoneName])
                        {
                            QuitSelecting();
                            UIOnDeck.inst.ChangeButtonsActive();
                            return hit.collider.GetComponent<Entity>();
                        }
                    }
                }
            }
        }
    }
    public async Task<Entity> SelectCardByTypes<T1,T2>(string zoneName) where T1 : Card where T2 : Card
    {
        Player initiator = G.Players.priorPlayer;
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
                    if(hit.collider.GetComponent<Entity>() != null)
                    {
                        Dictionary<string, bool> rightBool = new Dictionary<string, bool>()
                        {
                            {"InPlay", true},
                            {"MyHand", hit.collider.transform.parent.parent == initiator.transform},
                            {"Shop", hit.collider.transform.parent == G.shop.transform},
                            {"MonsterZone", hit.collider.transform.parent == G.monsterZone.transform}
                        };

                        if(rightBool[zoneName])
                        {
                            QuitSelecting();
                            UIOnDeck.inst.ChangeButtonsActive();
                            return hit.collider.GetComponent<Entity>();
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
        G.Players.RestorePrior();
    }
    public void CancelSelecting()
    {
        if(isSelectingSomething) ForceQuit = true;
    }
}
