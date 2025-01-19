using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIOnDeck : MonoBehaviour
{
    public static UIOnDeck inst { get; private set; }
    [field: SerializeField] public List<TMP_Text> playerText { get; private set; }
    [field: SerializeField] public List<TMP_Text> monstersHpCounters { get; private set; }
    [field: SerializeField] public TMP_Text cubeText { get; private set; }
    [field: SerializeField] public TMP_Text attackText { get; private set; }
    [field: SerializeField] public TMP_Text shopText { get; private set; }
    public void Awake()
    {
        inst = this;
        UpdateCubeUI(0);
    }

    public void UpdateTexts()
    {
        List<Player> players = GameMaster.inst.turnManager.players;
        for(int i = 0; i < players.Count; i++)
        {
            string hp = "<color=#E34444>☻: " + players[i].hp;
            string addHp = players[i].preventHp == 0 ? "" : "<color=#94EEEE>("+players[i].preventHp+")</color>";
            string endHp = "/" + players[i].HpMax+ "</color>";

            string power = players[i].preventHp == 0 ? "  <color=#969696>♥: " + players[i].attack + "</color>" : " <color=#969696>♥: " + players[i].attack + "</color>";
            string money = "  <color=#E3C034>кеш: " + players[i].coins +"¢</color>";
            string loots = "  <color=#94EEEE>карт лута: " + players[i].lootCount + "</color>";

            playerText[i].text = hp  + endHp + addHp + power + money + loots + "  души: " + players[i].souls + (players[i] == GameMaster.inst.turnManager.activePlayer ? " (!)" : "");
        }
        UpdateAddInfo();
    }
    
    public void UpdateMonsterUI()
    {
        for(int i = 0; i < GameMaster.inst.monsterZone.monstersInSlots.Count; i++)
        {
            if(GameMaster.inst.monsterZone.monstersInSlots[i] is MonsterCard m)
            {
                string hp = "<color=#E34444>☻: " + m.hp;
                string addHp = m.preventHp == 0 ? "" : "<color=#94EEEE>("+m.preventHp+")</color>";
                string endHp = "/" + m.HpMax+ "</color>";
                monstersHpCounters[i].text = hp + endHp + addHp;
            }
            else
            {
                monstersHpCounters[i].text = "";
            }
            
        }
        for(int i = GameMaster.inst.monsterZone.monstersInSlots.Count; i < 4; i++)
        {
            monstersHpCounters[i].text = "";
        }
    }
    public void UpdateAddInfo()
    {
        attackText.text = "кол-во атак: " + GameMaster.inst.turnManager.activePlayer.attackCount;
        shopText.text = "цена: " + GameMaster.inst.turnManager.activePlayer.shopPrice + "¢    кол-во покупок: " + GameMaster.inst.turnManager.activePlayer.buyCount;
    }
    public void UpdateCubeUI(int count)
    {
        cubeText.text = count != 0 ? count.ToString() : "";
    }
}
