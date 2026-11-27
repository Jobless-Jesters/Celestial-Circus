using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class EndMenuController : MonoBehaviour
{

    [Header("Dialogue Control")]
    public UnityEvent endDialogue;

    void Start()
    {
        if (GameController.Instance.getHasComedyMask() && GameController.Instance.getHasTragedyMask() && !GameController.Instance.getHasSeenOutro()) {
            endDialogue.Invoke();
        }
    }

    public void SetHasSeenOutro()
    {
        GameController.Instance.setHasSeenOutro(true);
    }

}
