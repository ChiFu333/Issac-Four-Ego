using System;
using System.Collections.Generic;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class StackVisual : MonoBehaviour
{
    private static readonly Vector3 CARD_ROTATION = new Vector3(40f, 0f, -45f);
    private const float DELTA_VERTICAL = 0.85f;
    private const int sortingCount = 100;
    private TagStackUnitIcons config;

    private void Awake()
    {
        config = CMS.GetOnlyOneComponent<TagStackUnitIcons>();
    }

    public async UniTask SortVisual()
    {
        List<UniTask> tasks = new List<UniTask>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 placeTarget = new Vector3(transform.position.x, transform.position.y + i * DELTA_VERTICAL, transform.position.z);
            if (transform.GetChild(i).GetComponent<CubeVisual>() != null)
            {
                placeTarget += new Vector3(0, -0.11f, 0);
            }
            
            if (transform.GetChild(i).transform.position != placeTarget)
            {
                tasks.Add(transform.GetChild(i).DOMove(placeTarget, 0.3f).SetEase(Ease.OutBack).AsyncWaitForCompletion().AsUniTask());
                if (transform.GetChild(i).GetComponent<CubeVisual>() != null)
                {
                    transform.GetChild(i).GetComponent<CubeVisual>().Visual.sortingOrder = i + sortingCount;
                }
                else if (transform.GetChild(i).GetComponent<Card>() != null)
                {
                    transform.GetChild(i).GetComponent<Card>().visual.sortingOrder = i + sortingCount;
                    transform.GetChild(i).GetComponentInChildren<StackUnitIconVisual>().ChangeOrder(i + sortingCount);
                }
            }
        }
        await UniTask.WhenAll(tasks);
    }
    public async UniTask PutLootCard(Card lootCard)
    {
        int childrenCount = transform.childCount;
        Vector3 placeTarget = new Vector3(transform.position.x, transform.position.y + childrenCount * DELTA_VERTICAL,
            transform.position.z);
        lootCard.transform.parent = transform;
        lootCard.visual.sortingOrder = sortingCount + childrenCount;
        lootCard.visual.transform.DORotate(CARD_ROTATION, 0.3f);
        lootCard.transform.DOScale(transform.lossyScale * Card.CARDSIZE, ActionTime.timeToChangeScaleCard/2);
        await lootCard.MoveTo(placeTarget, CARD_ROTATION.z);
        await lootCard.gameObject.GetComponentInChildren<StackUnitIconVisual>().SetupIcon(config.lootplayEffect, childrenCount + sortingCount);
    }
    public async UniTask PutNearStack(Card lootCard)
    {
        lootCard.gameObject.GetComponentInChildren<StackUnitIconVisual>().RemoveIcon();
        lootCard.transform.parent = null;
        Vector3 placeTarget = new Vector3(transform.position.x - 2f, transform.position.y  * DELTA_VERTICAL, transform.position.z);
        lootCard.visual.transform.DORotate(new Vector3(0,0,0), 0.3f);
        await lootCard.MoveTo(placeTarget, 0);
    }
    public async UniTask<CubeVisual> CreateAndPutCubeView(int value, bool isReady)
    {
        int childrenCount = transform.childCount;
        Vector3 placeTarget = new Vector3(transform.position.x, transform.position.y + childrenCount * DELTA_VERTICAL - 0.11f,
            transform.position.z);
        CubeVisual cv = Instantiate(CMS.GetOnlyOneComponent<DeckListConfig>().CubeView,placeTarget, Quaternion.identity).GetComponent<CubeVisual>();
        cv.transform.localScale = Vector3.zero;
        cv.transform.parent = transform;
        cv.Visual.sortingOrder = sortingCount + childrenCount;
        cv.SetValue(value, isReady);
        await cv.transform.DOScale(Vector3.one * 0.13f, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        
        return cv;
    }
    public async UniTask RemoveCubeView(CubeVisual cv)
    {
        await cv.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        Destroy(cv.gameObject);
        await UniTask.Delay(100);
    }

    public async UniTask<Card> CreateTriggeredEffectCard(Card soucre, StackUnitTriggeredEffect su, StackUnit suSource)
    {
        int childrenCount = transform.childCount;
        Vector3 placeTarget = new Vector3(transform.position.x, transform.position.y + childrenCount * DELTA_VERTICAL,
            transform.position.z);

        Card templeCard = soucre.CloneCard();
        templeCard.transform.position = soucre.transform.position;
        templeCard.transform.parent = transform;
        templeCard.SetActive(true);
        templeCard.HideUI();
        templeCard.transform.localScale = Vector3.zero;
        templeCard.visual.sortingOrder = sortingCount + childrenCount;
        templeCard.Get<TagCardType>().cloneType = su.type;
        if (su.type == StackCardUnitType.ActivateValue)
        {
            templeCard.visual.color = new Color(1f, 0.4f, 0f);
        }
        else
        {
            templeCard.visual.color = new Color(0.36f, 0.64f, 0.61f);
        }
        
        await templeCard.transform.DOScale(Card.CARDSIZE, 0.3f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        await UniTask.Delay(200);
        
        templeCard.visual.transform.DORotate(CARD_ROTATION, 0.3f);
        await templeCard.MoveTo(placeTarget, CARD_ROTATION.z);
        if (su.valueToPlay != -1)
        {
            await templeCard.gameObject.GetComponentInChildren<StackUnitIconVisual>().SetupIcon(config.cubeEffects[(su as StackUnitTriggeredEffect)!.valueToPlay - 1], childrenCount + sortingCount);
        }
        else if (su.type == StackCardUnitType.ActivateValue)
        {
            await templeCard.gameObject.GetComponentInChildren<StackUnitIconVisual>().SetupIcon(config.activateEffect, childrenCount + sortingCount);
        }
        else if(su.type == StackCardUnitType.TriggeredEffect)
        {
            await templeCard.gameObject.GetComponentInChildren<StackUnitIconVisual>().SetupIcon(config.triggeredEffect, childrenCount + sortingCount);
        }
        else
        {
            await templeCard.gameObject.GetComponentInChildren<StackUnitIconVisual>().SetupIcon(config.cubeEffects[su.type.GetIntByStackCardUnitType()-1], childrenCount + sortingCount);
        }
        return templeCard;
    }

    public async UniTask RemoveTriggeredEffectCard(Card soucre)
    {
        await soucre.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        Destroy(soucre.gameObject);
        await UniTask.Delay(200);
    }
}
