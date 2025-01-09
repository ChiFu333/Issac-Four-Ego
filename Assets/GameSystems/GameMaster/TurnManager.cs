using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class TurnManager : MonoBehaviour
{
    [field: SerializeField] public List<Player> players { get; private set; } = new List<Player>();
    public Player activePlayer { get => players[id];}
    public Player priorPlayer { get => players[priorId];}
    [field: SerializeField] public int priorId { get; private set; } = 0;
    private int id = 0;
    public void Init()
    {
        InitAllPlayers();
        UIOnDeck.inst.UpdateTexts();
        StartGame();
    }
    public void StartGame()
    {
        GiveStartResources();
        activePlayer.StartTurn();
    }
    public void SwitchTurn()
    {
        activePlayer.EndTurn();

        id++;
        id %= players.Count;

        HealEveryone();

        priorId = id;
        activePlayer.StartTurn();
    }
    private void InitAllPlayers()
    {
        GameObject g = new GameObject();
        for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
        {
            
            //Создание объекта игрока
            GameObject p = Instantiate(g, transform);
            Player player = p.AddComponent<Player>();
            players.Add(p.GetComponent<Player>());
            p.name = "Player " + i;

            //Создать ему руку
            GameObject h = new GameObject("Hand");
            Hand ha = h.AddComponent<Hand>();
            ha.transform.position = CardPlaces.inst.hands[i].position;
            ha.transform.parent = players[i].transform;
            ha.transform.localScale = CardPlaces.inst.hands[i].lossyScale;

            SetPrior(player);
            //Дать ему карту персонажу
            CharacterCardData ccd = (CharacterCardData)GameMaster.inst.characterDeck.TakeOneCard();
            CharacterCard c = Card.CreateCard<CharacterCard>(ccd);
            c.transform.DOMove(CardPlaces.inst.playersPos[i][0].position,GameMaster.CARDSPEED);
            c.transform.localScale = CardPlaces.inst.playersPos[i][0].lossyScale;
            c.Flip();

            ItemCard it = Card.CreateCard<ItemCard>(ccd.characterItemData); 
            it.Flip();

            players[i].Init(c, it, ha);
        }
        RestorePrior();
        for(int j = GameMaster.PLAYERCOUNT; j < 4; j++)
        {
            foreach(Transform t in CardPlaces.inst.playersPos[j])
            {
                t.gameObject.SetActive(false);
            }
            CardPlaces.inst.playersCurses[j][0].gameObject.SetActive(false);
            CardPlaces.inst.playersCurses[j][1].gameObject.SetActive(false);
            CardPlaces.inst.hands[j].gameObject.SetActive(false);
            UIOnDeck.inst.playerText[j].gameObject.SetActive(false);
        }
        Destroy(g);
    }
    private void GiveStartResources()
    {
        for(int i = 0; i < players.Count; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                LootCard c = Card.CreateCard<LootCard>(GameMaster.inst.lootDeck.TakeOneCard());
                players[i].TakeOneCard(c);
            }
            players[i].ChangeMoney(3);
        }
    }
    public bool IsMyTurn()
    {
        return id == 0;
    }
    public void SetPrior(Player player)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if(players[i] == player) 
            {
                priorId = i;
                return;
            }
        }
    }
    public void RestorePrior()
    {
        priorId = id;
    }
    public void HealEveryone()
    {
        for(int i = 0; i < players.Count; i++)
        {
            players[i].SetBaseStats();
        }
    }
}
