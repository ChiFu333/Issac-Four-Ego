using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Rendering;
using System.Threading.Tasks;

public class TurnManager : MonoBehaviour
{
    [field: SerializeField] public List<Player> players { get; private set; } = new List<Player>();
    public Player activePlayer { get => players[id];}
    public Player priorPlayer() => players[priorId];
    [field: SerializeField] public int priorId { get; private set; } = 0;
    public int id { get; private set; } = 0;
    
    public void Init()
    {
        InitAllPlayers();
        UIOnDeck.inst.UpdateTexts();
        StartGame();
    }
    public void StartGame()
    {
        GiveStartResources();
        HealEveryone();
        GameMaster.inst.phaseSystem.StartStartTurn();
    }
    public void SwitchTurn()
    {
        id++;
        id %= players.Count;

        priorId = id;
        GameMaster.inst.phaseSystem.StartStartTurn();
    }
    public void EndTurn()
    {
        
        _ = GameMaster.inst.phaseSystem.StartEndPhase();
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
            if(i != 0) h.transform.Rotate(0, 0, 180);
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
                players[i].TakeOneLootCard(c);
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
            players[i].HealHp(100, true);
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
public enum Phase 
{
    Start, Action, End
}
public enum SubPhase 
{
    None, Shop, Attack, PlayerDie, MonsterDie
}
