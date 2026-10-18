using System;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using Unity.VisualScripting;
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
        ApplyCurtainMode(curtainMode);
    }

    public void setOpen()
    {
        // Called by animation event in the curtain open and closing animations
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
    }

    public void toggleCurtain(CurtainMode mode)
    {
        ApplyCurtainMode(mode);
        animator.SetTrigger("toggleCurtain");
        animator.SetBool("isOpen", isOpen);

    }

    private void ApplyCurtainMode(CurtainMode mode)
    {
        // Arrange curtains differently depending on the situation
        // Can also customize speed of animation
        if (top == null || left == null || right == null || topLeft == null || topRight == null)
        {
            return;
        }

        curtainMode = mode;

        switch (curtainMode)
        {
            case CurtainMode.Full:
                top.SetActive(true);
                topLeft.SetActive(true);
                topRight.SetActive(true);
                left.SetActive(true);
                right.SetActive(true);
                animator.speed = 1.0f;
                break;
            case CurtainMode.Death:
                top.SetActive(false);
                topLeft.SetActive(false);
                topRight.SetActive(false);
                left.SetActive(true);
                right.SetActive(true);
                animator.speed = 3f;
                break;
            case CurtainMode.Pause:
                top.SetActive(true);
                topLeft.SetActive(false);
                topRight.SetActive(false);
                left.SetActive(false);
                right.SetActive(false);
                animator.speed = 3f;
                break;
        }
    }
}
