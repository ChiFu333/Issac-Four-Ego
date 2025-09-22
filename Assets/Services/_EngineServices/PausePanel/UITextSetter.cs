using TMPro;
using UnityEngine;

public class UITextSetter : MonoBehaviour
{
    public LocString text;
    private TMP_Text _tmp;
    void Start()
    {
        _tmp = GetComponent<TMP_Text>();
        UpdateUI();
    }

    public void UpdateUI()
    {
        _tmp.text = text.ToString();
    }

}
