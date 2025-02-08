using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Timeline;

public class MonsterZone : MonoBehaviour
{
    [field: SerializeField] public int activeSlotsCount { get; private set; } = 2;
    [field: SerializeField] public List<Card> monstersInSlots { get; private set; } = new List<Card>();
    public MonsterCard currentEnemy { get; set; }
    public void Init()
    {
        for(int j = 0; j < activeSlotsCount; j++)
        {
            MonsterCard c = (MonsterCard)GameMaster.inst.monsterDeck.TakeOneCard();
            monstersInSlots.Add(c);
        }
        _ = RestockSlots(); 
    }
    public async Task RestockSlots()
    {
        for(int i = 0; i < activeSlotsCount; i++)
        {
            if(monstersInSlots[i] == null)
            {
                Card c = GameMaster.inst.monsterDeck.TakeOneCard();
                monstersInSlots[i] = c;
            }
        }
        bool trigger = false;
        for(int j = 0; j < activeSlotsCount; j++)
        {
            if(j + 1 == activeSlotsCount)
            {
                monstersInSlots[j].MoveTo(CardPlaces.inst.monsterSlots[j], transform, () => trigger = true);
            }
            else
            {
                monstersInSlots[j].MoveTo(CardPlaces.inst.monsterSlots[j], transform);
            }
        }
        while(!trigger) await Task.Yield();
        RestoreAllStats();
        UIOnDeck.inst.UpdateMonsterUI();
    }
    public List<EventCard> CheckEvents()
    {
        List<EventCard> events = new List<EventCard>();
        foreach(Card c in monstersInSlots) if(c is EventCard eventC) events.Add(eventC); 
        return events;
    }
    public async void StartAttackSubPhase()
    {
        if(GameMaster.inst.turnManager.activePlayer.attackCount <= 0) return;
        GameMaster.inst.turnManager.activePlayer.attackCount -= 1;
        await GameMaster.inst.phaseSystem.StartFighting();
    }
    public async Task EndAttack()
    {
        await GameMaster.inst.phaseSystem.EndAttack();
    }
    public void IncreaseZone(int count)
    {
        activeSlotsCount = math.min(activeSlotsCount + count, 4); 
        _ = RestockSlots();
    }
    public void RemoveMonster(Card c)
    {
        if(monstersInSlots.IndexOf(c) != -1) monstersInSlots[monstersInSlots.IndexOf(c)] = null;
    }
    public void RestoreAllStats()
    {
        foreach(MonsterCard c in monstersInSlots) c.SetBaseStats();
        UIOnDeck.inst.UpdateMonsterUI();
    }
    public async Task StashSlot(Card c, bool canStashAttackedMonster = false)
    {
        RemoveMonster(c);
        await c.DiscardCard();
    }
}
