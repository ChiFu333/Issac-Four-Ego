using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIOnDeck : MonoBehaviour
{
    public static UIOnDeck Inst; 
    public List<TMP_Text> PlayerText;
    public TMP_Text MonsterText;
    public TMP_Text ShopText;
    public void Awake()
    {
        Inst = this;
    }

    public void UpdateTexts()
    {
        List<Player> players = TurnManager.Inst.Players;
        for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
        {
            PlayerText[i].text = "хп: " + players[i].Hp + "    сила: " + players[i].Attack + "    кеш: " + players[i].Coins + "¢    карт лута: "+ players[i].LootCount +"    души: "+ players[i].Souls;
        }
        ShopText.text = "Цена: " + TurnManager.Inst.ActivePlayer.ShopPrice + "¢    кол-во покупок: " + TurnManager.Inst.ActivePlayer.BuyCount;
    }
}
