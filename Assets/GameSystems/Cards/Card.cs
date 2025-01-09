using UnityEngine;
using System.Collections.Generic;
using System;

public class Card : MonoBehaviour
{
    public const float CARDSIZE = 0.28f;
    public static Vector2 CARDPIXELSIZE = new Vector2(4.03f, 5.62f);
    [field: SerializeField] public CardData data { get; private set; }
    [field: SerializeField] public SpriteRenderer Renderer { get; private set; }
    [field: SerializeField] public BoxCollider2D Collider { get; private set; }
    public event Action<Card> MouseClicked;
    public event Action<Card> MouseDown; 
    public event Action<Card> MouseExit;
    public virtual void Init(CardData d, bool isFaceUp = true)
    {
        data = d;

        Renderer = gameObject.AddComponent<SpriteRenderer>();
        Renderer.sortingOrder = 3;
        Renderer.sprite = isFaceUp ? data.face : data.back;

        Collider = gameObject.AddComponent<BoxCollider2D>();
        ChangeColliderSize(CARDPIXELSIZE);
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
        if(!SubSystems.Inst.IsSelectingSomething) MouseClicked?.Invoke(this);
    }
    public void ChangeColliderSize(Vector2 s)
    {
        Collider.size = s;
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
    public Player GetMyPlayer()
    {
        return GetComponentInParent<Player>();
    }
}
