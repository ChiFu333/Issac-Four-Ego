using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Rendering;

public class TurnManager : MonoBehaviour
{
    [field: SerializeField] public List<Player> players { get; private set; } = new List<Player>();
    public Player activePlayer { get => players[id];}
    public Player priorPlayer { get => players[priorId];}
    [field: SerializeField] public int priorId { get; private set; } = 0;
    public Card cardTarget;
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
    public async void SwitchTurn()
    {
        if(!SubSystems.inst.isSelectingSomething)
        {
            await activePlayer.EndTurn();

            id++;
            id %= players.Count;

            HealEveryone();
            GameMaster.inst.monsterZone.RestoreAllStats();
            priorId = id;
            activePlayer.StartTurn();
        }
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
            //Инициировать персонажей
            players[i].Init(ha);
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
            players[i].AddMoney(3);
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
            players[i].HealHp(100);
            players[i].SetBaseStats();
        }
    }
    public int GetMyId(Player p)
    {
        for(int i = 0; i < GameMaster.PLAYERCOUNT; i++)
        {
            if(players[i] == p) return i;
        }
        return -1;
    }
}
