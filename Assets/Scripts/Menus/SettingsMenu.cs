using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] public Slider masterSlider;
    [SerializeField] public Slider musicSlider;
    [SerializeField] public Slider sfxSlider;

    // Lookup when needed
    private AudioController audioController => AudioController.Instance;

    // https://discussions.unity.com/t/how-do-i-make-a-change-a-volume-with-a-slider/154975/4
    void Awake()
    {
        masterSlider.onValueChanged.AddListener(v => audioController.SetMasterVolume(v));
        musicSlider.onValueChanged.AddListener(v => audioController.SetMusicVolume(v));
        sfxSlider.onValueChanged.AddListener(v => audioController.SetSFXVolume(v));
    }
    // Syncs sliders to stored values 
    void OnEnable()
    {
        masterSlider.value = audioController.MasterVolume;
        musicSlider.value = audioController.MusicVolume;
        sfxSlider.value = audioController.SFXVolume;
    }

    private void GoMainMenu()
    {
        GameController.Instance.setIsPaused(false);
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MainMenu1");
    }

    // For a back button
    private void GoBack()
    {
        if (GameController.Instance.getIsPaused()) GameController.Instance.setIsPaused(false);
    }
}
