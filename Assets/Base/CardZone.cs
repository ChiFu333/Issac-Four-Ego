using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

public class SlotZone : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private float zoneWidth = 6.5f; // Ширина зоны

    [Header("Interactions")] 
    [SerializeField] private bool enableRotationAndCurve = true;
    
    public bool enableDrag = true; // Возможность перемещать карты ПКМ
    public bool enableOutlineWhileDragging = true;
    public bool ignoreActionsCherker = false;
    public int plusOrder = 50;
    [SerializeField] private float swapThreshold = 0.5f;
    private int reservedSlots = 0;
    public List<Card> cards { get; private set; } = new List<Card>();
    private Card draggedCard;
    private bool isDragging;
    private Func<bool> dragOutChecker;
    private Func<Card, UniTask<bool>> dragOutResult;
    private async UniTask SwapCards(int indexA, int indexB)
    {
        if (indexA < 0 || indexB < 0 || indexA >= cards.Count || indexB >= cards.Count)
            return;

        // Меняем карты местами в списке
        (cards[indexA], cards[indexB]) = (cards[indexB], cards[indexA]);
        
        // Обновляем позиции с анимацией
        await UpdateCardPositions();
    }

    public void Init(float zWidth, Func<bool> funcToCheckDragOut = null, Func<Card, UniTask<bool>> dragOutRes = null)
    {
        zoneWidth = zWidth;
        dragOutChecker = funcToCheckDragOut;
        dragOutResult = dragOutRes;
    }

    public async UniTask AddCard(Card card)
    {
        int targetIndex = reservedSlots > 0 
            ? cards.Take(reservedSlots)
                  .Select((c, i) => c == null ? (int?)i : null)
                  .FirstOrDefault(i => i.HasValue) 
              ?? (cards.Count < reservedSlots ? cards.Count : reservedSlots)
            : cards.Count;

        // Вставляем карту
        if (targetIndex < cards.Count)
            cards[targetIndex] = card;
        else
            cards.Add(card);
        card.zoneHolder = this;
        card.transform.SetParent(transform);
        card.transform.DOScale(Card.CARDSIZE, ActionTime.timeToChangeScaleCard);
        await UpdateCardPositions();
    }
    
    public async UniTask RemoveCard(Card card)
    {
        cards.Remove(card);
        card.zoneHolder = null;
        card.transform.localRotation = Quaternion.identity;
        await UpdateCardPositions();
    }
    public void ReserveSlots(int count)
    {
        reservedSlots = Mathf.Max(0, count);
        _ = UpdateCardPositions();
    }

    
    private async UniTask UpdateCardPositions(bool animate = true)
    {
        UniTask task = UniTask.CompletedTask;
        
        for (int i = 0; i < cards.Count; i++)
        {
            int index = i;
            
            // Пропускаем анимацию для перетаскиваемой карты
            if (cards[index] == draggedCard) 
            {
                continue;
            }
            CalculateCardPosition(index, out Vector3 position, out float rotation, out Vector3 scale);

            if (animate)
            {
                task = cards[index].MoveTo(new Vector3(position.x, 0, -1) + transform.position, rotation);
                if(enableRotationAndCurve) cards[index].visual.transform.DOLocalMoveY(position.y, 0.3f);
                bool aLot = ((cards.Count - 1) * Card.CARDPIXELSIZE.x * Card.CARDSIZE / 2f) > zoneWidth / 2;
                float delCard = aLot ? zoneWidth / (cards.Count - 1) : Card.CARDPIXELSIZE.x * Card.CARDSIZE;
                cards[i].Collider.size =
                    new Vector2(delCard / Card.CARDSIZE, Card.CARDPIXELSIZE.y);
                cards[index].visual.sortingOrder = plusOrder + index;
            }
            else
            {
                cards[index].transform.position = new Vector3(position.x, 0, -1) + transform.position;
                //cards[index].transform.localEulerAngles = new Vector3(0, 0, rotation);
            }
        }
        
        await UniTask.WhenAll(task);
    }
    
    private void CalculateCardPosition(int index, out Vector3 position, out float rotation, out Vector3 scale)
    {
        int totalSlots = Mathf.Max(cards.Count, reservedSlots);
        
        // Рассчитываем базовые параметры
        float spacing = CalculateSpacing();
        float startX = -((totalSlots - 1) * spacing) / 2f;
        
        // Позиция
        float xPos = startX + index * spacing;
        float yPos = enableRotationAndCurve ? CalculateArcHeight(index) : 0;
        position = new Vector3(xPos, yPos, transform.position.z);
        
        // Поворот
        float rotationAngle = enableRotationAndCurve ? CalculateRotationAngle(index) : 0;
        rotation = rotationAngle;
        
        // Масштаб
        scale = Vector3.one * (Card.CARDSIZE);
    }
    
    private float CalculateSpacing()
    {
        int totalSlots = Mathf.Max(cards.Count, reservedSlots);
        if (totalSlots <= 1) return 0;
        
        // Рассчитываем оптимальный интервал между картами
        float d = 1.1f;
        float requiredSpace = (totalSlots - 1) * (Card.CARDPIXELSIZE.x * Card.CARDSIZE) * d * transform.lossyScale.x;
        float maxSpacing = Card.CARDPIXELSIZE.x * d * Card.CARDSIZE; // Максимальный интервал
        
        if (requiredSpace <= zoneWidth)
        {
            // Если все карты помещаются без растяжения
            return Card.CARDPIXELSIZE.x * Card.CARDSIZE * transform.lossyScale.x * d;
        }
        else
        {
            // Если карт слишком много - равномерно распределяем
            return Mathf.Min(zoneWidth / (totalSlots - 1), maxSpacing) * d;
        }
    }
    
    private float CalculateArcHeight(int index)
    {
        float delta = positioning.Evaluate(GetNormalPosition(cards[index])) * (cards.Count * positioningInfluence);
        float curveYOffset = cards.Count < 5 ? 0 : delta;

        return curveYOffset;
    }
    
    private float CalculateRotationAngle(int index)
    {
        float t = rotation.Evaluate(GetNormalPosition(cards[index])) * (rotationInfluence * cards.Count);
        if(float.IsNaN(t)) t = 0;
        return t;
    }
    
    private void Update()
    {
        HandleRightClickDrag();
        HandleCardSwapping();
    }
    
    private async UniTaskVoid HandleRightClickDrag()
    {
        if (!enableDrag) return;
        
        // Начало перетаскивания
        if (Input.GetMouseButtonDown(0) && (G.Main.ActionChecker.Check() || ignoreActionsCherker))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main!.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider && hit.collider.GetComponent<Card>() && ContainsCard(hit.collider.GetComponent<Card>()))
            {
                draggedCard = hit.collider.GetComponent<Card>();
                isDragging = true;
                draggedCard.OnPointerExit(null);
                G.Main.ActionChecker.isDraggingCard = true;
                draggedCard.RemoveLit();
                // Поднимаем карту над другими
                draggedCard.visual.sortingOrder = plusOrder + 500;
                //draggedCard.visual.transform.DOScale(1.35f, ActionTime.timeToChangeScaleCard / 2);
            }
        }
        
        // Процесс перетаскивания
        if (Input.GetMouseButton(0) && draggedCard)
        {
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _ = draggedCard.MoveTo(targetPosition, 0);
            
            if (dragOutChecker != null && dragOutChecker.Invoke())
            {
                if(enableOutlineWhileDragging) draggedCard.AddOutline(Color.green);
            }
            else
            {
                if(enableOutlineWhileDragging) draggedCard.AddOutline(new Color(256f/256,125f/256,0));
            }
        }
        
        // Конец перетаскивания
        if (Input.GetMouseButtonUp(0) && draggedCard)
        {
            G.Main.ActionChecker.isDraggingCard = false;
            
            if (dragOutChecker != null && dragOutChecker.Invoke())
            {
                if (dragOutResult != null)
                {
                    Card c = draggedCard;
                    draggedCard.visual.transform.DOScale(1f, ActionTime.timeToChangeScaleCard / 2);
                    if(enableOutlineWhileDragging) draggedCard.RemoveOutline();
                    draggedCard = null;
                    isDragging = false;
                    await dragOutResult(c);
                    _ = UpdateCardPositions();
                    return;
                }
                
            }
            
            draggedCard.visual.transform.DOScale(1f, ActionTime.timeToChangeScaleCard / 2);
            draggedCard.visual.sortingOrder = plusOrder + cards.IndexOf(draggedCard);
            if(enableOutlineWhileDragging) draggedCard.RemoveOutline();
            draggedCard.RestoreLit();
            draggedCard = null;
            isDragging = false;
            
            _ = UpdateCardPositions();
        }
    }
    
    private void HandleCardSwapping()
    {
        if (!isDragging || draggedCard == null) return;
    
        // Получаем текущую позицию карты в локальных координатах
        Vector3 localPos = transform.InverseTransformPoint(draggedCard.transform.position);
    
        // Рассчитываем текущий "виртуальный" индекс на основе позиции
        float spacing = CalculateSpacing();
        float startX = -((cards.Count - 1) * spacing) / 2f;
        float virtualIndex = (localPos.x * transform.localScale.x - startX) / (spacing);
        // Ограничиваем в пределах допустимых индексов
        virtualIndex = Mathf.Clamp(virtualIndex, 0, cards.Count - 1);
        // Текущий индекс перетаскиваемой карты
        int currentIndex = cards.IndexOf(draggedCard);
    
        // Определяем направление движения
        float direction = Mathf.Sign(virtualIndex - currentIndex);
    
        // Проверяем нужно ли сделать свап только с соседней картой
        if (Mathf.Abs(virtualIndex - currentIndex) > swapThreshold)
        {
            int newIndex = currentIndex + (int)direction; // Меняем только на соседний индекс
            newIndex = Mathf.Clamp(newIndex, 0, cards.Count - 1);
        
            if (newIndex != currentIndex)
            {
                // Выполняем свап
                _ = SwapCards(currentIndex, newIndex);
            }
        }
    }

    public int GetSortingOrder(Card ent)
    {
        List<Card> temp = cards.ToList();
        for (int i = 0; i < temp.Count; i++)
        {
            if (cards[i] == ent)
            {
                return i + plusOrder;
            }
        }

        return -1;
    }

    public float GetDeltaY(Card entity)
    {
        List<Card> temp = cards.ToList();
        for (int i = 0; i < temp.Count; i++)
        {
            if (cards[i] == entity)
            {
                return CalculateArcHeight(i);
            }
        }
        return -1;
    }
    // Вспомогательные методы
    public int GetCardCount() => cards.Count;
    public bool ContainsCard(Card card) => cards.Contains(card);
    
    private AnimationCurve positioning = new AnimationCurve
    (
        new Keyframe(0f, 0f), 
        new Keyframe(0.08333334f, 0.4043479f), 
        new Keyframe(0.1666667f, 0.6431358f), 
        new Keyframe(0.25f, 0.8037757f), 
        new Keyframe(0.3333333f, 0.9118787f), 
        new Keyframe(0.4166667f, 0.9769853f), 
        new Keyframe(0.5f, 1f), 
        new Keyframe(0.5833333f, 0.9767235f), 
        new Keyframe(0.6666667f, 0.9109904f), 
        new Keyframe(0.75f, 0.8020976f), 
        new Keyframe(0.8333333f, 0.6407319f), 
        new Keyframe(0.9166667f, 0.4017494f), 
        new Keyframe(1f, 0f)
    );
    private float positioningInfluence = .025f;
    private AnimationCurve rotation = new AnimationCurve(
        new Keyframe(0f, 1f), // Время 0, значение 10
        new Keyframe(1f, -1f) // Время 0.33, значение 0.5
    );
    private float rotationInfluence = 0.25f;
    private float GetNormalPosition(Card card)
    {
        int totalSlots = Mathf.Max(cards.Count, reservedSlots);
        float normalizedPos = 0;
        
        for (float i = 0; i < cards.Count; i++)
        {
            if (cards[(int)i] == card)
            {
                normalizedPos = i / (totalSlots - 1);
                break;
            }
        }

        return normalizedPos;
    }

    public Card GetCardById(int index)
    {
        return cards[index];
    }
}