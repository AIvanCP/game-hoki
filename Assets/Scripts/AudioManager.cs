using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip bgmIntro;
    [SerializeField] private AudioClip bgmLoop;
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField] private bool playOnAwake = true;

    private AudioSource introBgmSource;
    private AudioSource loopBgmSource;

    public static AudioManager Instance { get; private set; }

    private bool isTransitioning = false;
    private float introPosition = 0f;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            introBgmSource = gameObject.AddComponent<AudioSource>();
            loopBgmSource = gameObject.AddComponent<AudioSource>();

            introBgmSource.loop = false;
            loopBgmSource.loop = true;

            introBgmSource.volume = bgmVolume;
            loopBgmSource.volume = bgmVolume;

            if (playOnAwake && (bgmIntro != null || bgmLoop != null))
            {
                PlayBGM();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (introBgmSource.volume != bgmVolume)
        {
            introBgmSource.volume = bgmVolume;
            loopBgmSource.volume = bgmVolume;
        }
    }

    public void PlayBGM()
    {
        StopBGM();

        if (bgmIntro != null && bgmLoop != null)
        {
            PlayBGMWithDSP();
        }
        else if (bgmLoop != null)
        {
            loopBgmSource.clip = bgmLoop;
            loopBgmSource.Play();
        }
    }

    private void PlayBGMWithDSP()
    {
        isTransitioning = true;
        introPosition = 0f;

        introBgmSource.clip = bgmIntro;
        loopBgmSource.clip = bgmLoop;

        introBgmSource.Play();

        // Start monitoring the intro playback to seamlessly transition to the loop
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(MonitorIntroPlayback());
    }

    private IEnumerator MonitorIntroPlayback()
    {
        // Wait until almost at the end of the intro clip (20ms before)
        float targetTime = bgmIntro.length - 0.02f;

        while (introBgmSource.isPlaying && introBgmSource.time < targetTime)
        {
            yield return null;
        }

        // Start the loop right at the end of the intro
        loopBgmSource.Play();

        // Wait until intro is fully done
        while (introBgmSource.isPlaying)
        {
            yield return null;
        }

        isTransitioning = false;
    }

    public void StopBGM()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        isTransitioning = false;
        introBgmSource.Stop();
        loopBgmSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        introBgmSource.volume = bgmVolume;
        loopBgmSource.volume = bgmVolume;
    }

    public bool IsBGMPlaying()
    {
        return introBgmSource.isPlaying || loopBgmSource.isPlaying;
    }

    public void SetAndPlayBGM(AudioClip intro, AudioClip loop)
    {
        if (loop == null) return;

        bgmIntro = intro;
        bgmLoop = loop;
        PlayBGM();
    }
}
