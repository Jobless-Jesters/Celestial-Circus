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
    public String mask; // Which mask this balloon animal belongs to. LOWERCASE NAMES
    public UnityEvent onClick;
    private bool playerInRange;

    // Start is called before the first frame update
    void Start()
    {
        AudioController.Instance.PlayTragedyBG();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.X))
        {

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

            onClick.Invoke();

            //playLevelEndAnimation();

            //SceneManager.LoadScene("MainMenu");

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

    void playLevelEndAnimation()
    {
        // Code for level end animations and effects here
        
        return;
    }
}
