using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] public Button comedyButton;
    [SerializeField] public Button tragedyButton;

    void Start()
    {
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
