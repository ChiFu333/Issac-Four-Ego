using UnityEngine;

public class GameZones : MonoBehaviour
{
    [SerializeField] private BoxCollider2D lootZone;

    public void ChangeActive(bool enable)
    {
        lootZone.gameObject.SetActive(enable);
    }
}
