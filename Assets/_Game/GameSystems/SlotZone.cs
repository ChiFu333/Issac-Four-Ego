using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class SlotZone : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private float zoneWidth = 6.5f; // Ширина зоны

    [Header("Interactions")] 
    [SerializeField] private bool enableRotationAndCurve = true;
    public bool enableRightClickDrag = true; // Возможность перемещать карты ПКМ
    [SerializeField] private float swapThreshold = 0.5f;

    public List<Entity> cards { get; private set; } = new List<Entity>();
    private Entity draggedCard;
    private bool isDragging;

    private async UniTask SwapCards(int indexA, int indexB)
    {
        if (indexA < 0 || indexB < 0 || indexA >= cards.Count || indexB >= cards.Count)
            return;

        // Меняем карты местами в списке
        (cards[indexA], cards[indexB]) = (cards[indexB], cards[indexA]);
        
        // Обновляем позиции с анимацией
        await UpdateCardPositions();
    }

    public void Init(float zWidth)
    {
        zoneWidth = zWidth;
    }

    public async UniTask AddCard(Entity card)
    {
        cards.Add(card);
        card.transform.SetParent(transform);
        await UpdateCardPositions();
    }
    
    public async UniTask RemoveCard(Entity card)
    {
        cards.Remove(card);
        card.transform.localRotation = Quaternion.identity;
        await UpdateCardPositions();
    }
    
    private async UniTask UpdateCardPositions(bool animate = true)
    {
        List<bool> completionFlags = new List<bool>();
        for (int i = 0; i < cards.Count; i++) completionFlags.Add(false);
        
        for (int i = 0; i < cards.Count; i++)
        {
            int index = i;
            
            // Пропускаем анимацию для перетаскиваемой карты
            if (cards[index] == draggedCard) 
            {
                completionFlags[index] = true;
                continue;
            }
            CalculateCardPosition(index, out Vector3 position, out float rotation, out Vector3 scale);

            if (animate)
            {
                _ = cards[index].MoveToForHand(new Vector3(position.x, 0, -1) + transform.position, rotation, () => completionFlags[index] = true, false);
                if(enableRotationAndCurve) cards[index].visual.transform.DOLocalMoveY(position.y, 0.3f);
                bool aLot = ((cards.Count - 1) * Entity.CARDPIXELSIZE.x * Entity.CARDSIZE / 2f) > zoneWidth / 2;
                float delCard = aLot ? zoneWidth / (cards.Count - 1) : Entity.CARDPIXELSIZE.x * Entity.CARDSIZE;
                cards[i].Collider.size =
                    new Vector2(delCard / Card.CARDSIZE, Card.CARDPIXELSIZE.y);
                cards[index].visual.render.sortingOrder = 50 + index;
                cards[index].transform.localScale = scale;
            }
            else
            {
                cards[index].transform.position = new Vector3(position.x, 0, -1) + transform.position;
                //cards[index].transform.localEulerAngles = new Vector3(0, 0, rotation);
                cards[index].transform.localScale = scale;
                completionFlags[index] = true;
            }
        }
        
        await WaitForCompletions(completionFlags);
    }
    
    private void CalculateCardPosition(int index, out Vector3 position, out float rotation, out Vector3 scale)
    {
        // Рассчитываем базовые параметры
        float spacing = CalculateSpacing();
        float startX = -((cards.Count - 1) * spacing) / 2f;
        
        // Позиция
        float xPos = startX + index * spacing;
        float yPos = enableRotationAndCurve ? CalculateArcHeight(index) : 0;
        position = new Vector3(xPos, yPos, 0);
        
        // Поворот
        float rotationAngle = enableRotationAndCurve ? CalculateRotationAngle(index) : 0;
        rotation = rotationAngle;
        
        // Масштаб
        scale = Vector3.one * (Entity.CARDSIZE);
    }
    
    private float CalculateSpacing()
    {
        if (cards.Count <= 1) return 0;
        
        // Рассчитываем оптимальный интервал между картами
        float requiredSpace = cards.Count * (Entity.CARDPIXELSIZE.x * Entity.CARDSIZE) * 1.1f;
        float maxSpacing = Entity.CARDPIXELSIZE.x * 1.1f * Entity.CARDSIZE; // Максимальный интервал
        
        if (requiredSpace <= zoneWidth)
        {
            // Если все карты помещаются без растяжения
            return Entity.CARDPIXELSIZE.x * Entity.CARDSIZE * transform.localScale.x * 1.1f;
        }
        else
        {
            // Если карт слишком много - равномерно распределяем
            return Mathf.Min(zoneWidth/ (cards.Count - 1), maxSpacing) * 1.1f;
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
    
    private async UniTask WaitForCompletions(List<bool> flags)
    {
        while (flags.Contains(false))
        {
            await UniTask.Yield();
        }
    }
    
    private void Update()
    {
        HandleRightClickDrag();
        HandleCardSwapping();
    }
    
    private void HandleRightClickDrag()
    {
        if (!enableRightClickDrag) return;
        
        // Начало перетаскивания
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider && hit.collider.GetComponent<Entity>() && ContainsCard(hit.collider.GetComponent<Entity>()))
            {
                draggedCard = hit.collider.GetComponent<Entity>();
                isDragging = true;
                G.isGraggingCard = true;
                
                // Поднимаем карту над другими
                draggedCard.visual.render.sortingOrder = 1000;
            }
        }
        
        // Процесс перетаскивания
        if (Input.GetMouseButton(1) && draggedCard)
        {
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            draggedCard.MoveToForHand(targetPosition, 0 , null, false);
        }
        
        // Конец перетаскивания
        if (Input.GetMouseButtonUp(1) && draggedCard)
        {
            draggedCard.visual.render.sortingOrder = 50 + cards.IndexOf(draggedCard);
            draggedCard = null;
            isDragging = false;
            G.isGraggingCard = false;
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

    public int GetSortingOrder(Entity ent)
    {
        List<Entity> temp = cards.ToList();
        for (int i = 0; i < temp.Count; i++)
        {
            if (cards[i] == ent)
            {
                return i + 50;
            }
        }

        return -1;
    }

    public float GetDeltaY(Entity entity)
    {
        List<Entity> temp = cards.ToList();
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
    public bool ContainsCard(Entity card) => cards.Contains(card);
    
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
    private float GetNormalPosition(Entity card)
    {
        float normalizedPos = 0;
        for (float i = 0; i < cards.Count; i++)
        {
            if (cards[(int)i] == card)
            {
                normalizedPos = i / (cards.Count - 1);
                break;
            }
        }

        return normalizedPos;
    }

    public Entity GetCardById(int index)
    {
        return cards[index];
    }
}