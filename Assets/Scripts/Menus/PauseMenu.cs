using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Handles the pause menu buttons: Resume, Settings, Main Menu and Go Back
public class PauseMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject mainMenuButton;
    [SerializeField] private GameObject goBackButton;

    [Header("Settings")]
    [SerializeField] private GameObject[] settingsRows;   // volume slider rows, only shown in settings view
    [SerializeField] private Image panel;
    [SerializeField] private Sprite settingsPanelSprite;  // panel art with the Master/Music/SFX labels

    private Sprite pausePanelSprite;
    private bool settingsOpen;
    private bool openedFromMainMenu;  // Go Back closes the menu instead of returning to the pause buttons

    void Awake()
    {
        pausePanelSprite = panel.sprite;
    }

    // Always reopen on the main buttons
    void OnEnable()
    {
        settingsOpen = false;
        openedFromMainMenu = false;
        ApplyView();
    }

    public void Resume()
    {
        AudioController.Instance.PlayUIButtonSFX();
        if (GameController.Instance.getIsPaused()) GameController.Instance.TogglePause();
    }

    public void OpenSettings()
    {
        AudioController.Instance.PlayUIButtonSFX();
        settingsOpen = true;
        ApplyView();
    }

    public void GoBack()
    {
        if (openedFromMainMenu)
        {
            Resume();
            return;
        }
        AudioController.Instance.PlayUIButtonSFX();
        settingsOpen = false;
        ApplyView();
    }

    // Opens straight onto the volume sliders (used by the main menu's Settings button)
    public void ShowSettings()
    {
        settingsOpen = true;
        openedFromMainMenu = true;
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
        settingsButton.SetActive(!settingsOpen);
        mainMenuButton.SetActive(!settingsOpen);
        goBackButton.SetActive(settingsOpen);
        foreach (GameObject row in settingsRows) row.SetActive(settingsOpen);
        panel.sprite = settingsOpen ? settingsPanelSprite : pausePanelSprite;
    }
}
