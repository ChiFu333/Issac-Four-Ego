using UnityEngine;

public class ActionChecker
{
    public bool isDraggingCard = false;
    public bool isSelectingSomething = false;
    public bool isThrowingCube = false;
    public bool isSelectingAction = false;
    public bool isWatchingCards = false;
    public bool Check()
    {
        return !isDraggingCard && !isSelectingSomething && !isThrowingCube && !isSelectingAction && !isWatchingCards;
    }

    public bool CheckForCard(SlotZone zone)
    {
        bool first = Check();
        bool second = zone != null && zone.ignoreActionsCherker;
        return first || second;
    }
}
