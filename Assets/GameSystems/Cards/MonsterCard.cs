using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;
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
        hpMax = GetData<MonsterCardData>().hp;
        dodge = GetData<MonsterCardData>().dodge;
        attack = GetData<MonsterCardData>().attack;
        //SetBaseStats();
    }
    public async Task Damage(int count)
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
            //await StackSystem.inst.PushPrimalEffect(PrimalEffect.Kill, this);
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
    public async Task StartMonsterDieSubphase()
    {
        //await GameMaster.inst.phaseSystem.StartEnemyDie(this);
    }
    public void SetBaseStats()
    {
        preventHp = 0;
        HpMax = GetData<MonsterCardData>().hp;
        HealHp(100);
        dodge = GetData<MonsterCardData>().dodge;
        attack = GetData<MonsterCardData>().attack;
    }
    public override async Task DiscardCard()
    {
        await DiscardCard<MonsterCard>();
    }
}
