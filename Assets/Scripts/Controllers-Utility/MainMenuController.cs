using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : MonoBehaviour
{
    [SerializeField] public Button comedyButton;
    [SerializeField] public Button tragedyButton;

    public UnityEvent introDialogue;
    public UnityEvent endDialogue;

    void Start()
    {
        // Play background music
        AudioController.Instance.PlayMenuBG();
        
        // Hide comedy button if we already got the mask
        if (GameController.Instance.getHasComedyMask()) {
            comedyButton.gameObject.SetActive(false);
        }

        // Hide tragedy button if we already got the mask
        if (GameController.Instance.getHasTragedyMask()) {
            tragedyButton.gameObject.SetActive(false);
            //introDialogue.Invoke();
        }

        GameController.Instance.setPlayerIsAlive(true);

        // Only play the intro once, so returning from the pause menu goes straight to the menu
        if (!GameController.Instance.getHasComedyMask() && !GameController.Instance.getHasTragedyMask() && !GameController.Instance.getHasSeenIntro()) {
            GameController.Instance.setHasSeenIntro(true);
            introDialogue.Invoke();
        }
        if (GameController.Instance.getHasComedyMask() && GameController.Instance.getHasTragedyMask()) {
            endDialogue.Invoke();
        }
    }

    // Settings button: reuses the in-game pause menu's settings
    public void OpenSettings()
    {
        GameController.Instance.OpenSettingsMenu();
    }

    public void LoadSceneByName(string sceneName)
    {
        GameController.Instance.LoadSceneByName(sceneName);
    }

    public void PlayLevelSelectSFX()
    {
        AudioController.Instance.PlayLevelSelectSFX();
    }

    public void PlayUIButtonSFX()
    {
        AudioController.Instance.PlayUIButtonSFX();
    }
}
