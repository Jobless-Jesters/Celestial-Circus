using UnityEngine;

public class TEMP_curtains : MonoBehaviour
{
    public enum CurtainMode
    {
        Full,
        Death,
        Pause
    }

    // Curtain Parts
    [SerializeField] private GameObject top;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;
    [SerializeField] private GameObject topLeft;
    [SerializeField] private GameObject topRight;
    [SerializeField] private Animator animator;

    // State tracking
    [SerializeField] private bool isOpen = false;
    [SerializeField] private CurtainMode curtainMode = CurtainMode.Full;

    private void Start()
    {
        ApplyCurtainMode();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetCurtainMode(CurtainMode.Full);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetCurtainMode(CurtainMode.Death);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetCurtainMode(CurtainMode.Pause);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("pressed t");
            animator.SetTrigger("toggleCurtain");
            animator.SetBool("isOpen", isOpen);
        }
    }

    public void SetCurtainMode(CurtainMode mode)
    {
        curtainMode = mode;
        ApplyCurtainMode();
    }

    private void ApplyCurtainMode()
    {
        // Arrange curtains differently depending on the situation
        if (top == null || left == null || right == null || topLeft == null || topRight == null)
        {
            return;
        }

        switch (curtainMode)
        {
            case CurtainMode.Full:
                top.SetActive(true);
                topLeft.SetActive(true);
                topRight.SetActive(true);
                left.SetActive(true);
                right.SetActive(true);
                break;
            case CurtainMode.Death:
                top.SetActive(false);
                topLeft.SetActive(false);
                topRight.SetActive(false);
                left.SetActive(true);
                right.SetActive(true);
                break;
            case CurtainMode.Pause:
                top.SetActive(true);
                topLeft.SetActive(false);
                topRight.SetActive(false);
                left.SetActive(false);
                right.SetActive(false);
                break;
        }
    }
}
