using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Entity : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public const float CARDSIZE = 0.28f;
    
    public EntityVisual visual { get; private set; }
    public BoxCollider2D Collider { get; private set; }
    
    [SerializeReference] 
    public List<ITag> tags = new List<ITag>();
    
    private float _speed = GameMaster.CARD_SPEED;

    private void Init(bool isFaceUp = true)
    {
        visual = GetComponentInChildren<EntityVisual>();
        visual.Init(this);
        Collider = GetComponent<BoxCollider2D>();

        visualTags = new Dictionary<Type, GameObject>()
        {
            { typeof(Characteristics), visual.transform.GetChild(0).gameObject},
        };
        
        foreach (var t in tags) t.Init(this);

        gameObject.SetActive(false);
        visual.render.sortingOrder = 3;
        visual.render.sprite = GetTag<CardSpritesData>().isFlipped ? GetTag<CardSpritesData>().front : GetTag<CardSpritesData>().back;

        SetActive(false);
        gameObject.SetActive(true);
    }
    public void SetActive(bool active)
    {
        visual.render.enabled = active;
        Collider.enabled = active;
        foreach(var tag in tags)
            if(tag is IHaveUI uiTag)
                if(active) uiTag.ShowUI();
                else uiTag.HideUI();
    }
    
    #region Tag and Flag
    public void AddTag(ITag t)
    {
        t.Init(this);
        tags.Add(t);
        
    }
    public bool HasTag<T>() where T : ITag
    {
        foreach (var t in tags.ToList())
        {
            if(t is T) return true;
        }

        return false;
    }
    public T GetTag<T>() where T : ITag
    {
        foreach (var t1 in tags.ToList())
        {
            if(t1 is T t) return t;
        }

        throw new Exception("Нет такого тега!");
    }

    public void RemoveTag(ITag t)
    {
        foreach (var t1 in tags.ToList())
        {
            if (t1 == t)
            {
                tags.Remove(t1);
                break;
            }
        }
    }
    #endregion
    
    public static Entity CreateEntity(GameObject prefab, bool isFaceUp = true)
    {
        Dictionary<CardType, Vector3> posToSpawnCard = new Dictionary<CardType, Vector3>
        {
            {CardType.lootCard, G.CardPlaces.lootDeck.position},
            {CardType.characterCard, G.CardPlaces.otherPos},
            {CardType.treasureCard, G.CardPlaces.treasureDeck.position},
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
    public async UniTask DiscardEntity()
    {
        transform.parent = null;
        Dictionary<CardType, CardDeck> posToDiscardCard = new Dictionary<CardType, CardDeck>
        {
            {CardType.lootCard, G.Decks.lootStash},
            {CardType.treasureCard, G.Decks.treasureStash},
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
        while(!trigger) await UniTask.Yield();
    }
    
    public void MoveTo(Transform target, Transform parent, Action afterComplete = null, bool changeOrder = true)
    {   
        visual.transform.eulerAngles = new Vector3(0, 0, 0);
        Collider.enabled = false;
        int order = visual.render.sortingOrder;
        if(changeOrder) visual.render.sortingOrder = 1000;
        transform.DOScale(target.lossyScale, _speed);
        transform.DOMove(target.position, _speed).onComplete += () => 
        {
            visual.render.sortingOrder = order;
            Collider.enabled = true;
            afterComplete?.Invoke();
        };
        if(parent != null) transform.parent = parent;
    }
    public void MoveTo(Vector3 target, Transform parent, Action afterComplete = null, bool changeOrder = true)
    {     
        visual.transform.eulerAngles = new Vector3(0, 0, 0);   
        Collider.enabled = false;
        int order = visual.render.sortingOrder;
        if(changeOrder) visual.render.sortingOrder = 1000;
        transform.DOMove(target, _speed).onComplete += () => 
        {
            visual.render.sortingOrder = order;
            Collider.enabled = true;
            afterComplete?.Invoke();
        };
        if(parent != null) transform.parent = parent;
    }
    
    public async UniTask MoveToForHand(Vector3 target, float angle, Action afterComplete = null, bool changeOrder = true)
    {
        Collider.enabled = false;
        int order = visual.render.sortingOrder;
        if(changeOrder) visual.render.sortingOrder = 1000;
        await visual.MoveTo(target, angle,() => 
        {
            visual.render.sortingOrder = order;
            Collider.enabled = true;
            afterComplete?.Invoke();
        });
    }
    
    
    public async void OnPointerEnter(PointerEventData eventData)
    {
        //PointerEnterEvent.Invoke(this);
        isHovering = true;
        if(!G.CardSelector.isSelectingSomething)
            foreach(var tag in tags.ToList()) if(tag is IOnMouseEnter eve) await eve.OnMouseEnter();
    }

    public async void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if(!G.CardSelector.isSelectingSomething)
            foreach(var tag in tags.ToList()) if(tag is IOnMouseExit eve) await eve.OnMouseExit();
    }
    public async void OnPointerClick(PointerEventData eventData)
    {
        if(!G.CardSelector.isSelectingSomething)
             foreach(var tag in tags.ToList()) if(tag is IOnMouseDown eve) await eve.OnMouseDown();
    }
    
    public async UniTask PutCardNearHand(Hand h)
    {
        bool trigger = false;
        transform.parent = null;
        if(GetTag<CardTypeTag>().cardType == CardType.lootCard) _speed = GameMaster.CARD_SPEED / 2.5f;
        MoveTo(h.transform.TransformPoint(new Vector3(0, Hand.UPMOVE * 3f)), null, () => 
        {
            trigger = true;
            _speed = GameMaster.CARD_SPEED;
        });
        
        while(!trigger) await UniTask.Yield();
    }
    public Player GetMyPlayer()
    {
        return GetComponentInParent<Player>();
    }
    public async UniTask Shake()
    {
        bool trigger = false;
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, elasticity: 0.1f);
        transform.DOPunchRotation(new Vector3(0,0,20), 0.3f, elasticity: 0.1f).onComplete = () => trigger = true;
        while(!trigger) await UniTask.Yield();
    }
    
    public Dictionary<Type, GameObject> visualTags = new Dictionary<Type, GameObject>();

    public bool isDragging = false;
    public bool isHovering = false;
}
