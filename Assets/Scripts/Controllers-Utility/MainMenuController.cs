using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : MonoBehaviour
{
    [Header("Masks Control")]
    [SerializeField] public Button comedyButton;
    [SerializeField] public Button tragedyButton;

    [Header("Replay Control")]
    [SerializeField] public Button replayButton;
    [SerializeField] public Button playButton;

    [Header("Dialogue Control")]
    public UnityEvent introDialogue;
    public UnityEvent endDialogue;

    void Start()
    {
        // Play background music
        AudioController.Instance.PlayMenuBG();

        // Controlling which buttons are displayed and when 
        SetTragedyDisplay();
        SetComedyDisplay();
        SetPlaysDisplay();

        // Setting player status
        GameController.Instance.setPlayerIsAlive(true);

        // if player has both masks
        // and player hasn't seen outro 
        // do the endDialogue event 
        if (GameController.Instance.getHasComedyMask() && GameController.Instance.getHasTragedyMask() && !GameController.Instance.getHasSeenOutro()) {
            endDialogue.Invoke();
        }
    }

    public void SetTragedyDisplay()
    {
        // Don't have tragedy mask
        // Display tragedy level button 
        if (!GameController.Instance.getHasTragedyMask()) {
            tragedyButton.gameObject.SetActive(true);
        }
        else
        {
            tragedyButton.gameObject.SetActive(false);
        }
    }

    public void SetComedyDisplay()
    {
        // Don't have comedy mask
        // Display comedy level button 
        if (!GameController.Instance.getHasComedyMask())
        {
            comedyButton.gameObject.SetActive(true);
        }
        else
        {
            comedyButton.gameObject.SetActive(false);
        }
    }

    public void SetPlaysDisplay()
    {
        // hide replay button to start 
        replayButton.gameObject.SetActive(false);
        
        // If the intro hasn't been seen
        // Play intro button is displayed
        // hide masks buttons 
        if (!GameController.Instance.getHasSeenIntro())
        {
            playButton.gameObject.SetActive(true);
            comedyButton.gameObject.SetActive(false);
            tragedyButton.gameObject.SetActive(false);
        }
        // If they have seen the intro
        // hide the play button
        // deal wtih mask buttons separately 
        else
        {
            playButton.gameObject.SetActive(false);
        }

        // If player has at least one mask 
        // Replay button becomes available 
        if (GameController.Instance.getHasComedyMask() || GameController.Instance.getHasTragedyMask())
        {
            replayButton.gameObject.SetActive(true);
        }
    }

    public void SetHasSeenIntro()
    {
        GameController.Instance.setHasSeenIntro(true);
    }

    public void SetHasSeenOutro()
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

    public void CallReset() {
        GameController.Instance.resetGame();
    }
}
