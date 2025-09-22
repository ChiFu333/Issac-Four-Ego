using UnityEngine;
using DG.Tweening;
public static class Extensions
{
    public static void ShowUp(GameObject obj)
    {
        Vector3 templeScale = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;
        obj.transform.DOScale(templeScale, 0.75f)
            .SetEase(Ease.OutElastic, 1.1f, 0.5f);
    }
}
