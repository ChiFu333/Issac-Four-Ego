using UnityEngine;
using UnityEngine.UI;

public class SliderSettings : MonoBehaviour
{
    private Slider slider;
    public bool isSound = true;
    public void Start()
    {
        slider = GetComponent<Slider>();
        if(isSound) slider.value = G.AudioManager.soundVolume;
        else slider.value = G.AudioManager.musicVolume;
    }
    public void UpdateMusicVolume()
    {
        G.AudioManager.SetMusicVolume(slider.value);
    }
    public void UpdateSoundVolume()
    {
        G.AudioManager.SetSoundVolume(slider.value);
    }
}
