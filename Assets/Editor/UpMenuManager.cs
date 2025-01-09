#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
public class UpMenuManager : MonoBehaviour
{
    
    [MenuItem("Initiation/!!InitAll!!")]
    private static void InitAlll()
    {
        CreateAllFolders();
        LoadFromInternetPlugins();
        LoadPackagesFromDisk();
        CreateNewScene();
    }
    #region Initiation
    [MenuItem("Initiation/Create folders for project")]
    private static void CreateAllFolders()
    {
        var firstF = new List<string>
        {
            "Engine",
            "Fonts",
            "Game",
            "Utils"
        };
        foreach (var folder in firstF.Where(folder => !Directory.Exists("Assets/" + folder)))
        {
            Directory.CreateDirectory("Assets/" + folder);
        }
        var secondF = new List<string>
        {
            "Audio",
            "Prefabs",
            "Scenes",
            "Sprites",
            "Systems"
        };
        foreach (var folder in secondF.Where(folder => !Directory.Exists("Assets/Game/" + folder)))
        {
            Directory.CreateDirectory("Assets/Game/" + folder);
        }

        AssetDatabase.Refresh();
    }
    
    [MenuItem("Initiation/Import custom package")]
    private static void ImportUnityPackage()
    {
        string packagePath = EditorUtility.OpenFilePanel("Select Unity Package", "", "unitypackage");

        if (!string.IsNullOrEmpty(packagePath))
        {
            AssetDatabase.ImportPackage(packagePath, false);
            Debug.Log("Package imported: " + packagePath);
        }
        else
        {
            Debug.LogWarning("No package selected.");
        }
    }
    [MenuItem("Initiation/Load Packages From My Disk")]
    private static void LoadPackagesFromDisk()
    {
        string folderPath = "V:/UnityPackagesPlace";
        string[] packageFiles = Directory.GetFiles(folderPath, "*.unitypackage");

        if (packageFiles.Length == 0)
        {
            Debug.LogWarning("No .unitypackage files found in the selected folder.");
            return;
        }
        foreach (string packageFile in packageFiles)
        {
            Debug.Log("Importing package: " + packageFile);
            AssetDatabase.ImportPackage(packageFile, false);
        }
    }
    static AddRequest Request;
    static int id = 0;
    static List<string> adresses = new List<string>()
    {
        "https://github.com/Team-on/unity3d-rainbow-folders.git#master",
        "https://github.com/smkplus/CustomToolbar.git#master"
    };

    [MenuItem("Initiation/Load From Internet Plugins")]
    private static void LoadFromInternetPlugins()
    {    
        Add(adresses[id]);
    }
    private static void Add(string adress)
    {
        Request = Client.Add(adress);
        EditorApplication.update += Progress;   
    }
    private static void Progress()
    {
        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
                Debug.Log("Installed: " + Request.Result.packageId);
            else if (Request.Status >= StatusCode.Failure)
                Debug.Log(Request.Error.message);

            EditorApplication.update -= Progress;
            id++;
            if(id != adresses.Count) Add(adresses[id]);
        }
    }
    #endregion
    
    [MenuItem("Custom/Create New Scene")]
    public static void CreateNewScene()
    {
        string templatePath = "Assets/Editor/SceneTemplate/MyTemplate.scenetemplate";

        string newScenePath = "Assets/Game/Scenes/RENAME_ME.unity";

        SceneTemplateAsset sceneTemplate = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(templatePath);
        
        if (sceneTemplate == null)
        {
            AssetDatabase.ImportPackage("v:/UnityPackagesPlace/SceneTemplate.unitypackage", false);
            sceneTemplate = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(templatePath);
            if (sceneTemplate == null)
            {
                Debug.LogError("Ошибка в создании сцены, мб просто попробовать снова?");
                return;
            }
        }

        SceneTemplateService.Instantiate(sceneTemplate, false, newScenePath);

        AssetDatabase.Refresh();
    }
}
#endif