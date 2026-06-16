using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameAudioController : MonoBehaviour
{
    private const string BgmAssetPath = "Assets/Audio/Music/BGM.wav";
    private const string CardsSfxAssetPath = "Assets/Audio/Cards.wav";
    private const string CashOutSfxAssetPath = "Assets/Audio/Cashout.wav";

    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip cardsSfxClip;
    [SerializeField] private AudioClip cashOutSfxClip;
    [SerializeField] private float musicVolume = 0.55f;
    [SerializeField] private float sfxVolume = 0.85f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private bool hasInitialized;

    public void Initialize()
    {
        if (hasInitialized)
        {
            return;
        }

        ResolveAudioSources();
        ResolveAudioClips();
        PlayBgm();
        hasInitialized = true;
    }

    public void PlayCardsSfx()
    {
        PlayOneShot(cardsSfxClip, "Cards");
    }

    public void PlayCashOutSfx()
    {
        PlayOneShot(cashOutSfxClip, "Cashout");
    }

    private void ResolveAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length > 0)
        {
            musicSource = sources[0];
        }
        else
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sources.Length > 1)
        {
            sfxSource = sources[1];
        }
        else
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
    }

    private void ResolveAudioClips()
    {
        bgmClip = bgmClip != null ? bgmClip : LoadAudioClip(BgmAssetPath, "BGM");
        cardsSfxClip = cardsSfxClip != null ? cardsSfxClip : LoadAudioClip(CardsSfxAssetPath, "Cards");
        cashOutSfxClip = cashOutSfxClip != null ? cashOutSfxClip : LoadAudioClip(CashOutSfxAssetPath, "Cashout");
    }

    private void PlayBgm()
    {
        if (musicSource == null || bgmClip == null)
        {
            Debug.LogWarning("Cannot play BGM: audio source or BGM clip is missing.");
            return;
        }

        if (musicSource.clip == bgmClip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = bgmClip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
        Debug.Log("BGM started.");
    }

    private void PlayOneShot(AudioClip clip, string label)
    {
        if (sfxSource == null || clip == null)
        {
            Debug.LogWarning($"Cannot play {label} SFX: audio source or clip is missing.");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private AudioClip LoadAudioClip(string assetPath, string label)
    {
#if UNITY_EDITOR
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);

        if (clip == null)
        {
            Debug.LogWarning($"Missing {label} audio clip at {assetPath}");
        }

        return clip;
#else
        Debug.LogWarning($"{label} audio clip is not assigned. Assign it in the inspector for player builds.");
        return null;
#endif
    }
}
