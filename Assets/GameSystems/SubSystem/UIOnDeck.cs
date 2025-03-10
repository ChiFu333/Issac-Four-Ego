using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class UIOnDeck : MonoBehaviour
{
    public static UIOnDeck inst { get; private set; }
    [field: SerializeField] public List<TMP_Text> playerText { get; private set; }
    [field: SerializeField] public TMP_Text attackText { get; private set; }
    [field: SerializeField] public TMP_Text shopText { get; private set; }
    [field: SerializeField] public List<GameObject> buttons;
    [field: SerializeField] public List<SpriteRenderer> stackCardsRenderers;
    [field: SerializeField] public List<SpriteRenderer> stackTargetRenderers;
    [field: SerializeField] public TMP_Text PhaseText;
    public void Awake()
    {
        inst = this;
    }

    public void UpdateTexts(int id = -1)
    {
        List<Player> players = G.Players.players;
        int count = id != -1 ? id+1 : players.Count;
        for(int i = 0; i < count; i++)
        {
            /*
            string hp = "<color=#E34444>☻: " + players[i].characteristics.health;
            string addHp = players[i].characteristics.healthPrevent == 0 ? "" : "<color=#94EEEE>("+players[i].characteristics.healthPrevent+")</color>";
            string endHp = "/" + players[i].characteristics.HealthMax+ "</color>";

            string power = players[i].characteristics.healthPrevent == 0 ? "  <color=#969696>♥: " + players[i].attack + "</color>" : " <color=#969696>♥: " + players[i].attack + "</color>";
            */
            //string money = "  <color=#E3C034>кеш: " + players[i].coins +"¢</color>";
            //string loots = "  <color=#94EEEE>карт лута: " + players[i].lootCount + "</color>";

            //playerText[i].text = money + loots + "  души: " + players[i].souls + (players[i] == G.Players.activePlayer ? " (!)" : "");
        }
        UpdateAddInfo();
    }
    public void UpdateMonsterUI()
    {
        for(int i = 0; i < G.monsterZone.monstersInSlots.Count; i++)
        {
            if(G.monsterZone.monstersInSlots[i].GetTag<CardTypeTag>().cardType == CardType.monsterCard)
            {
                Entity m = G.monsterZone.monstersInSlots[i];
                /*
                string hp = "<color=#E34444>☻: " + m.hp;
                string addHp = m.preventHp == 0 ? "" : "<color=#94EEEE>("+m.preventHp+")</color>";
                string endHp = "/" + m.HpMax+ "</color>";
                monstersHpCounters[i].text = hp + endHp + addHp;
                */
            }
        }
    }
    public void UpdateAddInfo()
    {
        attackText.text = "кол-во атак: " + G.Players.activePlayer.attackCount;
        shopText.text = "цена: " + G.Players.activePlayer.shopPrice + "¢    кол-во покупок: " + G.Players.activePlayer.buyCount;
    }
    public void ChangeButtonsActive()
    {
        for(int i = 0; i < buttons.Count; i++) 
        {
            bool a = StackSystem.inst.stack.Count == 0;
            bool b = !SubSystems.inst.isSelectingSomething;
            bool c = GameMaster.inst.phaseSystem.currentPhase == Phase.Action && GameMaster.inst.phaseSystem.subphases == 0;
            bool d = G.Players.activePlayer.buyCount != 0;
            bool e = G.Players.activePlayer.attackCount != 0;
            buttons[i].SetActive(a && b && c && (d || i != 0) && (e || i != 1));
        }
    }
    public void UpdateStack()
    {
        StackEffect[] st = StackSystem.inst.stack.ToArray();
        for(int i = 0; i < 7; i++)
        {
            if(st.Length > i && st[i] != null)
            {
                stackCardsRenderers[i].sprite = st[st.Length - i - 1].GetSprite(true);
                stackTargetRenderers[i].sprite = st[st.Length - i - 1].GetSprite(false);
            }
            else
            {
                stackCardsRenderers[i].sprite = null;
                stackTargetRenderers[i].sprite = null;
            }
        }
        ChangeButtonsActive();
    }
    private string firstWordPhrase = "";
    public void UpdatePhase(string one = null)
    {
        if(one != null) firstWordPhrase = one;
        PhaseText.text = firstWordPhrase;
    }
}
