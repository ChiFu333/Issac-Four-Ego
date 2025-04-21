using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static partial class G
{
    public static CardSelector CardSelector;
}
public class CardSelector
{
    public bool isSelectingSomething = false;
    private bool ForceQuit = false;

    public async UniTask<Entity> SelectCardByType<T>(string zoneName, CardType cardType = CardType.none) where T : ITag
    {
        //Player initiator = G.Players.priorPlayer;
        isSelectingSomething = true;
        UIOnDeck.inst.ChangeButtonsActive();
        while (true)
        {
            await UniTask.Yield();
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
                if (hit.collider != null && hit.collider.GetComponent<Entity>() != null && hit.collider.GetComponent<Entity>().HasTag<T>())
                {
                    
                    if(cardType == CardType.none || hit.collider.GetComponent<Entity>().GetTag<CardTypeTag>().cardType == cardType)
                    {
                        //Debug.Log("CardPlayer: " + hit.collider.GetComponent<Entity>().GetMyPlayer().name);
                        //Debug.Log("Initiator: " + initiator.name);
                        Dictionary<string, bool> rightBool = new Dictionary<string, bool>()
                        {
                            {"InPlay", true},
                            {"MyHand", hit.collider.GetComponent<Entity>().GetMyPlayer() == G.Players.priorPlayer},
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
