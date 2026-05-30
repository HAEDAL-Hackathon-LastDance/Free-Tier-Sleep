using UnityEngine;

public class Phase1AudioManager : MonoBehaviour
{
    public static Phase1AudioManager Instance { get; private set; }

    [Header("BGM")]
    public AudioClip bgmClip;

    [Header("SFX")]
    public AudioClip glitchSfx;
    public AudioClip warningSfx;
    public AudioClip typeSfx;
    public AudioClip systemBeepSfx;
    public AudioClip hoverSfx;
    public AudioClip tvStaticSfx;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var sources = GetComponents<AudioSource>();
        bgmSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayGlitch() => PlaySFX(glitchSfx, 0.8f);
    public void PlayWarning() => PlaySFX(warningSfx, 1f);
    public void PlayType() => PlaySFX(typeSfx, 0.6f);
    public void PlaySystemBeep() => PlaySFX(systemBeepSfx, 0.7f);
    public void PlayHover() => PlaySFX(hoverSfx, 0.5f);
    public void PlayTVStatic() => PlaySFX(tvStaticSfx, 0.4f);
}
