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
    [SerializeField] public AudioSource subsfxSource;
    [SerializeField] public AudioSource loopedSFXSource;
    [SerializeField] public AudioSource menumusicSource;
    [SerializeField] public AudioSource gamemusicSource;

    private AudioMixerGroup masterGroup;
    private AudioMixerGroup musicGroup;
    private AudioMixerGroup sfxGroup;
    private AudioMixerGroup subsfxGroup;
    private AudioMixerGroup menumusicGroup;
    private AudioMixerGroup gamemusicGroup;

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;

    // TODO: Replace with all the actual sound effects, split by category (you can add more categories/change them)
    [Header("Player SFX")]
    [SerializeField] public AudioClip walkSFX;
    [SerializeField] public AudioClip jumpSFX;
    [SerializeField] public AudioClip twirlSFX;
    [SerializeField] public AudioClip hurtSFX;
    [Header("UI SFX")]
    [SerializeField] public AudioClip levelselectSFX;
    [SerializeField] public AudioClip uibuttonSFX;
    [SerializeField] public AudioClip curtainopenSFX;
    [SerializeField] public AudioClip curtaincloseSFX;
    [Header("Environment SFX")]
    [SerializeField] public AudioClip swingingStarSFX;
    [Header("Background Tracks")]
    [SerializeField] public AudioClip tragedyBGMusic;
    [SerializeField] public AudioClip comedyBGMusic;
    [SerializeField] public AudioClip menuBGMusic;

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
        
        menumusicGroup = audioMixer.FindMatchingGroups("Menu Music")[0];
        menumusicSource.outputAudioMixerGroup = menumusicGroup;
        
        gamemusicGroup = audioMixer.FindMatchingGroups("Music")[0];
        gamemusicSource.outputAudioMixerGroup = gamemusicGroup;

        sfxGroup = audioMixer.FindMatchingGroups("SFX")[0];
        sfxSource.outputAudioMixerGroup = sfxGroup;
        
        subsfxGroup = audioMixer.FindMatchingGroups("Sub SFX")[0];
        sfxSource.outputAudioMixerGroup = subsfxGroup;
        loopedSFXSource.outputAudioMixerGroup = subsfxGroup;
        
        

        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
    }

    // Getter Setter pattern for encapsulation
    public float MasterVolume
    {
        get => masterVolume;
        set {SetMasterVolume(value);}
    }

    public float MusicVolume
    {
        get => musicVolume;
        set {SetMusicVolume(value);}
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set {SetSFXVolume(value);}
    }

    // Set the volume of the Music group in the AudioMixer
    // Volume needs to be exposed in the AudioMixer
    private void SetVolume(string key, float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f); // Avoids 0 case
        audioMixer.SetFloat(key, Mathf.Log10(volume) * 20f); // Conevrt todecibels
        PlayerPrefs.SetFloat(key, volume); // Store preference as linear 
    }

    public void SetMasterVolume(float volume)
    {
        SetVolume("MasterVolume", volume);
        masterVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        SetVolume("MusicVolume", volume);
        musicVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume("SFXVolume", volume);
        sfxVolume = volume;
    }

    // TODO: Replace the function names and SFX with the correct sounds and names
    // Player SFX
    public void PlayWalkSFX()
    {
        subsfxSource.PlayOneShot(walkSFX);
    }

    public void PlayJumpSFX()
    {
        subsfxSource.PlayOneShot(jumpSFX);
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
    
    // UI SFX
    public void PlayLevelSelectSFX()
    {
        sfxSource.PlayOneShot(levelselectSFX);
    }
    
    public void PlayUIButtonSFX()
    {
        sfxSource.PlayOneShot(uibuttonSFX);
    }
    
    public void PlayCurtainOpenSFX()
    {
        sfxSource.PlayOneShot(curtainopenSFX);
    }
    
    public void PlayCurtainCloseSFX()
    {
        sfxSource.PlayOneShot(curtaincloseSFX);
    }

    // Music
    public void PlayTragedyBG()
    {
        gamemusicSource.clip = tragedyBGMusic;
        gamemusicSource.Play();
        gamemusicSource.loop = true;
    }
    public void PlayComedyBG()
    {
        gamemusicSource.clip = comedyBGMusic;
        gamemusicSource.Play();
        gamemusicSource.loop = true;
    }
    public void PlayMenuBG()
    {
        menumusicSource.clip = menuBGMusic;
        menumusicSource.Play();
        menumusicSource.loop = true;
    }
    public void StopGameScore()
    {
        gamemusicSource.Stop();
    }
    public void StopMenuScore()
    {
        menumusicSource.Stop();
    }
}

