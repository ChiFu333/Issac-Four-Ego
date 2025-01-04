using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst;
    [Header("Players")]
    public List<Player> Players = new List<Player>();
    public Player ActivePlayer;
    public int PriorId = 0;
    private int id = 0;
    public void Awake()
    {
        Inst = this;
    }
    public void Init()
    {
        InitAllPlayers();
        ActivePlayer = Players[id];
        
        UIOnDeck.Inst.UpdateTexts();
        
        StartGame();
    }
    public void StartGame()
    {
        ActivePlayer = Players[id];
        GiveStartResources();

        ActivePlayer.StartTurn();
    }
    public void SwitchTurn()
    {
        ActivePlayer.EndTurn();
        HealEveryone();

        id++;
        id %= Players.Count;

        ActivePlayer = Players[id];
        PriorId = id;
        ActivePlayer.StartTurn();
    }
    public void InitAllPlayers()
    {
        GameObject g = new GameObject();
        for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
        {
            //Создание объекта игрока
            GameObject p = Instantiate(g, transform);
            p.AddComponent<Player>();
            Players.Add(p.GetComponent<Player>());
            p.name = "Player " + i;

            //Создать ему руку
            GameObject h = new GameObject("Hand");
            Hand ha = h.AddComponent<Hand>();
            ha.transform.position = CardPlaces.Inst.Hands[i].position;
            ha.transform.parent = Players[i].transform;
            ha.transform.localScale = CardPlaces.Inst.Hands[i].lossyScale;

            //Дать ему карту персонажу
            CharacterCardData ccd = (CharacterCardData)GameMaster.Inst.CharacterDeck.TakeOneCard();
            CharacterCard c = Card.CreateCard<CharacterCard>(ccd);
            c.transform.DOMove(CardPlaces.Inst.PlayersPos[i][0].position,GameMaster.CARDSPEED);
            c.transform.localScale = CardPlaces.Inst.PlayersPos[i][0].lossyScale;
            c.Flip();
            ItemCard it = Card.CreateCard<ItemCard>(ccd.CharacterItemData); 
            it.transform.DOMove(CardPlaces.Inst.PlayersPos[i][1].position,GameMaster.CARDSPEED);
            it.transform.localScale = CardPlaces.Inst.PlayersPos[i][1].lossyScale;
            it.Flip();

            Players[i].Init(c, it, ha);
        }
        for(int j = GameMaster.PLAYERCOUNT; j < 4; j++)
        {
            foreach(Transform t in CardPlaces.Inst.PlayersPos[j])
            {
                t.gameObject.SetActive(false);
            }
            CardPlaces.Inst.PlayersCurses[j*2].gameObject.SetActive(false);
            CardPlaces.Inst.PlayersCurses[j*2+1].gameObject.SetActive(false);  
            CardPlaces.Inst.Hands[j].gameObject.SetActive(false);
            UIOnDeck.Inst.PlayerText[j].gameObject.SetActive(false);
        }
        Destroy(g);
    }
    public void GiveStartResources()
    {
        for(int i = 0; i < Players.Count; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                LootCard c = Card.CreateCard<LootCard>(GameMaster.Inst.LootDeck.TakeOneCard(), i == 0);
                Players[i].TakeOneCard(c);
            }
            Players[i].Coins = 3;
        }
    }
    public bool IsMyTurn()
    {
        return id == 0;
    }
    public Player GetPriorPlayer()
    {
        return Players[PriorId];
    }
    public void SetPrior(Player player)
    {
        for(int i = 0; i < Players.Count; i++)
        {
            if(Players[i] == player) 
            {
                PriorId = i;
                return;
            }
        }
    }
    public void RestorePrior()
    {
        PriorId = id;
    }
    public void HealEveryone()
    {
        for(int i = 0; i < Players.Count; i++)
        {
            Players[i].FullHeal();
        }
    }
}
