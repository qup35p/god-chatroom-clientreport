using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundClip
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.5f, 2f)]
    public float pitch = 1f;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("Sound Effects")]
    public List<SoundClip> soundClips = new List<SoundClip>();

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float uiVolume = 1f;

    [Header("Fade Settings")]
    public float fadeSpeed = 1f;

    private Dictionary<string, SoundClip> soundDictionary;
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 單例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioManager()
    {
        // 建立音效字典
        soundDictionary = new Dictionary<string, SoundClip>();
        foreach (SoundClip sound in soundClips)
        {
            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound);
            }
        }

        // 設置音源
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        if (uiSource == null)
        {
            uiSource = gameObject.AddComponent<AudioSource>();
        }

        // 開始播放背景音樂
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
    }

    private void Start()
    {
        // 設置初始音量
        UpdateAllVolumes();
    }

    public void PlaySound(string soundName, AudioSourceType sourceType = AudioSourceType.SFX)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            SoundClip sound = soundDictionary[soundName];
            AudioSource targetSource = GetAudioSource(sourceType);

            if (targetSource != null && sound.clip != null)
            {
                targetSource.pitch = sound.pitch;

                if (sound.loop)
                {
                    targetSource.clip = sound.clip;
                    targetSource.volume = sound.volume * GetVolumeMultiplier(sourceType) * masterVolume;
                    targetSource.loop = true;
                    targetSource.Play();
                }
                else
                {
                    targetSource.PlayOneShot(sound.clip, sound.volume * GetVolumeMultiplier(sourceType) * masterVolume);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in AudioManager!");
        }
    }

    public void PlaySoundOneShot(AudioClip clip, float volume = 1f, AudioSourceType sourceType = AudioSourceType.SFX)
    {
        if (clip != null)
        {
            AudioSource targetSource = GetAudioSource(sourceType);
            if (targetSource != null)
            {
                targetSource.PlayOneShot(clip, volume * GetVolumeMultiplier(sourceType) * masterVolume);
            }
        }
    }

    public void StopSound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            SoundClip sound = soundDictionary[soundName];

            // 檢查所有音源是否正在播放此音效
            if (musicSource.clip == sound.clip && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
            if (sfxSource.clip == sound.clip && sfxSource.isPlaying)
            {
                sfxSource.Stop();
            }
            if (uiSource.clip == sound.clip && uiSource.isPlaying)
            {
                uiSource.Stop();
            }
        }
    }

    public void StopAllSounds()
    {
        musicSource.Stop();
        sfxSource.Stop();
        uiSource.Stop();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void FadeOutMusic(float duration = 2f)
    {
        if (musicSource.isPlaying)
        {
            StartCoroutine(FadeAudioSource(musicSource, 0f, duration));
        }
    }

    public void FadeInMusic(float duration = 2f, float targetVolume = -1f)
    {
        if (targetVolume < 0f)
        {
            targetVolume = musicVolume * masterVolume;
        }

        if (musicSource != null)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.volume = 0f;
                musicSource.Play();
            }
            StartCoroutine(FadeAudioSource(musicSource, targetVolume, duration));
        }
    }

    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        source.volume = targetVolume;

        if (targetVolume <= 0f)
        {
            source.Stop();
        }
    }

    private AudioSource GetAudioSource(AudioSourceType sourceType)
    {
        switch (sourceType)
        {
            case AudioSourceType.Music:
                return musicSource;
            case AudioSourceType.SFX:
                return sfxSource;
            case AudioSourceType.UI:
                return uiSource;
            default:
                return sfxSource;
        }
    }

    private float GetVolumeMultiplier(AudioSourceType sourceType)
    {
        switch (sourceType)
        {
            case AudioSourceType.Music:
                return musicVolume;
            case AudioSourceType.SFX:
                return sfxVolume;
            case AudioSourceType.UI:
                return uiVolume;
            default:
                return sfxVolume;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
    }

    private void UpdateAllVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    // 遊戲特定音效方法
    public void PlayButtonClick()
    {
        PlaySound("button_click", AudioSourceType.UI);
    }

    public void PlaySubmitSound()
    {
        PlaySound("submit", AudioSourceType.UI);
    }

    public void PlayNotificationSound()
    {
        PlaySound("notification", AudioSourceType.UI);
    }

    public void PlayTimerWarning()
    {
        PlaySound("timer_warning", AudioSourceType.SFX);
    }

    public void PlayGameEnd()
    {
        PlaySound("game_end", AudioSourceType.SFX);
    }

    // 在遊戲結束時調用
    public void OnGameEnd()
    {
        PlayGameEnd();
        FadeOutMusic(3f);
    }

    // 添加新音效到字典
    public void AddSound(string name, AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        SoundClip newSound = new SoundClip
        {
            name = name,
            clip = clip,
            volume = volume,
            pitch = pitch,
            loop = loop
        };

        if (!soundDictionary.ContainsKey(name))
        {
            soundDictionary.Add(name, newSound);
            soundClips.Add(newSound);
        }
    }

    // 檢查音效是否存在
    public bool HasSound(string soundName)
    {
        return soundDictionary.ContainsKey(soundName);
    }

    // 獲取當前播放狀態
    public bool IsSoundPlaying(AudioSourceType sourceType)
    {
        AudioSource source = GetAudioSource(sourceType);
        return source != null && source.isPlaying;
    }
}

public enum AudioSourceType
{
    Music,
    SFX,
    UI
}