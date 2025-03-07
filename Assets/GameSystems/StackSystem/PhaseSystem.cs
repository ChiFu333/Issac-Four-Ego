using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;

public class PhaseSystem : MonoBehaviour
{
    public Phase currentPhase { get; set; } = Phase.Start;
    [field: SerializeField] public int subphases = 0;
    public async void StartStartTurn()
    {
        currentPhase = Phase.Start;
        UIOnDeck.inst.ChangeButtonsActive();

        TriggersSystem.CleanAll();
        for(int i = 0; i < GameMaster.PLAYERCOUNT; i++) G.Players.players[i].SetPassiveItems();

        #region Step1
        Console.WriteText("Перезаряди всё");
        UIOnDeck.inst.UpdatePhase("Фаза: начало хода (шаг 1)");
        await Task.Delay(500);
        G.Players.activePlayer.ChangeAllPlayerItemCharge(true);
        await Task.Delay(500);
        #endregion Step1

        #region Step2
        Console.WriteText("Эффекты начала хода");
        UIOnDeck.inst.UpdatePhase("Фаза: начало хода (шаг 2)");
        await TriggersSystem.withStartTurn[0]?.PlayTriggeredEffects();
        await TriggersSystem.withStartTurn[1 + G.Players.playerTurnId]?.PlayTriggeredEffects();
        
        StackSystem.inst.GivePrior();
        while(subphases != 0 || StackSystem.inst.stack.Count != 0 || StackSystem.inst.prioreNow) 
        {
            await Task.Yield();
            if(currentPhase != Phase.Start) return;
        }
        #endregion Step2

        #region Step3
        Console.WriteText("Эффекты после взятия лута");
        UIOnDeck.inst.UpdatePhase("Фаза: начало хода (шаг 3)");
        G.Players.activePlayer.TakeOneLootCard(G.Decks.lootDeck.TakeOneCard());

        StackSystem.inst.GivePrior();
        while(subphases != 0 || StackSystem.inst.stack.Count != 0 || StackSystem.inst.prioreNow) 
        {
            await Task.Yield();
            if(currentPhase != Phase.Start) return;
        }
        #endregion Step3

        StartActionPhase();
    }
    private void StartActionPhase()
    {
        currentPhase = Phase.Action;

        Console.WriteText("Фаза действий!");
        UIOnDeck.inst.UpdatePhase("Фаза: действия");
        G.Players.activePlayer.lootPlayCount++;
        G.Players.activePlayer.buyCount++;
        G.Players.activePlayer.attackCount++;
        UIOnDeck.inst.UpdateTexts();
        UIOnDeck.inst.ChangeButtonsActive();
    }
    public async Task StartEndPhase()
    {
        currentPhase = Phase.End;
        UIOnDeck.inst.ChangeButtonsActive();
        EndBuying();
        UIOnDeck.inst.UpdatePhase("Фаза: конец хода");
        
        while(subphases != 0 || StackSystem.inst.stack.Count != 0) await Task.Yield();

        #region Step1
        UIOnDeck.inst.UpdatePhase("Фаза: конец хода (шаг 1)");
        Console.WriteText("Эффекты конца хода");
        UIOnDeck.inst.ChangeButtonsActive();
        await TriggersSystem.withEndTurn[0]?.PlayTriggeredEffects();
        await TriggersSystem.withEndTurn[1 + G.Players.GetPlayerId(G.Players.activePlayer)]?.PlayTriggeredEffects();

        StackSystem.inst.GivePrior();
        while(subphases != 0 || StackSystem.inst.stack.Count != 0 || StackSystem.inst.prioreNow) await Task.Yield();
        #endregion Step1

        #region Step2
        UIOnDeck.inst.UpdatePhase("Фаза: конец хода (шаг 2)");
        int count = G.Players.activePlayer.lootCount - 10;
        for(int i = 0; i < count; i++) 
        {
            Console.WriteText("Сбрось до 10 карт лута");
            Entity c = await SubSystems.inst.SelectCardByType("MyHand"); 
            await G.Players.activePlayer.DiscardCard(c);
        }
        #endregion Step2

        #region Step3
        Console.WriteText("Передаю ход");
        UIOnDeck.inst.UpdatePhase("Фаза: конец хода (шаг 3)");
        await Task.Delay(500);
        GameMaster.inst.HealEveryone();
        G.monsterZone.RestoreAllStats();
        
        #endregion Step3

        GameMaster.inst.SwitchTurn();
    }
    private bool isBuying = false;
    public async Task StartBuying()
    {
        subphases++;
        isBuying = true;
        Console.WriteText("Приоритет перед покупкой");
        UIOnDeck.inst.ChangeButtonsActive();

        StackSystem.inst.GivePrior();
        while(subphases != 1 || StackSystem.inst.stack.Count != 0 || StackSystem.inst.prioreNow) 
        {
            await Task.Yield();
            if(!isBuying) 
            {
                subphases--;
                UIOnDeck.inst.ChangeButtonsActive();
                return;
            }
        }
        if(!isBuying) 
        {
            subphases--;
            UIOnDeck.inst.ChangeButtonsActive();
            return;
        }
        Console.WriteText("Выбери предмет на покупку");        
        Entity c = await SubSystems.inst.SelectCardByType("Shop");
        //здесь эффекты после выбора цели
        if(G.Players.activePlayer.PermitBuy())
        {
            UIOnDeck.inst.UpdateAddInfo();
            G.shop.InstBuy(c);
        }
        else
        {
            Console.WriteText("Нехватает денег");
        }
        subphases--;
        UIOnDeck.inst.ChangeButtonsActive();
    }
    public async Task StartFighting()
    {
        subphases++;
        UIOnDeck.inst.UpdateAddInfo();
        UIOnDeck.inst.ChangeButtonsActive();
        
        #region Step1
        Console.WriteText("Приоритет перед атакой");
        StackSystem.inst.GivePrior();
        while(subphases != 1 || StackSystem.inst.stack.Count != 0 || StackSystem.inst.prioreNow) 
        {
            await Task.Yield();
            Debug.Log("Halo");
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; UIOnDeck.inst.ChangeButtonsActive(); return; }
        }
        #endregion Step1

        #region Step2
        Console.WriteText("Выбери цель атаки");
        G.monsterZone.currentEnemy = await SubSystems.inst.SelectCardByType("MonsterZone");
        Console.WriteText("Атака начата!");
        #endregion Step2

        #region StepRepeat
        while(G.monsterZone.currentEnemy != null && G.monsterZone.currentEnemy.GetTag<Characteristics>().health != 0 && G.Players.activePlayer.hp != 0)
        {
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; UIOnDeck.inst.ChangeButtonsActive(); return; }
            
            List<StackEffect> additionTrigger = new List<StackEffect>();
            for(int i = 0; i < 6; i++)
            {
                StackEffect eff = null;
                if(i+1 >= G.monsterZone.currentEnemy.GetTag<Characteristics>().dodge)
                {
                    eff = new PrimalStackEffect(PrimalEffect.Damage, G.monsterZone.currentEnemy, G.Players.activePlayer.attack, true);
                }
                else
                {
                    eff = new PrimalStackEffect(PrimalEffect.Damage, G.Players.activePlayer.GetMyCard(), G.monsterZone.currentEnemy.GetTag<Characteristics>().attack, true);
                }
                additionTrigger.Add(eff); 
            }
            
            await StackSystem.inst.PushCubeEffect(UnityEngine.Random.Range(1, 7), false, true, additionTrigger);
            
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; UIOnDeck.inst.ChangeButtonsActive(); return; }
            while(subphases != 1 || StackSystem.inst.stack.Count != 0) 
            {
                await Task.Yield();
                if(currentPhase != Phase.Action) { await EndAttack(); subphases--; UIOnDeck.inst.ChangeButtonsActive(); return; }
            }
        }
        #endregion StepRepeat
        
        await EndAttack();
        subphases--;
        UIOnDeck.inst.ChangeButtonsActive();
    }
    public async Task EndAttack()
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
    public async Task StartPlayerDie(Player p)
    {
        Debug.Log("Before:" + subphases);
        subphases += 1;
        Debug.Log("After:" + subphases);
        int tempSubphase = subphases;
        Debug.Log("After2:" + tempSubphase);
        
        p.isDead = true;
        if(G.Players.activePlayer == p) await G.monsterZone.EndAttack();
        UIOnDeck.inst.ChangeButtonsActive();
        Console.WriteText("Игрок умер");
        await Task.Delay(500);
        int tempStackCount = StackSystem.inst.stack.Count;

        #region Step1
        Console.WriteText("Эффекты перед смертью");
        //здесь положить эффекты когда кто-то "должен" умереть
        StackSystem.inst.GivePrior();
        
        while(tempSubphase != subphases || StackSystem.inst.stack.Count > tempStackCount) 
        {
            await Task.Yield();
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
            await p.DiscardCard(await SubSystems.inst.SelectCardByType("MyHand"));
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
        while(subphases != 0 || StackSystem.inst.stack.Count != 0) await Task.Yield();
        #endregion Step3
        
        Console.WriteText("Переход в конец хода");
        await Task.Delay(500);
        _ = StartEndPhase();
    }
    public async Task StartEnemyDie(Entity c)
    {
        subphases += 1;
        int tempSubphase = subphases;
        int tempStackCount = StackSystem.inst.stack.Count;
        //p.isDead = true;
        if(G.monsterZone.currentEnemy == c) await G.monsterZone.EndAttack();
        UIOnDeck.inst.ChangeButtonsActive();
        Console.WriteText("Монстер умер");
        await Task.Delay(500);

        #region Step1
        G.monsterZone.RemoveMonster(c);
        await c.PutCardNearHand(G.Players.activePlayer.hand);
        #endregion Step1

        #region Step2
        //Способности до получения награды, особые, должно быть описано
        StackSystem.inst.GivePrior();
        while(tempSubphase != subphases || StackSystem.inst.stack.Count > tempStackCount /*|| StackSystem.inst.prioreNow*/) await Task.Yield();
        tempStackCount = StackSystem.inst.stack.Count;
        #endregion Step2

        #region Step3
        await c.Shake();
        //await StackSystem.inst.PushAndAgree(new CardStackEffect(c.GetData<MonsterCardData>().GetRewardEffect(), c));
        #endregion Step3

        #region Step4
        //Способности после смерти монстра, тут как обычно
        StackSystem.inst.GivePrior();
        while(tempSubphase > subphases || StackSystem.inst.stack.Count > tempStackCount /*|| StackSystem.inst.prioreNow*/) 
        {
            await Task.Yield();
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
    public async Task StartEventPlay(Entity c, bool smoothGo)
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
        
        StackSystem.inst.GivePrior();*/
        while(tempSubphase > subphases || StackSystem.inst.stack.Count > tempStackCount /*|| StackSystem.inst.prioreNow*/) 
        {
            await Task.Yield();
            
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
}
public enum Phase 
{
    Start, Action, End
}