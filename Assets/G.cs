using UnityEngine;

public static class G 
{
    public static LocSystem LocSystem;
    public static AudioManager AudioManager;
    public static SceneLoader SceneLoader;

    public static LightController LightController;
    public static GameMain Main;
}

[DefaultExecutionOrder(-9999)]
public static class GameBootstrapper
{
    private static bool _initialized = false;
    private static GameObject serviceHolder;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        if (_initialized) return;

        #if !UNITY_EDITOR
                    CMS.Init();
        #endif
        R.InitAll();
        
        serviceHolder = new GameObject("===Services==="); 
        Object.DontDestroyOnLoad(serviceHolder);
        
        G.AudioManager = CreateSimpleService<AudioManager>();
        G.LocSystem = CreateSimpleService<LocSystem>();
        G.SceneLoader = CreateSimpleService<SceneLoader>();
        G.LightController = CreateSimpleService<LightController>();
        G.Main = Object.FindFirstObjectByType<GameMain>();
        
        G.SceneLoader.onLoadAction = () =>
        {
            G.LightController.SetupLight();
            G.Main = Object.FindFirstObjectByType<GameMain>();
            Debug.Log(G.Main);
        };
    }
    private static T CreateSimpleService<T>() where T : Component, IService
    {
        GameObject g = new GameObject(typeof(T).ToString());
        
        g.transform.parent = serviceHolder.transform;
        T t = g.AddComponent<T>();
        t.Init();
        return g.GetComponent<T>();
    }
}
public interface IService
{
    public void Init();
}