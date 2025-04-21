using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IStaticBuff : ITag, IRemovable { }

public class AddDamageToBattleThrow : IStaticBuff
{
    public Entity me;
    public int placeToPlusDamage;

    public AddDamageToBattleThrow(int c)
    {
        placeToPlusDamage = c;
    }
    public void Init(Entity entity)
    {
        me = entity;
        me.GetMyPlayer().addDamageToBattleThrow[placeToPlusDamage] += 1;
    }

    public void Remove()
    {
        me.GetMyPlayer().addDamageToBattleThrow[placeToPlusDamage] -= 1;
    }
}
public class PlusOneCoinGain : IStaticBuff
{
    public void Init(Entity entity) { }

    public void Remove() { }
}
public class AddLootPlayThisTurn : IStaticBuff
{
    public Entity me;
    
    public void Init(Entity entity)
    {
        me = entity;
        me.GetMyPlayer().lootPlayMax += 1;
        me.GetMyPlayer().lootPlayCount += 1;
    }

    public void Remove()
    {
        me.GetMyPlayer().lootPlayMax -= 1;
        if(me.GetMyPlayer().lootPlayMax > me.GetMyPlayer().lootPlayCount)
            me.GetMyPlayer().lootPlayCount = me.GetMyPlayer().lootPlayMax;
    }
}
public class HpStatic : IStaticBuff
{
    public Entity me;
    private int count;
    public HpStatic(int c)
    {
        count = c;
    }
    public void Init(Entity entity)
    {
        me = entity;
        me.GetMyPlayer().characteristics.AddHp(count);
    }

    public void Remove()
    {
        me.GetMyPlayer().characteristics.AddHp(count);
    }
}
public class AttackStatic : IStaticBuff
{
    public Entity me;
    private int count;
    public AttackStatic(int c)
    {
        count = c;
    }
    public void Init(Entity entity)
    {
        me = entity;
        me.GetMyPlayer().characteristics.AddAttack(count);
    }

    public void Remove()
    {
        me.GetMyPlayer().characteristics.AddAttack(-count);
    }
}
public class AttackCountStatic : IStaticBuff
{
    public Entity me;
    private int count;
    public AttackCountStatic(int c)
    {
        count = c;
    }
    public void Init(Entity entity)
    {
        me = entity;
        me.GetMyPlayer().attackMax += count;
    }

    public void Remove()
    {
        me.GetMyPlayer().attackMax -= count;
    }
}
public class StartLootTake : IStaticBuff
{
    public Entity me;
    private int count;
    public StartLootTake(int c)
    {
        count = c;
    }
    public void Init(Entity entity)
    {
        me = entity;
        me.GetMyPlayer().lootTakeCount += count;
    }

    public void Remove()
    {
        me.GetMyPlayer().lootTakeCount -= count;
    }
}