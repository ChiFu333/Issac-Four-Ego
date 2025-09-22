using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField] private Transform startPoint; // Точка начала (карта)
    [SerializeField] private Vector3 endPoint;   // Точка конца (курсор или другая карта)
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject arrowHead; // Спрайт наконечника стрелки
    [SerializeField] private GameObject startDot;
    
    private bool isActive = false;
    private Vector3 controlPoint;
    private float curveHeight = 5f;

    private void Update()
    {
        if (!isActive) return;
        
        UpdateArrowPosition();
    }

    public void ActivateArrow(Transform start)
    {
        G.LightController.SetLight(G.LightController.config.NORMAL_LIGHTOUT).Forget();
        startPoint = start;
        UpdateArrowPosition();
        isActive = true;
        lineRenderer.enabled = true;
        arrowHead.SetActive(true);
        startDot.SetActive(true);
    }

    public void DeactivateArrow()
    {
        G.LightController.RestoreLight().Forget();
        isActive = false;
        lineRenderer.enabled = false;
        arrowHead.SetActive(false);
        startDot.SetActive(false);
    }

    private void UpdateArrowPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        endPoint = Camera.main.ScreenToWorldPoint(mousePos);
        endPoint.z = 0;
        
        arrowHead.transform.position = endPoint;
        startDot.transform.position = startPoint.position;
        
        // Рассчитываем контрольную точку для кривой (середина + высота)
        Vector3 middle = (startPoint.position + endPoint) / 2;
        controlPoint = middle + new Vector3(0, GetSmoothCurveHeight(startPoint.position, endPoint, 1, curveHeight, 18), 0);
        
        // Обновляем точки LineRenderer
        int segments = 40; // Количество сегментов кривой
        lineRenderer.positionCount = segments + 1;
        
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)(segments);
            Vector3 point = CalculateBezierPoint(t, startPoint.position, controlPoint, endPoint);
            lineRenderer.SetPosition(i, point);
        }
        
        // Обновляем наконечник стрелки
        //endPoint += deltaDir.normalized * -0.25f;
        //lineRenderer.SetPosition(15,endPoint);
        Vector3 direction = endPoint - lineRenderer.GetPosition(segments - 2);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowHead.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // Формула квадратичной кривой Безье
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 point = uu * p0; // (1-t)^2 * p0
        point += 2 * u * t * p1; // 2*(1-t)*t * p1
        point += tt * p2; // t^2 * p2
        
        return point;
    }
    private float GetSmoothCurveHeight(Vector3 start, Vector3 end, float minHeight, float maxHeight, float maxDistance)
    {
        float distance = Vector3.Distance(start, end);
        float t = Mathf.Clamp01(distance / maxDistance);
        return Mathf.SmoothStep(minHeight, maxHeight, t);
    }
}
