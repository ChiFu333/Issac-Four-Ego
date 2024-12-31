using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    public static Console Inst;
    private static TMP_Text text;
    public void Awake()
    {
        Inst = this;
        text = GetComponentInChildren<TMP_Text>();
    }
    public static void WriteText(string t)
    {
        text.text = t;
    }
}
