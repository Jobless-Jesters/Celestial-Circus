using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : MonoBehaviour
{
    [SerializeField] public Button comedyButton;
    [SerializeField] public Button tragedyButton;

    public UnityEvent introDialogueStart;
    public UnityEvent endDialogueStart;
    public UnityEvent tragDialogueStart;

    void Start()
    {
        if(!GameController.Instance.getHasComedyMask() && !GameController.Instance.getHasTragedyMask())
        {
            Debug.LogWarning("MainMenuController introDialogueStart.Invoke() called");
        } 
        
        // Hide comedy button if we already got the mask
        if (GameController.Instance.getHasComedyMask()) {
            comedyButton.gameObject.SetActive(false);
        }

        // Hide tragedy button if we already got the mask
        if (GameController.Instance.getHasTragedyMask()) {
            tragedyButton.gameObject.SetActive(false);
        }

        // Clear checkpoint data between levels
        GameController.Instance.clearCheckpoints();
        GameController.Instance.setPlayerIsAlive(true);
    }
}
