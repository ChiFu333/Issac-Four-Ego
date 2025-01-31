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
        RestockSlots();
    }
    public async Task RemoveMonster(Card c)
    {
        monstersInSlots[monstersInSlots.IndexOf(c)] = null;
        bool trigger = false;
        c.MoveTo(GameMaster.inst.turnManager.activePlayer.hand.transform.TransformPoint(new Vector3(0, Hand.UPMOVE * 3f)), null, () => 
        {
            trigger = true;
        });
        while(!trigger) await Task.Yield();
    }
    public void RestoreAllStats()
    {
        foreach(MonsterCard c in monstersInSlots) c.SetBaseStats();
        UIOnDeck.inst.UpdateMonsterUI();
    }
    public async Task StashSlot(Card c, bool canStashAttackedMonster = false)
    {
        await RemoveMonster(c);
    }
}
