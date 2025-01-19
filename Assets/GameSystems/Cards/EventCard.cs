using UnityEngine;
using DG.Tweening;
public class EventCard : Card
{
    public override void Init(CardData d, bool isFaceUp = true)
    {
        base.Init(d, isFaceUp);
    }
    public async void PlayEvent()
    {
        EventCardData d = GetData<EventCardData>();
        if(d.eventEffect.type == EventEffectType.Curse)
        {
            await d.eventEffect.GiveCurse(this);
            GameMaster.inst.monsterZone.RemoveMonster(this, false);
        }
        else if(d.eventEffect.type == EventEffectType.Play)
        {
            await d.eventEffect.PlayAction();
            GameMaster.inst.monsterZone.RemoveMonster(this);
        }
    }
}
