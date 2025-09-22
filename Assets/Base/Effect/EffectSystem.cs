using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;

[Serializable]
public class Effect
{
    [HideInInspector] public Player effectPlayerSource;
    [HideInInspector] public Card effectCardSource;
    [HideInInspector] public int actionSelectorNum = -1;
    [SerializeReference, SubclassSelector] 
    public List<OneGameAction> globalStorage;
    [SerializeReference, SubclassSelector]
    public List<ActionsController> effectLines;

    public async UniTask<bool> SetTargets(Player effPlayer, Card effCard = null)
    {
        effectPlayerSource = effPlayer;
        effectCardSource = effCard;
        
        effCard.AddGlow();
        foreach (var st in globalStorage)
        {
            
            if(st is OneGameAction { action: ITargetSelector })
            {
                if (!await st.SetTarget(effectCardSource, true))
                {
                    effCard.RemoveGlow();
                    return false;
                }
            }
            if(st is OneGameAction { action: ITargetConverter })
            {
                //НЕ КЛАСТЬ КОНВЕРТОРЫ В ЛОКАЛЬНОЕ ХРАНИЛИЩЕ
            }
        }
        effCard.RemoveGlow();

        foreach (var st in effectLines)
        {
            if(st is LineSelectAction lineSelectAction)
            {
                actionSelectorNum = await G.Main.EffectSelector.SelectEffect(effectCardSource, lineSelectAction.labels);
            }
        }

        return true;
    }
    public async UniTask PlayEffect()
    {
        await effectLines[0].DoAction(this);
    }

    public void OverrideSource(Card c)
    {
        effectCardSource = c;
    }
}

public interface ActionsController
{
    public UniTask DoAction(Effect effectSource);
}
public interface ISelectableTarget {}

[Serializable]
public class LineAction : ActionsController
{
    [SerializeReference, SubclassSelector]
    public List<OneGameAction> actions;
    
    public async UniTask DoAction(Effect effectSource)
    {
        for(int i = 0; i < actions.Count; i++)
        {
            if (actions[i].action is IGameAction)
            {
                await actions[i].DoAction(effectSource, this);
            }
            if (actions[i].action is ITargetSelector)
            {
                await actions[i].SetTarget(effectSource.effectCardSource, false);
            }
            if(actions[i].action is ITargetConverter)
            {
                actions[i].ConvertTarget(effectSource, this);
            }
        }
    }
}
[Serializable]
public class LineThrowCube : ActionsController
{
    [SerializeReference, SubclassSelector] public List<LineAction> actions = new List<LineAction>();
    
    public async UniTask DoAction(Effect effectSource)
    {
        int result = await G.Main.CubeThrower.ThrowCube();
        StackUnitCube su = new StackUnitCube(result, false);
        su.AddTriggeredEffect(effectSource);
        await G.Main.StackSystem.PutStackUnit(su);
    }
    public async UniTask DoResultAction(Effect effectSource, int result)
    {
        await actions[result - 1].DoAction(effectSource);
    }
}
[Serializable]
public class LineSelectAction : ActionsController
{
    [SerializeField] public List<string> labels;
    [SerializeReference, SubclassSelector] public List<LineAction> actions = new List<LineAction>();
    
    public async UniTask DoAction(Effect effectSource)
    {
        await actions[effectSource.actionSelectorNum].DoAction(effectSource);
    }
}

[Serializable]
public class OneGameAction
{
    public int targetId; 
    public bool isTargetLocal = false;
    public bool canRefuse;
    [SerializeReference, SubclassSelector]
    public IAction action;
    public async UniTask<bool> DoAction(Effect source, LineAction lineAction)
    {
        if (canRefuse)
        {
            int result = await G.Main.EffectSelector.SelectEffect(source.effectCardSource,
                new List<string>() { "1. Выполнить", "2. Отказаться" });
            if (result == 1) return false;
        }

        List<ISelectableTarget> container = null;
        if (isTargetLocal)
        {
            container = (lineAction.actions[targetId].action as ITargetAction).container;
        }
        else
        {
            container = (source.globalStorage[targetId].action as ITargetAction).container;
        }
        await source.effectCardSource.WaitUntilStop();
        return await (action as IGameAction).Execute(source, container);
    }

    public async UniTask<bool> SetTarget(Card source, bool isCancelable)
    {
        if (action is ITargetSelector actionTargetSelecting)
        {
            if(!await actionTargetSelecting.SetTarget(source, isCancelable)) return false;
        }
        return true;
    }

    public void ConvertTarget(Effect source, LineAction lineAction)
    {
        List<ISelectableTarget> container = null;
        if (isTargetLocal)
        {
            container = (lineAction.actions[targetId].action as ITargetSelector).container;
        }
        else
        {
            container = (source.globalStorage[targetId].action as ITargetSelector).container;
        }
        ((ITargetConverter)action).ConvertTarget(container);
    }
}

public class CardSelector
{
    private List<Card> results;
    private List<Deck> deckResults;
    public async UniTask<T> SelectCardByType<T>(Transform startPlace, bool canExit, Func<Card,bool> cardPridicate, Func<Deck,bool> deckPridicate = null) where T : class, ISelectableTarget
    {
        //Player initiator = G.Players.priorPlayer;
        G.Main.ActionChecker.isSelectingSomething = true;
        //UIOnDeck.inst.ChangeButtonsActive();

        results = G.Main.AllCards.FindAll(x => x.visual.enabled && cardPridicate(x));
        deckResults = deckPridicate != null ? G.Main.Decks.GetNormalDecks().FindAll(d => deckPridicate(d)) : new List<Deck>();
        foreach (var c in results)
        {
            c.RemoveLit();
            c.AddGlow();
            c.AddOutline(new Color(256f/256,125f/256,0));
        }
        foreach (var deck in deckResults)
        {
            deck.SetLit(0f);
            deck.AddGlow();
            deck.AddOutline(new Color(256f/256,125f/256,0));
        }
        GameObject.FindFirstObjectByType<ArrowController>().ActivateArrow(startPlace);
        
        
        while (true)
        {
            await UniTask.Yield();
            if(Input.GetMouseButtonDown(1) && canExit)
            {
                _ = QuitSelecting();
                //Console.WriteText("Действие отменено");
                return null;
            }   
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
                RaycastHit2D hit = hits.FirstOrDefault(x =>
                    x.collider.GetComponent<Card>() || x.collider.GetComponentInParent<Deck>());
                
                if (hit.collider != null)
                {
                    if (CheckCard(hit.collider.GetComponent<Card>(), cardPridicate))
                    {
                        await QuitSelecting();
                        //UIOnDeck.inst.ChangeButtonsActive();
                        return hit.collider.GetComponent<T>();
                    }

                    if (CheckDeck(hit.collider.GetComponentInParent<Deck>(), deckPridicate))
                    {
                        await QuitSelecting();
                        //UIOnDeck.inst.ChangeButtonsActive();
                        return hit.collider.GetComponentInParent<T>();
                    }
                }
            }
        }
    }

    private bool CheckCard(Card c, Func<Card,bool> cardPridicate)
    {
        if (c == null) return false;
        if(cardPridicate == null) return true;
        return cardPridicate(c);
    }

    private bool CheckDeck(Deck d, Func<Deck, bool> deckPridicate)
    {
        if (d == null) return false;
        if(deckPridicate == null) return true;
        return deckPridicate(d);
    }
    private async UniTask QuitSelecting()
    {
        await UniTask.Yield();
        Transform.FindFirstObjectByType<ArrowController>()?.DeactivateArrow();
        foreach (var c in results)
        {
            c.RemoveOutline();
            c.RemoveGlow();
            c.RestoreLit();
        }
        foreach (var deck in deckResults)
        {
            deck.RemoveOutline();
            deck.RemoveGlow();
            deck.SetLit(1);
        }
        
        G.Main.ActionChecker.isSelectingSomething = false;
        //G.Players.RestorePrior();
    }
}