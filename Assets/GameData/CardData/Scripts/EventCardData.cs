using UnityEngine;
[CreateAssetMenu(fileName = "New EventCard", menuName = "Cards/EventCards", order = 51)]
public class EventCardData : CardData
{
    [field: SerializeField] public EventEffect eventEffect { get; private set; }
}
