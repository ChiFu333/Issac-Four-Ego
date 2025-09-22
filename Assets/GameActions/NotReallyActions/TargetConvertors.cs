using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

[AddTypeMenu(ActionNames.ConvertingName + "Player->AllHisItems")]
[Serializable]
public class PlayersAllItem : ITargetConverter
{
    public List<ISelectableTarget> container { get; set; }
    public void ConvertTarget(List<ISelectableTarget> container)
    {
        List<Card> playersItems = container.ConvertToPlayer().Get<TagBasePlayerData>().items;
        this.container = playersItems.PackCards();
    }
}