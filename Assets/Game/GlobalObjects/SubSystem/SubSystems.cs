using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SubSystems : MonoBehaviour
{
    public static SubSystems Inst;
    public static Action<Card> OnSelected;
    public bool IsSelectingSomething = false;
    public void Awake()
    {
        Inst = this;
    }
    public void SelectCardByType<T>(Player initiator, string zoneName) where T : Card
    {
        IsSelectingSomething = true;
        StartCoroutine(SelectingCard<T>(initiator, zoneName));
        Console.WriteText("Выбери карту");
    }
    public void SelectShopCard(Player initiator)
    {
        
    }
    //Ввести понятие зоны, что только в таких штуках можно искать.
    public IEnumerator SelectingCard<T>(Player initiator, string zoneName) where T : Card
    {
        while (true)
        {
            yield return null;
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
                            {"MyHand", hit.collider.transform.parent.parent == initiator.transform},
                            {"Shop", hit.collider.transform.parent == Shop.Inst.transform}
                        };

                        if(rightBool[zoneName])
                        {
                            IsSelectingSomething = false;
                            OnSelected?.Invoke(hit.collider.GetComponent<T>());
                            OnSelected = null;
                            TurnManager.Inst.RestorePrior();
                            yield break;
                        }
                    }
                }
            }
        }
    }
}
