using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectSelector : MonoBehaviour
{
    [SerializeField] private GameObject buttonHolder;
    [SerializeField] private Image cardImage;
    private List<Button> buttons;
    private int result;
    private bool trigger;
    private Ease show = Ease.OutBack;
    private Ease hide = Ease.InBack;
    public void Start()
    {
        buttons = buttonHolder.transform.GetComponentsInChildren<Button>(true).ToList();
        for (int i = 0; i < buttons.Count; i++)
        {
            int temp = i;
            buttons[i].onClick.AddListener(() => ButtonValue(temp));
        }
    }

    public async UniTask<int> SelectEffect(Card card, List<string> labels)
    {
        result = -1;
        trigger = false;
        G.Main.ActionChecker.isSelectingAction = true;
        
        await G.LightController.SetLight(G.LightController.config.DIM_LIGHTOUT);
        
        cardImage.sprite = card.Get<TagSprite>().sprite;
        cardImage.gameObject.SetActive(true);
        cardImage.gameObject.transform.DOScale(Vector3.one, 0.55f).From(Vector3.zero).SetEase(show);
        await SetupButtons(labels);
        await UniTask.WaitUntil(() => trigger);

        UniTask lastT = cardImage.transform.DOScale(Vector3.zero, 0.55f).SetEase(hide)
            .OnUpdate(() =>
            {
                var scale = cardImage.gameObject.transform.localScale;
                if (scale.x < 0 || scale.y < 0 || scale.z < 0)
                {
                    cardImage.gameObject.transform.localScale = Vector3.zero;
                }
            }).AsyncWaitForCompletion().AsUniTask();
        for (int i = 0; i < labels.Count; i++)
        {
            int temp = i;
            G.AudioManager.PlaySound(R.Audio.MouseHoverLoot, -0.075f * i);
            buttons[i].gameObject.transform.DOScale(Vector3.zero, 0.3f).SetEase(hide)
            .OnUpdate(() => 
            {
                var scale = buttons[temp].gameObject.transform.localScale;
                if (scale.x < 0 || scale.y < 0 || scale.z < 0)
                {
                    buttons[temp].gameObject.transform.localScale = Vector3.zero;
                }
            });;
            await UniTask.Delay(75);
        }

        await lastT;
        G.LightController.RestoreLight().Forget();
        G.Main.ActionChecker.isSelectingAction = false;
        return result;
    }

    private async UniTask SetupButtons(List<string> labels)
    {
        foreach (var b in buttons)
        {
            b.gameObject.SetActive(false);
        }
        for (int i = 0; i < labels.Count; i++)
        {
            SetText(buttons[i], labels[i]);
            buttons[i].gameObject.SetActive(i < labels.Count);
            buttons[i].gameObject.transform.DOScale(Vector3.one, 0.3f).From(Vector3.zero).SetEase(show);
            G.AudioManager.PlaySound(R.Audio.MouseHoverLoot, 0.1f * i);
            await UniTask.Delay(75);
        }
    }

    private void SetText(Button button, string text)
    {
        float padding = 50;
        TMP_Text textComponent = button.GetComponentInChildren<TMP_Text>(true);
        textComponent.text = text;

        // Получаем RectTransform кнопки
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        
        float newWidth = textComponent.preferredWidth + padding;
        buttonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        
    }
    public void ButtonValue(int c)
    {
        result = c;
        R.Audio.MyClickSound.PlayAsSoundRandomPitch(0.1f);
        trigger = true;
    }
}
