using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
public class MonsterCard : Card
{
    [field: SerializeField, HorizontalGroup("HP")] public int hp { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int dodge { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int attack { get; private set; }
    [field: SerializeField] public int preventHp { get; private set; }
    public int HpMax
    { 
        get => hpMax;
        set
        {
            int delta = value - hpMax;
            hpMax += delta;
            if(delta > 0)
            {
                hp += delta;
            }
            else if(delta < 0)
            {
                hp = hpMax < hp ? hpMax : hp;
            }
            UIOnDeck.inst.UpdateMonsterUI();
        }
    }
    private int hpMax;
    public override void Init(CardData d, bool isFaceUp = true)
    {
        base.Init(d, isFaceUp);
        SetBaseStats();
    }
    public void Damage(int count)
    {
        int damageCount = count;
        if(hp == 0) return;
        
        if(damageCount > preventHp)
        {
            damageCount -= preventHp;
            preventHp = 0;
            hp -= damageCount;
        }
        else
        {
            preventHp -= damageCount;
        }

        if(hp <= 0) 
        {
            hp = 0;
            Die();
        }
    }
    public void AddHp(int count) => hp += count;
    public void ChangePreventHp(int count)
    {
        preventHp += count;
    }
    public void HealHp(int count)
    {
        if(hp + count > HpMax)
            hp = HpMax;
        else
            hp += count;
        UIOnDeck.inst.UpdateMonsterUI();
    }
    public void AddAttack(int count) => attack += count;
    public async void Die()
    {
        //GameMaster.inst.monsterZone.CancelAttack();
        Console.WriteText("Монстр Убит");

        await GetData<MonsterCardData>().reward.PlayActions();

        GameMaster.inst.monsterZone.RemoveMonster(this);
    }
    public void SetBaseStats()
    {
        preventHp = 0;
        HpMax = GetData<MonsterCardData>().hp;
        HealHp(HpMax);
        dodge = GetData<MonsterCardData>().dodge;
        attack = GetData<MonsterCardData>().attack;
    }
}
