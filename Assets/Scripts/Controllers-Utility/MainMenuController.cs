using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : MonoBehaviour
{
    [SerializeField] public Button comedyButton;
    [SerializeField] public Button tragedyButton;

    [SerializeField] public Button replayButton;
    [SerializeField] public Button playButton;

    public UnityEvent introDialogue;
    public UnityEvent endDialogue;

    void Start()
    {
        // Play background music
        AudioController.Instance.PlayMenuBG();
        replayButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(true);
        
        // Hide comedy button if we already got the mask
        if (GameController.Instance.getHasComedyMask()) {
            comedyButton.gameObject.SetActive(false);
            replayButton.gameObject.SetActive(true);
        }

        // Hide tragedy button if we already got the mask
        if (GameController.Instance.getHasTragedyMask()) {
            tragedyButton.gameObject.SetActive(false);
            replayButton.gameObject.SetActive(true);
            //introDialogue.Invoke();
        }

        GameController.Instance.setPlayerIsAlive(true);

        // Only play the intro once, so returning from the pause menu goes straight to the menu
        if (!GameController.Instance.getHasComedyMask() && !GameController.Instance.getHasTragedyMask() && !GameController.Instance.getHasSeenIntro()) {

            Debug.Log("Intro Dialogue Invoke in MainMenu");
            introDialogue.Invoke();
            GameController.Instance.setHasSeenIntro(true);
        }
        else
        {
            playButton.gameObject.SetActive(false);
        }
        
        if (GameController.Instance.getHasComedyMask() && GameController.Instance.getHasTragedyMask() && !GameController.Instance.getHasSeenOutro()) {
            endDialogue.Invoke();
        }
    }

    public void setHasSeenOutro()
    {
        GameController.Instance.setHasSeenOutro(true);
    }

    // Settings button: reuses the in-game pause menu's settings
    public void OpenSettings()
    {
        GameController.Instance.OpenSettingsMenu();
    }

    public void PlayLevelSelectSFX()
    {
        AudioController.Instance.PlayLevelSelectSFX();
    }

    public void PlayUIButtonSFX()
    {
        AudioController.Instance.PlayUIButtonSFX();
    }

    public void callReset() {
        GameController.Instance.resetGame();
    }
}
