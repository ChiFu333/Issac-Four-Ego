using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour, IService
{
    public UIPanelScaler panel;
    private string _notInMainMenu = "MainMenu";

    public bool inMenu = false;
    public void Init()
    {
        GameObject can = new GameObject("MainMenuCanvas");
        DontDestroyOnLoad(can);
        
        Canvas c = can.AddComponent<Canvas>();
        
        CanvasScaler cs = can.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 1;
        
        can.AddComponent<GraphicRaycaster>();
        c.worldCamera = Camera.main;
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 500;

        GameObject temp = Resources.Load<GameObject>("Services/" + "PauseMenu");
        temp.SetActive(false);
        GameObject g = Instantiate(temp, can.transform);
        panel = g.GetComponent<UIPanelScaler>();
    }

    public void Update()
    {
        if (G.SceneLoader.currentSceneName != _notInMainMenu && Input.GetKeyDown(KeyCode.Escape) && !panel.inAnim)
        {
            
            if (panel.gameObject.activeSelf)
            {
                panel.Close();
                inMenu = false;
            }
            else
            {
                panel.gameObject.SetActive(true);
                inMenu = true;
            }
        }
    }
}
