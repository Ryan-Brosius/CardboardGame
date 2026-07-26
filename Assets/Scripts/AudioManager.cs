using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip musicClip;

    [Header("Settings")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private readonly List<AudioSource> sfxPool = new List<AudioSource>();
    private const int POOL_SIZE = 5;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSources();

        PlayMusic(musicClip);
    }

    private void InitializeSources()
    {
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;

        // Initialize SFX pool for rapidly repeating overlapping sounds
        for (int i = 0; i < POOL_SIZE; i++)
        {
            AudioSource poolSource = gameObject.AddComponent<AudioSource>();
            sfxPool.Add(poolSource);
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitchJitter = 0f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();

        source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        source.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying) return source;
        }
        return sfxSource;
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1.0f)
    {
        if (clip == null || musicSource.clip == clip) return;

        if (fadeDuration > 0f)
        {
            musicSource.DOFade(0f, fadeDuration * 0.5f).OnComplete(() =>
            {
                musicSource.clip = clip;
                musicSource.Play();
                musicSource.DOFade(musicVolume, fadeDuration * 0.5f);
            });
        }
        else
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void StopMusic(float fadeDuration = 0.5f)
    {
        if (fadeDuration > 0f)
        {
            musicSource.DOFade(0f, fadeDuration).OnComplete(() => musicSource.Stop());
        }
        else
        {
            musicSource.Stop();
        }
    }
}
