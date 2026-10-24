using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class BalloonAnimal : MonoBehaviour
{
    public TMP_Text hintText;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public String mask; // Which mask this balloon animal belongs to. LOWERCASE NAMES
    public UnityEvent onClick;
    private bool playerInRange;
    private bool isBalloonPopped = false;

    // Start is called before the first frame update
    void Start()
    {
        AudioController.Instance.StopMenuScore();
        if (mask == "comedy")
        {
            AudioController.Instance.PlayComedyBG();
        }
        else if (mask == "tragedy")
        {
            AudioController.Instance.PlayTragedyBG();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.X) && GameController.Instance.getPlayerIsAlive() && isBalloonPopped == false)
        {
            isBalloonPopped = true;  // can only pop balloon once to avoid glitches

            if (mask == "comedy")
            {
                GameController.Instance.setHasComedyMask(true);

            }
            else if (mask == "tragedy")
            {
                GameController.Instance.setHasTragedyMask(true);
            }
            else
            {
                print("Mask not found");
                return;
            }

            AudioController.Instance.StopGameScore();

            // Wait for balloon pop animation to finish before ending the level
            animator.SetTrigger("pop");
            AudioController.Instance.PlayLevelSelectSFX();
            StartCoroutine(playLevelEndAnimation());
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            hintText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            hintText.gameObject.SetActive(false);
        }
    }

    private IEnumerator playLevelEndAnimation()
    {
        yield return new WaitForSecondsRealtime(1.1f);

        // Invoke the level cutscene
        onClick.Invoke();
    }
}
