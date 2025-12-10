using UnityEngine;
using UnityEngine.Rendering;

public class AudioManagerController : MonoBehaviour
{
    public static AudioManagerController Instance;


    public AudioSource musicAudioSource;
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public float menuMusicVolume = 0.6f;
    public float gameMusicVolume = 0.4f;

    [Header("SFX Settings")]
    public AudioSource sfxAudioSource;
    public AudioClip bulletSFX;
    public AudioClip explosionSFX;
    public AudioClip collisionSFX;
    public AudioClip shipDeathSFX;
    public float collisionVolume = 0.3f; 
    public float normalCollisionVolume = 1f;   

    [Header("Thruster Settings")]
    public AudioSource thrusterAudioSource;
    public AudioClip thrusterSFX;

    void Start()
    {
        thrusterAudioSource.clip = thrusterSFX;
        thrusterAudioSource.loop = true;
        thrusterAudioSource.playOnAwake = false;


        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        
    }

    public void PlayMusic(AudioClip clip, float volume, bool loop = true)
    {
        if (!clip) 
            return;

        musicAudioSource.clip = clip;
        musicAudioSource.loop = loop;
        musicAudioSource.volume = volume;
        musicAudioSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (!clip) 
            return;

        musicAudioSource.PlayOneShot(clip, volume);
    }

    public void PlayThruster(bool active)
    {
        if (active)
        {
            if (!thrusterAudioSource.isPlaying)
            {
                thrusterAudioSource.Play();
            }
        }
        else
        {
            if (thrusterAudioSource.isPlaying)
            {
                thrusterAudioSource.Stop();
            }
        }
    }
}
