using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Timeline;

public class MonsterZone : MonoBehaviour
{
    [field: SerializeField] public int activeSlotsCount { get; private set; } = 2;
    [field: SerializeField] public List<Entity> monstersInSlots { get; private set; } = new List<Entity>();
    public Entity currentEnemy { get; set; }
    public void Init()
    {
        for(int j = 0; j < activeSlotsCount; j++)
        {
            Entity c = G.Decks.monsterDeck.TakeOneCard();
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
                Entity c = G.Decks.monsterDeck.TakeOneCard();
                monstersInSlots[i] = c;
            }
        }
        bool trigger = false;
        for(int j = 0; j < activeSlotsCount; j++)
        {
            if(j + 1 == activeSlotsCount)
            {
                monstersInSlots[j].MoveTo(G.CardPlaces.monsterSlots[j], transform, () => trigger = true);
            }
            else
            {
                monstersInSlots[j].MoveTo(G.CardPlaces.monsterSlots[j], transform);
            }
        }
        while(!trigger) await Task.Yield();
        RestoreAllStats();
        UIOnDeck.inst.UpdateMonsterUI();
    }
    public List<Entity> CheckEvents()
    {
        List<Entity> events = new List<Entity>();
        foreach(Entity c in monstersInSlots) 
            if(c.GetTag<CardTypeTag>().cardType == CardType.eventCard) 
                events.Add(c); 
        return events;
    }
    public async void StartAttackSubPhase()
    {
        if(G.Players.activePlayer.attackCount <= 0) return;
        G.Players.activePlayer.attackCount -= 1;
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
    public void RemoveMonster(Entity c)
    {
        if(monstersInSlots.IndexOf(c) != -1) monstersInSlots[monstersInSlots.IndexOf(c)] = null;
    }
    public void RestoreAllStats()
    {
        //foreach(Entity c in monstersInSlots) 
        //c.SetBaseStats();
        UIOnDeck.inst.UpdateMonsterUI();
    }
    public async Task StashSlot(Entity c, bool canStashAttackedMonster = false)
    {
        RemoveMonster(c);
        await c.DiscardEntity();
    }
}
