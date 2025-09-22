using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;

public class AudioManager : MonoBehaviour, IService
{
    private AudioSource musicSource;
    private AudioSource soundsSource;

    public float musicVolume { get; private set; } = 0.5f;
    public float soundVolume { get; private set; } = 0.5f;
    private Dictionary<int, ActiveSoundLoop> _activeLoops = new Dictionary<int, ActiveSoundLoop>();
    private int _nextLoopId = 0;
    private class ActiveSoundLoop
    {
        public CancellationTokenSource Cts { get; set; }
    }
    public void Init()
    {
        GameObject mSource = new GameObject("MusicSource") { transform = { parent = transform } };
        musicSource = mSource.AddComponent<AudioSource>();
        musicSource.loop = true;
        
        GameObject sSource = new GameObject("AudioSource") { transform = { parent = transform } };
        soundsSource = sSource.AddComponent<AudioSource>();
        soundsSource.loop = false;
        
        SetMusicVolume(musicVolume);
        SetSoundVolume(soundVolume);
    }
    public void SetMusicVolume(float value) {
        musicVolume = value;
        musicSource.volume = musicVolume;
    }
    public void SetSoundVolume(float value) {
        soundVolume = value;
        soundsSource.volume = soundVolume;
    }
    public void PlayMusic(AudioClip clip) {
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }
    public void StopMusic() {
        musicSource.Stop();
    }
    public void PlaySound(AudioClip clip, float addedPitch) {
        if (clip == null) return;
        GameObject tempAudioObject = new GameObject("TempAudio_" + clip.name);
        DontDestroyOnLoad(tempAudioObject);
        AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = soundVolume;
        audioSource.pitch = 1f + addedPitch;
        audioSource.Play();
        
        Destroy(tempAudioObject, (clip.length / audioSource.pitch) + 0.1f);
    }

    public int PlayLoop(AudioClip clip, float intervalSeconds, float deltaRandomPitch = 0)
    {
        if (clip == null) return -1;

        int id = _nextLoopId++;
        var cts = new CancellationTokenSource();
        
        GameObject loopObject = new GameObject($"SoundLoop_{id}");
        loopObject.transform.parent = transform;

        ActiveSoundLoop loop = new ActiveSoundLoop()
        {
            Cts = cts
        };
        _activeLoops.Add(id, loop);
        SoundLoopTask(clip, intervalSeconds, id, cts.Token).Forget();
        return id;
    }
    private async UniTaskVoid SoundLoopTask(AudioClip clip, float interval, int id, CancellationToken ct, float deltarandomPitch = 0)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                clip.PlayAsSoundRandomPitch(deltarandomPitch);
                
                // Ждем интервал + длительность звука с возможностью отмены
                await UniTask.Delay(
                    (int)((interval + clip.length) * 1000), 
                    DelayType.DeltaTime, 
                    PlayerLoopTiming.Update, 
                    ct);
            }
        }
        catch
        {
            if (_activeLoops.TryGetValue(id, out var loop))
            {
                _activeLoops.Remove(id);
            }
        }
    }
    public void RemoveLoop(int id)
    {
        if (_activeLoops.TryGetValue(id, out ActiveSoundLoop loop))
        {
            loop.Cts?.Cancel();
            loop.Cts?.Dispose();
        }
    }
    public void RemoveAllLoops()
    {
        foreach (var loop in _activeLoops.Values)
        {
            loop.Cts?.Cancel();
            loop.Cts?.Dispose();
        }
        _activeLoops.Clear();
    }
}

public static class AudioExtensions
{
    public static void PlayAsSound(this AudioClip clip, float addedPitch = 0) =>
        G.AudioManager.PlaySound(clip, addedPitch);
    public static void PlayAsMusic(this AudioClip clip) =>
        G.AudioManager.PlayMusic(clip);
    public static void PlayAsSoundRandomPitch(this AudioClip clip, float deltaPitch) =>
        G.AudioManager.PlaySound(clip, Random.Range(-deltaPitch, deltaPitch));
}