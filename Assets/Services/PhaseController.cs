using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class PhaseController
{
    public Phase currentPhase { get; set; } = Phase.Start;
    public int subphases = 0;
    public bool trigger = false;
    public async void StartStartTurn()
    {
        currentPhase = Phase.Start;
        //UIOnDeck.inst.ChangeButtonsActive();
        //TriggersSystem.CleanAll();
        //for(int i = 0; i < G.Players.players.Count; i++) G.Players.players[i].SetPassiveItems();

        #region Step1
        Debug.Log("Перезаряди всё");
        G.Main.TableUI.SetPhaseText("Начало хода: Перезарядка");
        
        G.Main.ActivePlayer = G.Main.Players[0];
        
        RechargeItem rechargeAction = new RechargeItem();
        List<Card> temp = new List<Card>(G.Main.ActivePlayer.Get<TagBasePlayerData>().items);
        temp.Add(G.Main.ActivePlayer.Get<TagBasePlayerData>().characterCard);
        await rechargeAction.Execute(null, temp.PackCards());

        await UniTask.Delay(100);
        #endregion Step1

        #region Step2
        G.Main.TableUI.SetPhaseText("Начало хода: Эффекты");
        //StackSystem.inst.GivePrior();
        await WaitTrigger();
        
        List<IOnTurnStart> onTurnStarts = 
            G.Main.AllCards
                .Where(card => card.visual.enabled && card.Is<TagIsItem>())  
                .SelectMany(card => card.state.OfType<IOnTurnStart>()).ToList();
        foreach (var variable in onTurnStarts)
        {
            await variable.OnTurnStart(G.Main.ActivePlayer);
        }
        #endregion Step2

        #region Step3
        G.Main.TableUI.SetPhaseText("Начало хода: После добычи");

        GainLootCardsAction lootAction = new GainLootCardsAction();
        lootAction.count = G.Main.ActivePlayer.Get<TagBasePlayerData>().lootTakeCount;
        await lootAction.Execute(null, G.Main.ActivePlayer.PackPlayer());
            

        //StackSystem.inst.GivePrior();
        await WaitTrigger();
        #endregion Step3

        StartActionPhase().Forget();
    }
    private async UniTask StartActionPhase()
    {
        currentPhase = Phase.Action;
        G.Main.TableUI.SetPhaseText("Фаза действий!");

        TagBasePlayerData currentPlayer = G.Main.Players[0].Get<TagBasePlayerData>();
        
        currentPlayer.lootPlayCount++;
        currentPlayer.buyCount++;
        currentPlayer.attackCount++;
        
        //UIOnDeck.inst.UpdateTexts();
        //UIOnDeck.inst.ChangeButtonsActive();
    }
    public async UniTask StartEndPhase()
    {
        currentPhase = Phase.End;
        //UIOnDeck.inst.ChangeButtonsActive();
        //EndBuying();
        
        //while(subphases != 0 || StackSystem.inst.stack.Count != 0) await UniTask.Yield();

        #region Step1
        G.Main.TableUI.SetPhaseText("Конец хода: эффекты");
        await UniTask.Delay(1000);
        //UIOnDeck.inst.ChangeButtonsActive();
        List<IOnTurnEnd> onTurnEnds = 
            G.Main.AllCards
                .Where(card => card.visual.enabled && card.Is<TagIsItem>())  
                .SelectMany(card => card.state.OfType<IOnTurnEnd>()).ToList();
        foreach (var variable in onTurnEnds)
        {
            await variable.OnTurnEnd(G.Main.ActivePlayer);
        }

        //StackSystem.inst.GivePrior();
        //while(subphases != 0 || StackSystem.inst.stack.Count != 0 || StackSystem.inst.prioreNow) await UniTask.Yield();
        #endregion Step1

        #region Step2
        
        G.Main.TableUI.SetPhaseText("Конец хода: сброс лута");
        int count = G.Main.ActivePlayer.Get<TagBasePlayerData>().hand.GetCount() - 10;

        if (count > 0)
        {
            DiscardLootCardsAction discardAction = new DiscardLootCardsAction();
            discardAction.count = count;
            await discardAction.Execute(null, G.Main.ActivePlayer.PackPlayer());
        }
        else
        {
            await UniTask.Delay(1000);
        }
        #endregion Step2

        #region Step3
        //Console.WriteText("Передаю ход");
        G.Main.TableUI.SetPhaseText("Конец хода: реальный конец");

        List<IOnRealTurnEnd> temp = 
            G.Main.AllCards
            .Where(card => card.visual.enabled)  
            .SelectMany(card => card.state.OfType<IOnRealTurnEnd>()).ToList();
        foreach (var variable in temp)
        {
            await variable.OnRealTurnEnd(G.Main.ActivePlayer);
        }
        
        await UniTask.Delay(1000);
        //GameMaster.inst.HealEveryone();
        //G.monsterZone.RestoreAllStats();
        
        #endregion Step3

        StartStartTurn();
    }
    private bool isBuying = false;
    public async UniTask StartBuying()
    {
        subphases++;
        isBuying = true;
        Debug.Log("Приоритет перед покупкой");
        //UIOnDeck.inst.ChangeButtonsActive();

        //StackSystem.inst.GivePrior();
        await WaitTrigger();
        if(!isBuying) 
        {
            subphases--;
            //UIOnDeck.inst.ChangeButtonsActive();
            return;
        }
        Debug.Log("Выбери предмет для покупки");
        
        ISelectableTarget t = await G.Main.CardSelector.SelectCardByType<ISelectableTarget>(G.Main.TableUI.buttons[1].transform, true,
            card =>
                card.Is<TagInShop>(),
            deck =>
                deck == G.Main.Decks.treasureDeck
        );
        
        PayCoinsActions GA = new PayCoinsActions();
        GA.count = 10;
        if (t == null || await GA.Execute(null, G.Main.Players[0].PackPlayer()))
        {
            Debug.Log("Покупка успешна!");
            if (t is Card card)
            {
                card.visual.sortingOrder = 1000;
                G.AudioManager.PlaySound(R.Audio.BuyShopSound, 0);
                await G.Main.Players[0].AddItem(card);
                card.visual.sortingOrder = 3;
            }
            if (t is Deck deck)
            {
                card = deck.TakeOneCard(0, true);
                card.visual.sortingOrder = 1000;
                G.AudioManager.PlaySound(R.Audio.TreasureGet, 0);
                await G.Main.Players[0].AddItem(card);
                card.visual.sortingOrder = 3;
            }
        }
        else
        {
            Debug.Log("Недостаточно денег");
        }
        subphases--;
        //UIOnDeck.inst.ChangeButtonsActive();
    }
    /*
    public async UniTask StartFighting()
    {
        subphases++;
        UIOnDeck.inst.UpdateAddInfo();
        UIOnDeck.inst.ChangeButtonsActive();
        
        #region Step1
        Console.WriteText("Приоритет перед атакой");
        StackSystem.inst.GivePrior();
        await TriggersSystem.atAttackDeclaration[0]?.PlayTriggeredEffects()!;
        await TriggersSystem.atAttackDeclaration[1 + G.Players.playerTurnId]?.PlayTriggeredEffects()!;
        while(subphases != 1 || StackSystem.inst.stack.Count != 0 || StackSystem.inst.prioreNow) 
        {
            await UniTask.Yield();
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; UIOnDeck.inst.ChangeButtonsActive(); return; }
        }
        #endregion Step1

        #region Step2
        Console.WriteText("Выбери цель атаки");
        G.monsterZone.currentEnemy = await G.CardSelector.SelectCardByType<CardTypeTag>("MonsterZone");
        Console.WriteText("Атака начата!");
        #endregion Step2

        #region StepRepeat
        while(G.monsterZone.currentEnemy != null && G.monsterZone.currentEnemy.GetTag<Characteristics>().health != 0 && G.Players.activePlayer.characteristics.health != 0)
        {
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; UIOnDeck.inst.ChangeButtonsActive(); return; }
            
            List<StackEffect> additionTrigger = new List<StackEffect>();
            for(int i = 0; i < 6; i++)
            {
                StackEffect eff = null;
                if(i+1 >= G.monsterZone.currentEnemy.GetTag<Characteristics>().dodge)
                {
                    eff = new PrimalStackEffect(PrimalEffect.Damage, G.monsterZone.currentEnemy, G.Players.activePlayer.GetDamageCount(), true);
                }
                else
                {
                    eff = new PrimalStackEffect(PrimalEffect.Damage, G.Players.activePlayer.GetMyCard(), G.monsterZone.currentEnemy.GetTag<Characteristics>().attack, true);
                }
                additionTrigger.Add(eff); 
            }
            G.Players.activePlayer.attackThrowCount += 1;
            await StackSystem.inst.PushCubeEffect(UnityEngine.Random.Range(1, 7), false, true, additionTrigger);
            
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; UIOnDeck.inst.ChangeButtonsActive(); return; }
            while(subphases != 1 || StackSystem.inst.stack.Count != 0) 
            {
                await UniTask.Yield();
                if(currentPhase != Phase.Action) { await EndAttack(); subphases--; UIOnDeck.inst.ChangeButtonsActive(); return; }
            }
            
        }
        #endregion StepRepeat
        
        await EndAttack();
        subphases--;
        UIOnDeck.inst.ChangeButtonsActive();
    }
    */
    /*
    public async UniTask EndAttack()
    {
        StackEffect[] st = StackSystem.inst.stack.ToArray();
        for(int i = st.Length - 1; i >= 0; i--)
        {
            if(st[i] is CubeStackEffect eff && eff.fightCube)
            {
                await eff.RemoveMeFromStack();
            }
            else if(st[i] is PrimalStackEffect pse && pse.type == (int)PrimalEffect.Damage && pse.fightDamage)
            {
                await pse.RemoveMeFromStack();
            }
        }
        G.monsterZone.currentEnemy = null;
        //if(currentPhase == Phase.End) await StartEndPhase();   
    }
    public void EndBuying()
    {
        if(isBuying)
        {
            isBuying = false;
        }
    }
    */
    /*
    public async UniTask StartPlayerDie(Player p)
    {
        Debug.Log("Before:" + subphases);
        subphases += 1;
        Debug.Log("After:" + subphases);
        int tempSubphase = subphases;
        Debug.Log("After2:" + tempSubphase);
        
        p.characteristics.isDead = true;
        if(G.Players.activePlayer == p) await G.monsterZone.EndAttack();
        UIOnDeck.inst.ChangeButtonsActive();
        Console.WriteText("Игрок умер");
        await UniTask.Delay(500);
        int tempStackCount = StackSystem.inst.stack.Count;

        #region Step1
        Console.WriteText("Эффекты перед оплатой");
        
        await TriggersSystem.playerDie[0]?.PlayTriggeredEffects()!;
        await TriggersSystem.playerDie[1 + G.Players.playerTurnId]?.PlayTriggeredEffects()!;
        
        StackSystem.inst.GivePrior();
        
        while(tempSubphase != subphases || StackSystem.inst.stack.Count > tempStackCount) 
        {
            await UniTask.Yield();
        }
        tempStackCount = StackSystem.inst.stack.Count;
        #endregion Step1

        #region Step2
        //здесь эффекты "когда ты умираешь фактически"

        Console.WriteText("Выбери предмет как оплату");
        //удаление своего предмета
        if(p.lootCount != 0)
        {
            Console.WriteText("Сбрось лут");
            G.Players.SetPrior(p);
            await p.DiscardCard(await G.CardSelector.SelectCardByType<PlayFromHand>("MyHand"));
            G.Players.RestorePrior();
        }
        p.AddMoney(-1);
        p.ChangeAllPlayerItemCharge(false);
        #endregion Step2
        
        subphases -= 1;
        if(G.Players.activePlayer != p) return;

        #region Step3
        Console.WriteText("Фаза очистки");
        EndBuying();
        while(subphases != 0 || StackSystem.inst.stack.Count != 0) await UniTask.Yield();
        #endregion Step3
        
        Console.WriteText("Переход в конец хода");
        await UniTask.Delay(500);
        _ = StartEndPhase();
    }
    public async UniTask StartEnemyDie(Entity c)
    {
        subphases += 1;
        int tempSubphase = subphases;
        int tempStackCount = StackSystem.inst.stack.Count;
        //p.isDead = true;
        if(G.monsterZone.currentEnemy == c) await G.monsterZone.EndAttack();
        UIOnDeck.inst.ChangeButtonsActive();
        Console.WriteText("Монстер умер");
        await UniTask.Delay(500);

        #region Step1
        G.monsterZone.RemoveMonster(c);
        await c.PutCardNearHand(G.Players.activePlayer.hand);
        #endregion Step1

        #region Step2
        //Способности до получения награды, особые, должно быть описано
        StackSystem.inst.GivePrior();
        while(tempSubphase != subphases || StackSystem.inst.stack.Count > tempStackCount /*|| StackSystem.inst.prioreNow) await UniTask.Yield();
        tempStackCount = StackSystem.inst.stack.Count;
        #endregion Step2

        #region Step3
        await c.Shake();
        //await StackSystem.inst.PushAndAgree(new CardStackEffect(c.GetData<MonsterCardData>().GetRewardEffect(), c));
        #endregion Step3

        #region Step4
        //Способности после смерти монстра, тут как обычно
        StackSystem.inst.GivePrior();
        while(tempSubphase > subphases || StackSystem.inst.stack.Count > tempStackCount /*|| StackSystem.inst.prioreNow) 
        {
            await UniTask.Yield();
            Debug.Log("1IF: " + (tempSubphase > subphases) + "2IF: " + (StackSystem.inst.stack.Count > tempStackCount));
            Debug.Log("1temp: " + tempSubphase + " 2sub:" + subphases);
        }
        tempStackCount = StackSystem.inst.stack.Count;
        #endregion Step4
        
        await c.DiscardEntity();
        await G.monsterZone.RestockSlots();
        CheckEventsAndPlay();
        UIOnDeck.inst.ChangeButtonsActive();
    }
    public async UniTask StartEventPlay(Entity c, bool smoothGo)
    {
        if(!smoothGo) subphases += 1;
        int tempSubphase = subphases;
        int tempStackCount = StackSystem.inst.stack.Count;
        UIOnDeck.inst.UpdateAddInfo();
        UIOnDeck.inst.ChangeButtonsActive();
        
        //#region Step1
        Console.WriteText("Событие!");
        
        /*CardStackEffect eff = new CardStackEffect(c.GetData<EventCardData>().GetPlayEffect(), c);
        await eff.Init();
        await StackSystem.inst.PushEffect(eff);
        
        StackSystem.inst.GivePrior();
        while(tempSubphase > subphases || StackSystem.inst.stack.Count > tempStackCount || StackSystem.inst.prioreNow) 
        {
            await UniTask.Yield();
            
            Debug.Log("1IF: " + (tempSubphase > subphases) + "2IF: " + (StackSystem.inst.stack.Count > tempStackCount));
            Debug.Log("Temp: " + tempSubphase + " Now: " + subphases);
        }
        //#endregion Step1

        await G.monsterZone.RestockSlots();
        CheckEventsAndPlay();
        UIOnDeck.inst.ChangeButtonsActive();
    }
    private void CheckEventsAndPlay()
    {
        List<Entity> events = G.monsterZone.CheckEvents();
        if(events.Count == 0) subphases -= 1;
        else
        {
            subphases += events.Count - 1; //тут прикол
            _ = StartEventPlay(events[0], true);
        }
    }
    */

    private async UniTask WaitTrigger()
    {
        trigger = false;
        await UniTask.WaitUntil(() => trigger);
    }
}
public enum Phase 
{
    Start, Action, End
}