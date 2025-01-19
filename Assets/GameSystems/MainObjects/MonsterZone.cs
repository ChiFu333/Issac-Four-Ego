using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

public class MonsterZone : MonoBehaviour
{
    [field: SerializeField] public int activeSlotsCount { get; private set; } = 2;
    [field: SerializeField] public List<Card> monstersInSlots { get; private set; } = new List<Card>();
    private bool cancelAttackTrigger = false;
    public void Init()
    {
        for(int j = 0; j < activeSlotsCount; j++)
        {
            MonsterCard c = Card.CreateCard<MonsterCard>(GameMaster.inst.monsterDeck.TakeOneCard());
            monstersInSlots.Add(c);
        }

        RestockSlots(); 
    }
    public void RestockSlots()
    {
        for(int i = 0; i < activeSlotsCount; i++)
        {
            if(monstersInSlots[i] == null)
            {
                Card c = null;
                CardData d = GameMaster.inst.monsterDeck.TakeOneCard();
                if(d is MonsterCardData monster)
                {
                    c = Card.CreateCard<MonsterCard>(monster);
                }
                else if(d is EventCardData eventC)
                {
                    c = Card.CreateCard<EventCard>(eventC);
                }
                monstersInSlots[i] = c;
            }
        }
        for(int j = 0; j < activeSlotsCount; j++)
        {
            monstersInSlots[j].MoveTo(CardPlaces.inst.monsterSlots[j], transform);
        }
        UIOnDeck.inst.UpdateMonsterUI();
        foreach(Card c in monstersInSlots) if(c is EventCard eventC) eventC.PlayEvent();
    }
    public async void Attack()
    {
        if(GameMaster.inst.turnManager.activePlayer.attackCount <= 0) return;
        GameMaster.inst.turnManager.activePlayer.attackCount--;
        UIOnDeck.inst.UpdateAddInfo();
        Console.WriteText("Выбери цель атаки");
        MonsterCard c = await SubSystems.inst.SelectCardByType<MonsterCard>("MonsterZone");
        if(c == null) return;

        Console.WriteText("Атака начата");
        
        while(true)
        {
            if(cancelAttackTrigger)
            {
                cancelAttackTrigger = false;
                UIOnDeck.inst.UpdateCubeUI(0);
                break;
            }
            int cube = await CubeManager.inst.ThrowDice();
            if(cube >= c.dodge)
            {
                Console.WriteText("Попал по монстру!");
                if(c.hp + c.preventHp - GameMaster.inst.turnManager.activePlayer.attack <= 0) 
                {
                    c.Damage(GameMaster.inst.turnManager.activePlayer.attack);
                    break;
                }
                else
                    c.Damage(GameMaster.inst.turnManager.activePlayer.attack);
            }
            else
            {
                Console.WriteText("Монстр бьёт!");
                
                if(GameMaster.inst.turnManager.activePlayer.hp + GameMaster.inst.turnManager.activePlayer.preventHp - c.attack <= 0) 
                {
                    GameMaster.inst.turnManager.activePlayer.Damage(c.attack);
                    break;
                }
                else
                    GameMaster.inst.turnManager.activePlayer.Damage(c.attack);
            }
            UIOnDeck.inst.UpdateTexts();
            UIOnDeck.inst.UpdateMonsterUI();
        }
    }
    public void CancelAttack()
    {
        cancelAttackTrigger = true;
    }
    public void RemoveMonster(Card c, bool ToStash = true)
    {
        monstersInSlots[monstersInSlots.IndexOf(c)] = null;
        RestockSlots();
        if(ToStash) c.MoveTo(CardPlaces.inst.monsterStash, null, () => GameMaster.inst.monsterStash.PutOneCardUp(c));
    }
    public void RestoreAllStats()
    {
        foreach(MonsterCard c in monstersInSlots) c.SetBaseStats();
        UIOnDeck.inst.UpdateMonsterUI();
    }
}
