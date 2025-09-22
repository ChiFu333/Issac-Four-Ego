using UnityEngine;
public class ParallaxEffect : MonoBehaviour
{
    [Header("Настройки параллакса")]
    [SerializeField, Range(0f, 1f)] 
    private float _parallaxStrength = 0.5f; // Сила эффекта (0 = нет эффекта, 1 = полное смещение)
    
    [SerializeField] 
    private bool _useScreenCenter = true; // Отталкиваться от центра экрана
    
    private RectTransform _rectTransform;
    private Vector2 _initialPosition;
    private Vector2 _screenCenter;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _initialPosition = _rectTransform.anchoredPosition;
        
        if (_useScreenCenter)
        {
            // Получаем центр Canvas (в координатах UI)
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Rect canvasRect = canvas.GetComponent<RectTransform>().rect;
                _screenCenter = new Vector2(canvasRect.width * 0.5f, canvasRect.height * 0.5f);
            }
            else
            {
                Debug.LogWarning("Canvas не найден! Используется центр экрана.");
                _screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }
        }
    }

    private void Update()
    {
        Vector2 inputPosition = Input.mousePosition;
        
        if (_useScreenCenter)
        {
            // Вычисляем разницу от центра экрана
            Vector2 offset = (inputPosition - _screenCenter) * _parallaxStrength;
            _rectTransform.anchoredPosition = _initialPosition - offset;
        }
        else
        {
            // Просто отталкиваемся от позиции мыши
            Vector2 offset = inputPosition * _parallaxStrength;
            _rectTransform.anchoredPosition = _initialPosition - offset;
        }
    }
}