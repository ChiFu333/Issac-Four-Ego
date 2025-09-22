using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.EventSystems;
using Runtime;
using UnityEngine.Serialization;

public class Card : MonoBehaviour, GameView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectableTarget
{
    public const float CARDSIZE = 0.28f;
    public static Vector2 CARDPIXELSIZE = new Vector2(4.03f, 5.62f);
    [SerializeReference, SubclassSelector]
    public List<EntityComponentDefinition> state;
    
    public BoxCollider2D Collider;
    public SpriteRenderer visual;

    [HideInInspector] public StackUnit myStackUnit;
    [HideInInspector] public StackUnitCube stackUnitCreatedMe;
    #region [ Create & Discard ]

    [HideInInspector] public CMSEntity myPreset;
    public static Card CreateCard(CMSEntity st, bool isFaceUp = true)
    {
        DeckListConfig config = CMS.GetAll<CMSEntity>().FirstOrDefault(x => x.Is<DeckListConfig>())!.Get<DeckListConfig>();
        GameObject newCard = Instantiate(config.CardView);
        newCard.name = st.GetSprite().name;
        newCard.transform.localScale = Vector3.one * CARDSIZE;

        Card ent = newCard.GetComponent<Card>();
        G.Main.AllCards.Add(ent);
        ent.myPreset = st;
        ent.state = st.components.CloneTags();
        ent.AddTag(new TagTemporaryBuff());
        CardType t = ent.Get<TagCardType>().cardType;
        newCard.transform.position = t.GetDeckTopPos();

        ent.Init(isFaceUp);
        return ent;
    }
    public async UniTask DiscardCard()
    {
        visual.transform.DORotate(new Vector3(0,0,0), 0.3f);
        GetMyPlayer()?.TryRemoveItem(this);
        Dictionary<CardType, Deck> posToDiscardCard = new Dictionary<CardType, Deck>
        {
            {CardType.lootCard, G.Main.Decks.lootStash},
            {CardType.treasureCard, G.Main.Decks.treasureStash},
            {CardType.eventCard, G.Main.Decks.monsterStash},
            {CardType.monsterCard, G.Main.Decks.monsterStash},
        };
        await PutOnDeck(posToDiscardCard[Get<TagCardType>().cardType]);
    }

    public async UniTask PutOnDeck(Deck deck)
    {
        transform.parent = null;
        
        transform.DOScale(deck.transform.lossyScale, ActionTime.timeToChangeScaleCard/2);
        visual.sortingOrder = deck.faceRenderer.sortingOrder + 3;
        
        Vector3 exactTopPos = deck.GetMyPos(false);
        await MoveTo(exactTopPos);
        G.AudioManager.PlaySound(R.Audio.CardTaked, -0.3f);
        if (!deck.isFaceUp && isFaceUp) await Flip(false, true);
        deck.PutOneCardUp(this);
    }
    public async UniTask PutUnderDeck(Deck deck)
    {
        transform.parent = null;
        
        transform.DOScale(deck.transform.lossyScale, ActionTime.timeToChangeScaleCard/2);
        
        Vector3 exactTopPos = deck.GetMyPos(true);
        visual.sortingOrder = deck.shadowCards[0].GetComponent<SpriteRenderer>().sortingOrder - 1;
        if (!deck.isFaceUp && isFaceUp) _ = Flip(false);
        await MoveTo(exactTopPos + Vector3.down * 2.3f);
        await WaitUntilStop();
        await UniTask.Delay(25);
        G.AudioManager.PlaySound(R.Audio.CardTaked, -0.5f);
        await MoveTo(deck.GetMyPos(true));
        deck.PutOneCardUnder(this);
    }
    #endregion
    
    #region  [ Card's Transform ]
    public bool isMoving;
    public bool isFaceUp = false;
    private CancellationTokenSource _moveCts;
    public bool ActiveSelf;
    private void Init(bool isFU = true)
    {
        isFaceUp = isFU;
        _moveCts = new CancellationTokenSource();
        visual = GetComponentInChildren<SpriteRenderer>();
        Collider = GetComponent<BoxCollider2D>();
        _moveEngine = new MoveEngine(transform,visual.transform);
        if(Get<TagCardType>().cardType == CardType.treasureCard) AddTag(new TagIsItem());
        
        foreach (var t in state) if(t is IInitable initable) initable.Init(this);
        gameObject.SetActive(false);
        visual.sortingOrder = 3;
        visual.sprite = isFaceUp ? Get<TagSprite>().sprite : Get<TagCardType>().cardType.GetBackSprite();

        SetActive(false);
        gameObject.SetActive(true);
    }
    public void SetActive(bool active)
    {
        visual.enabled = active;
        Collider.enabled = active;
        ActiveSelf = active;
    }

    public async UniTask MoveTo(Vector3 target, float targetRotation = 0)
    {
        _moveEngine.SetTarget(target, targetRotation);
        if (isMoving)
        {
            while (isMoving) await UniTask.Yield();
            return;
        }
        isMoving = true;
        
        while (_moveEngine.CheckDistant())
        {
            _moveEngine.SmoothFollow();
            _moveEngine.FollowRotation();
            await UniTask.Yield();
        }
        _moveEngine.SetToTargetPos();
        isMoving = false;
    }
    public async UniTask Flip(bool flip, bool doubleSpeed = false)
    {
        if (flip == isFaceUp) return;
        
        await transform.GetChild(0).DOLocalRotate(new Vector3(0, 90, 0), ActionTime.timeToFlipCard * (doubleSpeed ? 0.75f : 1))
            .SetEase(Ease.InCubic)
            .ToUniTask(cancellationToken: _moveCts.Token);
        isFaceUp = flip;
        foreach (var visualTag in state)
        {
            if (visualTag is IHaveUI UITag)
            {
                if(flip) UITag.ShowUI();
                else UITag.HideUI();
            }
        }
        visual.sprite = isFaceUp ? Get<TagSprite>().sprite : Get<TagCardType>().cardType.GetBackSprite();
        await transform.GetChild(0).DOLocalRotate(new Vector3(0, 0, 0), ActionTime.timeToFlipCard * (doubleSpeed ? 0.75f : 1)).SetEase(Ease.OutCubic).ToUniTask(cancellationToken: _moveCts.Token);
    }

    public void HideUI()
    {
        foreach (var visualTag in state)
        {
            if (visualTag is IHaveUI UITag)
            {
                UITag.HideUI();
            }
        }
    }
    public async UniTask WaitUntilStop()
    {
        await UniTask.WaitUntil(() => isMoving == false);
    }
    
    private MoveEngine _moveEngine;
    public class MoveEngine
    {
        private Transform _cardTransform;
        private Transform _visualTransform;
        
        private Vector2 targetPosition;
        private float targetRotation;
        private Vector2 velocity; 
        private float smoothTime = 0.12f; 
        private float maxVelocity = 8.5f; 
        private Vector2 currentVelocity;
        private Vector3 movementDelta;
        private Vector3 rotationDelta;

        public MoveEngine(Transform t, Transform visual)
        {
            _cardTransform = t;
            _visualTransform = visual;
        }

        public void SetTarget(Vector2 targetPos, float targetRot)
        {
            targetPosition = targetPos;
            targetRotation = targetRot;
            maxVelocity = ActionTime.cardSpeed;
        }

        public bool CheckDistant()
        {
            if (_cardTransform == null) return false;
            return Vector2.Distance(_cardTransform.position, targetPosition) > 0.0005f || velocity.magnitude > 0.0005f;
        }
            
        public void SmoothFollow()
        {
            Vector2 newPosition = Vector2.SmoothDamp(_cardTransform.position, targetPosition, ref currentVelocity, smoothTime, maxVelocity, Time.deltaTime);
            velocity = (newPosition - (Vector2)_cardTransform.position) / Time.deltaTime;

            if (velocity.sqrMagnitude > maxVelocity * maxVelocity)
            {
                velocity = velocity.normalized * maxVelocity;
            }

            Vector2 newPos = newPosition + velocity * (Time.deltaTime);
            _cardTransform.position = new Vector3(newPos.x, newPos.y, _cardTransform.position.z);
        } 
        
        public void FollowRotation()
        {
            float rotationAmount = 20;
            float rotationSpeed = 20;

            Vector3 movement = (_cardTransform.position - (Vector3)targetPosition);
            movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
            Vector3 movementRotation = movement * rotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta , movementRotation, rotationSpeed * Time.deltaTime);
            _visualTransform.eulerAngles = new Vector3(_visualTransform.eulerAngles.x, _visualTransform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60) + targetRotation);
        }
        
        public void SetToTargetPos()
        {   
            _cardTransform.position = new Vector3(targetPosition.x, targetPosition.y, _cardTransform.position.z);
            _visualTransform.localEulerAngles = new Vector3(_visualTransform.eulerAngles.x, _visualTransform.eulerAngles.y, this.targetRotation);
            velocity = Vector2.zero;
        }
    }
    #endregion

    #region [ Tag's Actions ]
    
    public void AddTag(EntityComponentDefinition t)
    {
        if(t is IInitable tInitable) tInitable.Init(this);
        state.Add(t);
    }
    public void RemoveTag(EntityComponentDefinition t)
    {
        state.Remove(t);
        if(t is IRemovable tRemovable) tRemovable.Remove();
    }
    public T Get<T>() where T : EntityComponentDefinition
    {
        return state.Find(m => m is T) as T;
    }
    public bool Is<T>() where T : EntityComponentDefinition
    {
        return Get<T>() != null;
    }
    #endregion
    
    #region [ Pointer Events ]

    public SlotZone zoneHolder;
    public async void OnPointerEnter(PointerEventData eventData)
    {
        if(!isMoving && G.Main.ActionChecker.CheckForCard(zoneHolder))
            foreach(var tag in state.ToList()) if(tag is IOnMouseEnter eve) await eve.OnMouseEnter();
    }

    public async void OnPointerExit(PointerEventData eventData)
    {
        if(G.Main.ActionChecker.CheckForCard(zoneHolder))
            foreach(var tag in state.ToList()) if(tag is IOnMouseExit eve) await eve.OnMouseExit();
    }
    public async void OnPointerClick(PointerEventData eventData)
    {
        if (!isMoving && G.Main.ActionChecker.CheckForCard(zoneHolder))
        {
            foreach(var tag in state.ToList()) if(tag is IOnMouseDown eve) await eve.OnMouseDown();
        }
             
    }
    #endregion

    public Player GetMyPlayer() => GetComponentInParent<Player>();
    public GameObject GetVisualObject<T>() where T : EntityComponentDefinition
    {
        Dictionary<Type, GameObject> visualTags = new Dictionary<Type, GameObject>()
        {
            { typeof(TagCharacteristics), visual.transform.GetChild(0).gameObject},
            { typeof(TagTappable), visual.transform.GetChild(1).gameObject},
        };
        return visualTags[typeof(T)];
    }
    private void OnDestroy()
    {
        _moveCts?.Cancel();
        _moveCts?.Dispose();

        DOTween.Kill(transform);
        DOTween.Kill(visual);
        G.Main.AllCards.Remove(this);
    }
}
public static class Helper
{
    public static Card CloneCard(this Card card)
    {
        Card c = Card.CreateCard(card.myPreset, card.isFaceUp);
        return c;
    }

    public static List<EntityComponentDefinition> CloneTags(this List<EntityComponentDefinition> l)
    {
        List<EntityComponentDefinition> result = new List<EntityComponentDefinition>();
        foreach (var tag in l)
        {
            result.Add(tag.DeepCopy());
        }

        return result;
    }
}