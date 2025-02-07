using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
public class EventCard : Card
{
    public bool isCurse = false;
    private bool curseTrigger = false;
    public override void Init(CardData d, bool isFaceUp = true)
    {
        base.Init(d, isFaceUp);
    }
    public override async Task DiscardCard()
    {
        if(!curseTrigger)
        {
            GameMaster.inst.monsterZone.RemoveMonster(this);
            await DiscardCard<EventCard>();
        } 
        else
        {
            curseTrigger = false;
        }
    }
    public void TurnIntoCurse()
    {
        isCurse = true;
        //curseTrigger = true;
    }
}
