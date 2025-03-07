using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using System;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Entity : MonoBehaviour, IPointerClickHandler
{
    private const float CARDSIZE = 0.28f;
    [SerializeReference]
    public List<ITag> tags = new List<ITag>();
    public SpriteRenderer render { get; private set; }
    public BoxCollider2D Collider { get; private set; }
    protected float speed = GameMaster.CARDSPEED;
    public void Init(bool isFaceUp = true)
    {
        render = GetComponent<SpriteRenderer>();
        Collider = GetComponent<BoxCollider2D>();

        visualTags = new Dictionary<Type, GameObject>()
        {
            { typeof(Characteristics), transform.GetChild(0).gameObject},
        };
        
        foreach (var tag in tags) tag.Init(this);

        gameObject.SetActive(false);
        render.sortingOrder = 3;
        render.sprite = GetTag<CardSpritesData>().isFlipped ? GetTag<CardSpritesData>().front : GetTag<CardSpritesData>().back;

        SetActive(false);
        gameObject.SetActive(true);
    }
    public void SetActive(bool active)
    {
        render.enabled = active;
        Collider.enabled = active;
        foreach(var tag in tags)
            if(tag is IHaveUI uiTag)
                if(active) uiTag.ShowUI();
                else uiTag.HideUI();
    }
    #region Tag and Flag
    public void AddTag(ITag t)
    {
        tags.Add(t);
    }
    public bool HasTag<T>() where T : ITag
    {
        for(int i = 0; i < tags.Count; i++)
        {
            if(tags[i] is T) return true;
        }
        return false;
    }
    public T GetTag<T>() where T : ITag
    {
        for(int i = 0; i < tags.Count; i++)
        {
            if(tags[i] is T t) return t;
        }
        throw new Exception("Нет такого тега!");
    }
    public void AddFlag(ITag flag)
    {
        AddTag(flag);
    }
    public bool Is<T>() where T : IFlag
    {
        foreach (var tag in tags) if(tag is T) return true;
        return false;
    }
    #endregion
    public async void OnPointerClick(PointerEventData eventData)
    {
        foreach(var tag in tags) if(tag is IOnMouseDown eve) await eve.OnMouseDown();
    }
    public static Entity CreateEntity(GameObject prefab, bool isFaceUp = true)
    {
        Dictionary<CardType, Vector3> posToSpawnCard = new Dictionary<CardType, Vector3>
        {
            {CardType.lootCard, G.CardPlaces.lootDeck.position},
            {CardType.characterCard, G.CardPlaces.otherPos},
            {CardType.treasureCard, G.CardPlaces.shopDeck.position},
            {CardType.monsterCard, G.CardPlaces.monsterDeck.position},
            {CardType.eventCard, G.CardPlaces.monsterDeck.position},
        };
        GameObject newCard = Instantiate(prefab);
        newCard.transform.localScale = Vector3.one * CARDSIZE;

        Entity ent = newCard.GetComponent<Entity>();
        newCard.transform.position = posToSpawnCard.ContainsKey(ent.GetTag<CardTypeTag>().cardType) ? posToSpawnCard[ent.GetTag<CardTypeTag>().cardType] : G.CardPlaces.otherPos;

        ent.Init(isFaceUp);
        return ent;
    }
    public async Task DiscardEntity()
    {
        transform.parent = null;
        Dictionary<CardType, CardDeck> posToDiscardCard = new Dictionary<CardType, CardDeck>
        {
            {CardType.lootCard, G.Decks.lootStash},
            {CardType.treasureCard, G.Decks.shopStash},
            {CardType.eventCard, G.Decks.monsterStash},
            {CardType.monsterCard, G.Decks.monsterStash},
        };
        bool trigger = false;
        MoveTo(posToDiscardCard[GetTag<CardTypeTag>().cardType].transform, null, () =>
        {
            posToDiscardCard[GetTag<CardTypeTag>().cardType].PutOneCardUp(this);
            trigger = true;
            SetActive(false);
        });
        while(!trigger) await Task.Yield();
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
    public event Action<Entity> MouseDown; 
    public event Action<Entity> MouseExit;
    public void OnMouseEnter()
    {
        MouseDown?.Invoke(this);
    }
    public void OnMouseExit()
    {
        MouseExit?.Invoke(this);
    }
    public async Task PutCardNearHand(Hand h)
    {
        bool trigger = false;
        if(GetTag<CardTypeTag>().cardType == CardType.lootCard) speed = GameMaster.CARDSPEED / 2.5f;
        MoveTo(h.transform.TransformPoint(new Vector3(0, Hand.UPMOVE * 3f)), null, () => 
        {
            trigger = true;
            speed = GameMaster.CARDSPEED;
        });
        while(!trigger) await Task.Yield();
    }
    public Player GetMyPlayer()
    {
        return GetComponentInParent<Player>();
    }
    public async Task Shake()
    {
        bool trigger = false;
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, elasticity: 0.1f);
        transform.DOPunchRotation(new Vector3(0,0,20), 0.3f, elasticity: 0.1f).onComplete = () => trigger = true;
        while(!trigger) await Task.Yield();
    }
    
    public Dictionary<Type, GameObject> visualTags = new Dictionary<Type, GameObject>();

}