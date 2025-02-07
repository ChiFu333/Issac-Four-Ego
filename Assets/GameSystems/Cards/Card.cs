using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.Animations;
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
    protected float speed = GameMaster.CARDSPEED;
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
        transform.DOScale(target.lossyScale, speed);
        transform.DOMove(target.position, speed).onComplete += () => 
        {
            render.sortingOrder = order;
            Collider.enabled = true;
            afterComplete?.Invoke();
        };
        if(parent != null) transform.parent = parent;
    }
    public void MoveTo(Vector3 target, Transform parent, Action afterComplete = null, bool changeOrder = true)
    {
        Collider.enabled = false;
        int order = render.sortingOrder;
        if(changeOrder) render.sortingOrder = 1000;
        transform.DOMove(target, speed).onComplete += () => 
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
            {typeof(LootCard), GameMaster.inst.lootStash},
            {typeof(ItemCard), GameMaster.inst.shopStash},
            {typeof(EventCard), GameMaster.inst.monsterStash},
            {typeof(MonsterCard), GameMaster.inst.monsterStash},
        };
        bool trigger = false;
        MoveTo(posToDiscardCard[typeof(T)].transform, null, () =>
        {
            posToDiscardCard[typeof(T)].PutOneCardUp(this);
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
    public async Task PutCardNearHand(Hand h)
    {
        bool trigger = false;
        if(this is LootCard) speed = GameMaster.CARDSPEED / 2.5f;
        MoveTo(h.transform.TransformPoint(new Vector3(0, Hand.UPMOVE * 3f)), null, () => 
        {
            trigger = true;
            speed = GameMaster.CARDSPEED;
        });
        while(!trigger) await Task.Yield();
    }
    public async Task Shake()
    {
        bool trigger = false;
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, elasticity: 0.1f);
        transform.DOPunchRotation(new Vector3(0,0,20), 0.3f, elasticity: 0.1f).onComplete = () => trigger = true;
        while(!trigger) await Task.Yield();
    }
}
