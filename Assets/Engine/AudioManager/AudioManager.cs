using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager inst { get; private set; }
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundsSource;
    [SerializeField] private AudioClip musicClip;

    private float musicVolume;
    private float soundVolume;
    private void Awake() {
        if (inst != null && inst != this) {
            Destroy(this);
        } else {
            inst = this;
            DontDestroyOnLoad(gameObject);
            PlayMusic(musicClip);
        }
    }
    public void SetMusicVolume(float value) {
        musicVolume = value;
        musicSource.volume = musicVolume;
    }

    public void SetSoundVolume(float value) {
        soundVolume = value;
        soundsSource.volume = soundVolume;
    }

    public void Play(AudioClip clip) {
        if (clip == null) return;
        soundsSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip) {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }
    public void StopMusic() {
        musicSource.Stop();
    }
}
