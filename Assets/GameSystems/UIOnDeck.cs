using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIOnDeck : MonoBehaviour
{
    public static UIOnDeck inst { get; private set; }
    [field: SerializeField] public List<TMP_Text> playerText { get; private set; }
    [field: SerializeField] public TMP_Text monsterText { get; private set; }
    [field: SerializeField] public TMP_Text shopText { get; private set; }
    [field: SerializeField] public List<TMP_Text> monstersHpCounters { get; private set; }
    [field: SerializeField] public TMP_Text cubeText { get; private set; }
    public void Awake()
    {
        inst = this;
        UpdateCubeUI(0);
    }

    public void UpdateTexts()
    {
        List<Player> players = GameMaster.inst.turnManager.players;
        for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
        {
            playerText[i].text = "хп: " + players[i].hp + "    сила: " + players[i].attack + "    кеш: " + players[i].coins + "¢    карт лута: "+ players[i].lootCount +"    души: "+ players[i].souls;
        }
        shopText.text = "Цена: " + GameMaster.inst.turnManager.activePlayer.shopPrice + "¢    кол-во покупок: " + GameMaster.inst.turnManager.activePlayer.buyCount;
    }
    public void UpdateMonsterUI()
    {
        for(int i = 0; i < GameMaster.inst.monsterZone.monstersInSlots.Count; i++)
        {
            monstersHpCounters[i].text = "HP: " + (GameMaster.inst.monsterZone.monstersInSlots[i] as MonsterCard)?.hp;
        }
        for(int i = GameMaster.inst.monsterZone.monstersInSlots.Count; i < 4; i++)
        {
            monstersHpCounters[i].text = "";
        }
    }
    public void UpdateCubeUI(int count)
    {
        cubeText.text = count != 0 ? count.ToString() : "";
    }
}
