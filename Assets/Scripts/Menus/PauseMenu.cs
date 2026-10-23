using UnityEngine;
using UnityEngine.SceneManagement;

// Handles the pause menu buttons: Resume, Settings and Main Menu
public class PauseMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private RectTransform settingsButton;
    [SerializeField] private GameObject mainMenuButton;

    [Header("Settings")]
    [SerializeField] private GameObject[] settingsRows;          // volume slider rows, only shown in settings view
    [SerializeField] private Vector2 settingsButtonOpenPosition; // where the Settings button sits while the sliders are showing

    private Vector2 settingsButtonHomePosition;
    private bool settingsOpen;

    void Awake()
    {
        settingsButtonHomePosition = settingsButton.anchoredPosition;
    }

    // Always reopen on the main buttons
    void OnEnable()
    {
        settingsOpen = false;
        ApplyView();
    }

    public void Resume()
    {
        AudioController.Instance.PlayUIButtonSFX();
        if (GameController.Instance.getIsPaused()) GameController.Instance.TogglePause();
    }

    // Swaps between the buttons and the volume sliders. Pressing Settings again goes back
    public void ToggleSettings()
    {
        AudioController.Instance.PlayUIButtonSFX();
        settingsOpen = !settingsOpen;
        ApplyView();
    }

    public void GoMainMenu()
    {
        AudioController.Instance.PlayUIButtonSFX();
        GameController.Instance.setIsPaused(false);
        Time.timeScale = 1.0f;
        AudioController.Instance.loopedSFXSource.Stop();
        gameObject.SetActive(false);
        SceneManager.LoadScene("MainMenu1");
    }

    private void ApplyView()
    {
        resumeButton.SetActive(!settingsOpen);
        mainMenuButton.SetActive(!settingsOpen);
        foreach (GameObject row in settingsRows) row.SetActive(settingsOpen);
        settingsButton.anchoredPosition = settingsOpen ? settingsButtonOpenPosition : settingsButtonHomePosition;
    }
}
