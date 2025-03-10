using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;
using System.Threading.Tasks;
public class Card : MonoBehaviour
{
    public const float CARDSIZE = 0.28f;
    public static Vector2 CARDPIXELSIZE = new Vector2(4.03f, 5.62f);
    [field: SerializeField] private CardData data;
    [field: SerializeField] public SpriteRenderer render { get; private set; }
    [field: SerializeField] public BoxCollider2D Collider { get; private set; }
    public event Action<Card> MouseClicked;
    public event Action<Card> MouseDown; 
    public event Action<Card> MouseExit;
    public virtual void Init(CardData d, bool isFaceUp = true)
    {
        data = d;
        gameObject.SetActive(false);
        render = gameObject.AddComponent<SpriteRenderer>();
        render.sortingOrder = 3;
        render.sprite = isFaceUp ? data.face : data.back;

        Collider = gameObject.AddComponent<BoxCollider2D>();
        Collider.size = CARDPIXELSIZE;

        SetActive(false);
        gameObject.SetActive(true);
    }
    protected float speed = GameMaster.CARD_SPEED;
    public void SetActive(bool active)
    {
        render.enabled = active;
        Collider.enabled = active;
    }
    public Player GetMyPlayer()
    {
        return GetComponentInParent<Player>();
    }
    public T GetData<T>() where T : CardData
    {
        return data as T;
    }
    public void MoveTo(Transform target, Transform parent, Action afterComplete = null, bool changeOrder = true)
    {
        Collider.enabled = false;
        int order = render.sortingOrder;
        if(changeOrder) render.sortingOrder = 1000;
        transform.DOScale(target.lossyScale, speed);
        transform.DOMove(target.position, speed).onComplete += () => 
        {
            render.sortingOrder = order;
            Collider.enabled = true;
            afterComplete?.Invoke();
        };
        if(parent != null) transform.parent = parent;
    }
    protected async Task DiscardCard<T>()
    {
        transform.parent = null;
        Dictionary<Type, CardDeck> posToDiscardCard = new Dictionary<Type, CardDeck>
        {
            {typeof(LootCard), G.Decks.lootStash},
            {typeof(ItemCard), G.Decks.treasureStash},
            {typeof(EventCard), G.Decks.monsterStash},
            {typeof(MonsterCard), G.Decks.monsterStash},
        };
        bool trigger = false;
        MoveTo(posToDiscardCard[typeof(T)].transform, null, () =>
        {
            //posToDiscardCard[typeof(T)].PutOneCardUp(this);
            trigger = true;
            SetActive(false);
        });
        while(!trigger) await Task.Yield();
    }
    public virtual async Task DiscardCard()
    {
        Debug.Log("NO CARD TYPE TO DISCARD");
        await Task.Yield();
    }
}
