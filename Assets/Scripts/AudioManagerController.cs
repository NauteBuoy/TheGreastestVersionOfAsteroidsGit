using UnityEngine;
using UnityEngine.Rendering;

public class AudioManagerController : MonoBehaviour
{
    [Header("Reference Settings")]
    public static AudioManagerController audioManagerInstance;

    [Header("Music Settings")]
    public AudioSource musicSource;
    public float musicVolume = 0.6f;

    [Header("Music List")]
    public AudioClip splashMusic;
    public AudioClip gameMusic;

    [Header("SFX Settings")]
    public AudioSource sfxSource;
    public float sfxVolume = 0.4f;
    public float sfxVolumeLoud = 0.8f;

    [Header("SFX List")]
    public AudioClip dischargeSFX;
    public AudioClip collisionSFX;
    public AudioClip explosionSFX;
    public AudioClip shieldHitSFX;
    public AudioClip shieldBreakSFX;
    public AudioClip stateChangeSFX;
    public AudioClip deathSFX;

    [Header("Score SFX")]
    public AudioClip scoreTickSFX;
    public float scoreTickVolume = 0.2f;

    [Header("Thruster Settings")]
    public AudioSource thrusterSource;
    public float thrusterVolume = 0.1f;
    public AudioClip thrusterSFX;



    void Start()
    {
        if (audioManagerInstance)
        {
            Destroy(gameObject);
            return;
        }
        audioManagerInstance = this;
        DontDestroyOnLoad(gameObject);

        if (thrusterSource && thrusterSFX)
        {
            thrusterSource.clip = thrusterSFX;
            thrusterSource.loop = true;
            thrusterSource.volume = thrusterVolume;
            thrusterSource.playOnAwake = false;
        }
    }

    public void PlayMusic(AudioClip clip, float volume, bool loop = true)
    {
        if (!musicSource) 
            return;
        if (!clip)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (!sfxSource)
            return;
        if (!clip) 
            return;

        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayThruster(bool active)
    {
        if (!thrusterSource)
            return;
        if (!thrusterSFX)
            return;

        if (active)
        {
            if (!thrusterSource.isPlaying)
            {
                thrusterSource.Play();
            }
        }
        else
        {
            if (thrusterSource.isPlaying)
            {
                thrusterSource.Stop();
            }
        }
    }

    public void PlayStateChange()
    {
        if (!stateChangeSFX)
            return;

        sfxSource.PlayOneShot(stateChangeSFX, sfxVolume);
    }

    public void PlayScoreTick()
    {
        if (!scoreTickSFX)
            return;

        sfxSource.PlayOneShot(scoreTickSFX, scoreTickVolume);
    }
}
