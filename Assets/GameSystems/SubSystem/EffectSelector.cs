using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

public class EffectSelector : MonoBehaviour
{
    public static EffectSelector inst { get; private set; }
    [field: SerializeField] public List<Button> selectButtons { get; private set; } = new List<Button>();
    [field: SerializeField] public Image cardImage { get; private set; }
    private GameObject panel;
    private int selectableId;
    public void Awake()
    { 
        inst = this;
        panel = inst.transform.GetChild(0).gameObject;
    }
    public async Task<int> SelectEffect(Sprite source, int allCount)
    {
        if(allCount == 1 || allCount == 0) return -1;
        cardImage.sprite = source;
        selectableId = -1;
        for(int i = 0; i < selectButtons.Count; i++)
        {
            selectButtons[i].gameObject.SetActive(i < allCount);
        }
        ShowPanel(true);
        while(true)
        {
            await Task.Yield();
            if(selectableId != -1) 
            {
                ShowPanel(false);
                return selectableId;
            }
        }
    }
    private void ShowPanel(bool show)
    {
        panel.SetActive(show);
    }
    public void ButtonValue(int c) => selectableId =  c;
}
