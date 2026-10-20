using System.Security.Authentication.ExtendedProtection;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    // To use:
    // AudioController.Instance.Play????SFX();
    // Wherever you need a certain sound effect
    private static AudioController _instance;
    public static AudioController Instance { get { return _instance; } }
    [Header("Mixer")]
    [SerializeField] AudioMixer audioMixer;
    
    // https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Audio.AudioMixerGroup.html
    [Header("Audio Sources")]
    [SerializeField] public AudioSource masterSource;
    [SerializeField] public AudioSource musicSource;
    [SerializeField] public AudioSource sfxSource;
    [SerializeField] public AudioSource loopedSFXSource;

    private AudioMixerGroup masterGroup;
    private AudioMixerGroup musicGroup;
    private AudioMixerGroup sfxGroup;

    // TODO: Replace with all the actual sound effects, split by category (you can add more categories/change them)
    [Header("Player SFX")]
    [SerializeField] public AudioClip walkSFX;
    [SerializeField] public AudioClip jumpSFX;
    [SerializeField] public AudioClip twirlSFX;
    [SerializeField] public AudioClip hurtSFX;
    [Header("Environment SFX")]
    [SerializeField] public AudioClip swingingStarSFX;
    [Header("Background Tracks")]
    [SerializeField] public AudioClip tragedyBGMusic;

    void Awake()
    {
        if (_instance != null && _instance!= this)
        {
            Destroy(gameObject);
            return; 
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject); // Controller persists between scenes
    }

    void Start()
    {
        masterGroup = audioMixer.FindMatchingGroups("Master")[0];
        masterSource.outputAudioMixerGroup = masterGroup;

        musicGroup = audioMixer.FindMatchingGroups("Music")[0];
        musicSource.outputAudioMixerGroup = musicGroup;

        sfxGroup = audioMixer.FindMatchingGroups("SFX")[0];
        sfxSource.outputAudioMixerGroup = sfxGroup;
        loopedSFXSource.outputAudioMixerGroup = sfxGroup;

        SetMasterVolume(1.0f);
        SetMusicVolume(1.0f);
        SetSFXVolume(1.0f);
    }

    public void SetMasterVolume(float volume)
    {
        // Set the volume of the Music group in the AudioMixer
        // Volume needs to be exposed in the AudioMixer
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        // Set the volume of the Music group in the AudioMixer
        // Volume needs to be exposed in the AudioMixer
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        // Set the volume of the SFX group in the AudioMixer
        // Volume needs to be exposed in the AudioMixer
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    // TODO: Replace the function names and SFX with the correct sounds and names
    public void PlayWalkSFX()
    {
        sfxSource.PlayOneShot(walkSFX);
    }

    public void PlayJumpSFX()
    {
        sfxSource.PlayOneShot(jumpSFX);
    }

    public void PlayTwirlSFX()
    {
        loopedSFXSource.clip = twirlSFX;
        loopedSFXSource.Play();
    }
    
    public void StopTwirlSFX()
    {
        loopedSFXSource.Stop();
    }

    public void PlayHurtSFX()
    {
        sfxSource.PlayOneShot(hurtSFX);
    }

    public void PlayTragedyBG()
    {
        musicSource.clip = tragedyBGMusic;
        musicSource.Play();
        musicSource.loop = true;
    }

    public void StopScore()
    {
        musicSource.Stop();
    }
}

