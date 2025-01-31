using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;
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

        render = gameObject.AddComponent<SpriteRenderer>();
        render.sortingOrder = 3;
        render.sprite = isFaceUp ? data.face : data.back;

        Collider = gameObject.AddComponent<BoxCollider2D>();
        Collider.size = CARDPIXELSIZE;
    }
    public Player GetMyPlayer()
    {
        return GetComponentInParent<Player>();
    }
    public T GetData<T>() where T : CardData
    {
        return data as T;
    }
    public void OnMouseEnter()
    {
        MouseDown?.Invoke(this);
    }
    public void OnMouseExit()
    {
        MouseExit?.Invoke(this);
    }
    public void OnMouseDown()
    {
        if(!SubSystems.inst.isSelectingSomething) MouseClicked?.Invoke(this);
    }
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
    public static T CreateCard<T>(CardData data, bool isFaceUp = true) where T : Card
    {
        Dictionary<Type, Vector3> posToSpawnCard = new Dictionary<Type, Vector3>
        {
            {typeof(LootCard), CardPlaces.inst.lootDeck.position},
            {typeof(CharacterCard), CardPlaces.inst.otherPos},
            {typeof(ItemCard), CardPlaces.inst.shopDeck.position},
            {typeof(EventCard), CardPlaces.inst.monsterDeck.position},
            {typeof(MonsterCard), CardPlaces.inst.monsterDeck.position},
        };
        GameObject newCard = new GameObject(data.name);
        newCard.transform.localScale = Vector3.one * CARDSIZE;
        newCard.transform.position = posToSpawnCard.ContainsKey(typeof(T)) ? posToSpawnCard[typeof(T)] : CardPlaces.inst.otherPos;
        Card c = newCard.AddComponent<T>();
        c.Init(data, isFaceUp);
        return (T)c;
    }
    public void MoveTo(Transform target, Transform parent, Action afterComplete = null, bool changeOrder = true)
    {
        Collider.enabled = false;
        int order = render.sortingOrder;
        if(changeOrder) render.sortingOrder = 1000;
        transform.DOScale(target.lossyScale, GameMaster.CARDSPEED);
        transform.DOMove(target.position,GameMaster.CARDSPEED).onComplete += () => 
        {
            afterComplete?.Invoke();
            render.sortingOrder = order;
            Collider.enabled = true;
        };
        if(parent != null) transform.parent = parent;
    }
    public void MoveTo(Vector3 target, Transform parent, Action afterComplete = null, bool changeOrder = true)
    {
        Collider.enabled = false;
        int order = render.sortingOrder;
        if(changeOrder) render.sortingOrder = 1000;
        transform.DOMove(target, GameMaster.CARDSPEED).onComplete += () => 
        {
            afterComplete?.Invoke();
            render.sortingOrder = order;
            Collider.enabled = true;
        };
        if(parent != null) transform.parent = parent;
    }
}
