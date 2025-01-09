using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
public class MonsterCard : Card
{
    [field: SerializeField, HorizontalGroup("HP")] public int hp { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int dodge { get; private set; }
    [field: SerializeField, HorizontalGroup("HP")] public int attack { get; private set; }
    public override void Init(CardData d, bool isFaceUp = true)
    {
        base.Init(d, isFaceUp);
        hp = (data as MonsterCardData).hp;
        dodge = (data as MonsterCardData).dodge;
        attack = (data as MonsterCardData).attack;
    }
    public void Damage(int i)
    {
        hp -= i;

        if(hp <= 0) Die();
    }
    public void Die()
    {
        Console.WriteText("Монстр Убит");
        
        (data as MonsterCardData).reward.result?.Invoke();

        GameMaster.inst.monsterZone.RemoveMonster(this);
    }
}
