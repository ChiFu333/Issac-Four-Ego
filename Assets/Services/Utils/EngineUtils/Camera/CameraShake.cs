using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    [Header("Настройки тряски")]
    [SerializeField] private float duration = 0.3f;     // Общая длительность
    [SerializeField] private float strength = 0.2f;     // Сила сдвига (меньше = микро-тряска)
    [SerializeField] private int vibrato = 40;          // Частота дрожания (высокая!)
    [SerializeField] private float randomness = 90f;     // Полная случайность направлений
    [SerializeField] private bool snapping = false;      // Плавное движение (не к целым пикселям)

    private Vector3 _originalPos;
    private Transform _cameraTransform;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        _originalPos = _cameraTransform.localPosition;
    }

    public void Shake(float force = 1)
    {
        // Сброс предыдущей тряски
        _cameraTransform.DOKill();
        _cameraTransform.localPosition = _originalPos;

        // Настройки как в Undertale:
        _cameraTransform.DOShakePosition(
            duration,
            strength * force,
            vibrato,
            randomness,
            snapping,
            fadeOut: true
        ).SetEase(Ease.OutQuad); // Линейное затухание для резкости
    }
}