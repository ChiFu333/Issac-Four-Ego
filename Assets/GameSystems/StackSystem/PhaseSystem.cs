using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class PhaseSystem : MonoBehaviour
{
    public Phase currentPhase { get; set; } = Phase.Start;
    [field: SerializeField] public int subphases = 0;
    public async void StartStartTurn()
    {
        currentPhase = Phase.Start;
        UIOnDeck.inst.ChangeButtonsActive();

        TriggersSystem.CleanAll();
        for(int i = 0; i < GameMaster.PLAYERCOUNT; i++) GameMaster.inst.turnManager.players[i].SetPassiveItems();

        #region Step1
        Console.WriteText("Перезаряди всё");
        UIOnDeck.inst.UpdatePhase("Фаза: начало хода (шаг 1)");
        await Task.Delay(500);
        GameMaster.inst.turnManager.activePlayer.ChangeAllPlayerItemCharge(true);
        await Task.Delay(1000);
        #endregion Step1

        #region Step2
        Console.WriteText("Эффекты начала хода");
        UIOnDeck.inst.UpdatePhase("Фаза: начало хода (шаг 2)");
        await TriggersSystem.withStartTurn[0]?.PlayTriggeredEffects();
        await TriggersSystem.withStartTurn[1 + GameMaster.inst.turnManager.id]?.PlayTriggeredEffects();
        
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
        GameMaster.inst.turnManager.activePlayer.TakeOneLootCard(Card.CreateCard<LootCard>(GameMaster.inst.lootDeck.TakeOneCard(),true));

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
        GameMaster.inst.turnManager.activePlayer.lootPlayCount++;
        GameMaster.inst.turnManager.activePlayer.buyCount++;
        GameMaster.inst.turnManager.activePlayer.attackCount++;
        UIOnDeck.inst.UpdateTexts();
        UIOnDeck.inst.ChangeButtonsActive();
    }
    public async Task StartEndPhase()
    {
        currentPhase = Phase.End;
        UIOnDeck.inst.ChangeButtonsActive();
        UIOnDeck.inst.UpdatePhase("Фаза: конец хода");
        
        while(subphases != 0 || StackSystem.inst.stack.Count != 0) await Task.Yield();

        #region Step1
        UIOnDeck.inst.UpdatePhase("Фаза: конец хода (шаг 1)");
        Console.WriteText("Эффекты конца хода");
        UIOnDeck.inst.ChangeButtonsActive();
        await TriggersSystem.withEndTurn[0]?.PlayTriggeredEffects();
        await TriggersSystem.withEndTurn[1 + GameMaster.inst.turnManager.GetMyId(GameMaster.inst.turnManager.activePlayer)]?.PlayTriggeredEffects();

        StackSystem.inst.GivePrior();
        while(subphases != 0 || StackSystem.inst.stack.Count != 0 || StackSystem.inst.prioreNow) await Task.Yield();
        #endregion Step1

        #region Step2
        UIOnDeck.inst.UpdatePhase("Фаза: конец хода (шаг 2)");
        for(int i = 0; i < GameMaster.inst.turnManager.activePlayer.lootCount - 10; i++) 
        {
            Console.WriteText("Сбрось до 10 карт лута");
            LootCard c = await SubSystems.inst.SelectCardByType<LootCard>("MyHand"); 
            GameMaster.inst.turnManager.activePlayer.DiscardCard(c);
        }
        #endregion Step2

        #region Step3
        Console.WriteText("Передаю ход");
        UIOnDeck.inst.UpdatePhase("Фаза: конец хода (шаг 3)");
        await Task.Delay(500);
        GameMaster.inst.turnManager.HealEveryone();
        GameMaster.inst.monsterZone.RestoreAllStats();
        
        #endregion Step3

        GameMaster.inst.turnManager.SwitchTurn();
    }
    public async Task StartPlayerDie(Player p)
    {
        Debug.Log("Before:" + subphases);
        subphases += 1;
        Debug.Log("After:" + subphases);
        int tempSubphase = subphases;
        Debug.Log("After2:" + tempSubphase);
        
        p.isDead = true;
        if(GameMaster.inst.turnManager.activePlayer == p) await GameMaster.inst.monsterZone.EndAttack();
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
            Debug.Log("Iff: " + (tempSubphase));
            Debug.Log("Ifs: " + (subphases));
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
            GameMaster.inst.turnManager.SetPrior(p);
            p.DiscardCard(await SubSystems.inst.SelectCardByType<LootCard>("MyHand"));
            GameMaster.inst.turnManager.RestorePrior();
        }
        p.AddMoney(-1);
        p.characterCard.Flip();
        p.ChangeAllPlayerItemCharge(false);
        #endregion Step2
        
        subphases -= 1;
        if(GameMaster.inst.turnManager.activePlayer != p) return;

        #region Step3
        Console.WriteText("Фаза очистки");
        EndBuying();
        while(subphases != 0 || StackSystem.inst.stack.Count != 0) await Task.Yield();
        #endregion Step3
        
        Console.WriteText("Переход в конец хода");
        await Task.Delay(500);
        _ = StartEndPhase();
    }
    public async Task StartEnemyDie(MonsterCard c)
    {
        subphases++;
        int tempSubphase = subphases;
        int tempStackCount = StackSystem.inst.stack.Count;
        //p.isDead = true;
        if(GameMaster.inst.monsterZone.currentEnemy == c) await GameMaster.inst.monsterZone.EndAttack();
        UIOnDeck.inst.ChangeButtonsActive();
        Console.WriteText("Монстер умер");
        await Task.Delay(500);

        #region Step1
        await GameMaster.inst.monsterZone.RemoveMonster(c);
        #endregion Step1

        #region Step2
        //Способности до получения награды, особые, должно быть описано
        StackSystem.inst.GivePrior();
        while(tempSubphase != subphases || StackSystem.inst.stack.Count > tempStackCount /*|| StackSystem.inst.prioreNow*/) await Task.Yield();
        tempStackCount = StackSystem.inst.stack.Count;
        #endregion Step2

        #region Step3
        await StackSystem.inst.PushAndAgree(new CardStackEffect(c.GetData<MonsterCardData>().GetRewardEffect(), c));
        #endregion Step3

        #region Step4
        //Способности после смерти монстра, тут как обычно
        StackSystem.inst.GivePrior();
        while(tempSubphase != subphases || StackSystem.inst.stack.Count > tempStackCount /*|| StackSystem.inst.prioreNow*/) await Task.Yield();
        tempStackCount = StackSystem.inst.stack.Count;
        #endregion Step4
        
        await Task.Delay(500);

        bool trigger = false;
        c.MoveTo(CardPlaces.inst.monsterStash, null, () => 
        {
            GameMaster.inst.monsterStash.PutOneCardUp(c);
            trigger = true;
        });
        while(!trigger) await Task.Yield();
        GameMaster.inst.monsterZone.RestockSlots();
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
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; return; }
        }
        #endregion Step1

        #region Step2
        Console.WriteText("Выбери цель атаки");
        GameMaster.inst.monsterZone.currentEnemy = await SubSystems.inst.SelectCardByType<MonsterCard>("MonsterZone");
        Console.WriteText("Атака начата!");
        #endregion Step2

        #region StepRepeat
        while(GameMaster.inst.monsterZone.currentEnemy != null && GameMaster.inst.monsterZone.currentEnemy.hp != 0 && GameMaster.inst.turnManager.activePlayer.hp != 0)
        {
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; return; }
            
            List<StackEffect> additionTrigger = new List<StackEffect>();
            for(int i = 0; i < 6; i++)
            {
                StackEffect eff = null;
                if(i+1 >= GameMaster.inst.monsterZone.currentEnemy.dodge)
                {
                    eff = new PrimalStackEffect(PrimalEffect.Damage, GameMaster.inst.monsterZone.currentEnemy, GameMaster.inst.turnManager.activePlayer.attack, true);
                }
                else
                {
                    eff = new PrimalStackEffect(PrimalEffect.Damage, GameMaster.inst.turnManager.activePlayer.GetMyCard(), GameMaster.inst.monsterZone.currentEnemy.attack, true);
                }
                additionTrigger.Add(eff); 
            }
            
            StackSystem.inst.PushCubeEffect(CubeManager.inst.ThrowDice(), false, true, additionTrigger);
            
            if(currentPhase != Phase.Action) { await EndAttack(); subphases--; return; }
            while(subphases != 1 || StackSystem.inst.stack.Count != 0) 
            {
                await Task.Yield();
                if(currentPhase != Phase.Action) { await EndAttack(); subphases--; return; }
            }
        }
        #endregion StepRepeat
        
        await EndAttack();
        subphases--;
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
        GameMaster.inst.monsterZone.currentEnemy = null;
        UIOnDeck.inst.ChangeButtonsActive();
        //if(currentPhase == Phase.End) await StartEndPhase();   
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
        Card c = await SubSystems.inst.SelectCardByType<ItemCard>("Shop");
        //здесь эффекты после выбора цели
        if(GameMaster.inst.turnManager.activePlayer.PermitBuy())
        {
            UIOnDeck.inst.UpdateAddInfo();
            GameMaster.inst.shop.InstBuy(c);
        }
        else
        {
            Console.WriteText("Нехватает денег");
        }
        subphases--;
        UIOnDeck.inst.ChangeButtonsActive();
    }
    public void EndBuying()
    {
        if(isBuying)
        {
            isBuying = false;
        }
    }
}

