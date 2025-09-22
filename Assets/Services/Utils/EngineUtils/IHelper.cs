using UnityEngine;
[CreateAssetMenu(fileName = "IHelper", menuName = "IHelper")]
public class IHelper : ScriptableObject
{
    public void SetRu()
    {
        G.LocSystem.language = LocSystem.LANG_RU;
        UpdateLan();    
    }

    public void SetEn()
    {
        G.LocSystem.language = LocSystem.LANG_EN;
        UpdateLan();
    }

    private void UpdateLan()
    {
        G.LocSystem.UpdateTexts();
    }

    public void LoadScene(string line)
    {
        _ = G.SceneLoader.Load(line);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void StartMusic(AudioClip clip)
    {
        G.AudioManager.PlayMusic(clip);
    }

    public void PlaySound(AudioClip clip)
    {
        G.AudioManager.PlaySound(clip, 0);
    }
}
