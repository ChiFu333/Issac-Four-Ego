using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MonsterZone : MonoBehaviour
{
    [field: SerializeField] public int activeSlotsCount { get; private set; } = 2;
    [field: SerializeField] public List<Card> monstersInSlots { get; private set; } = new List<Card>();
    public void Init()
    {
        for(int j = 0; j < activeSlotsCount; j++)
        {
            MonsterCard c = Card.CreateCard<MonsterCard>(TakeOneMonster());
            c.transform.parent = transform;
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
                MonsterCard c = Card.CreateCard<MonsterCard>(TakeOneMonster());
                c.transform.parent = transform;
                monstersInSlots[i] = c;
            }
        }

        for(int i = 0; i < monstersInSlots.Count; i++)
        {
            monstersInSlots[i].transform.DOMove(CardPlaces.inst.monsterSlots[i].position,GameMaster.CARDSPEED);
        }
        UIOnDeck.inst.UpdateMonsterUI();
    }
    private MonsterCardData TakeOneMonster()
    {
        CardData d = GameMaster.inst.monsterDeck.TakeOneCard();
        while(true)
        {
            if(d is MonsterCardData data)
            {
                return data;
            }
            else
            {
                GameMaster.inst.monsterStash.PutOneCardUp(d);
            }
        }
    }
    public async void Attack()
    {
        if(GameMaster.inst.turnManager.activePlayer.attackCount <= 0) return;

        MonsterCard c = await SubSystems.Inst.SelectCardByType<MonsterCard>("MonsterZone");
        if(c == null) return;

        while(true)
        {
            int cube = await CubeManager.Inst.ThrowDice();
            if(cube >= c.dodge)
            {
                c.Damage(GameMaster.inst.turnManager.activePlayer.attack);
                if(c.hp <= 0) break;
            }
            else
            {
                GameMaster.inst.turnManager.activePlayer.Damage(c.attack);
                if(GameMaster.inst.turnManager.activePlayer.hp <= 0) break;
            }
            UIOnDeck.inst.UpdateTexts();
            UIOnDeck.inst.UpdateMonsterUI();
        }
    }
    public void RemoveMonster(MonsterCard c)
    {
        monstersInSlots[monstersInSlots.IndexOf(c)] = null;
        transform.DOMove(CardPlaces.inst.monsterStash.position, GameMaster.CARDSPEED).onComplete = () => 
        {
            GameMaster.inst.monsterStash.PutOneCardUp(c);

            RestockSlots();
        };
    }
}
