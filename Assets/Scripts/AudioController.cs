using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    // To use:
    // AudioController.Instance.Play????SFX();
    // Wherever you need a certain sound effect
    private static AudioController _instance;
    public static AudioController Instance { get { return _instance; } }
    public AudioSource audioSource;
    // TODO: Replace with all the actual sound effects, split by category (you can add more categories/change them)
    [Header("Player SFX")]
    public AudioClip walkSFX, jumpSFX, twirlSFX, hurtSFX;
    [Header("Environment SFX")]
    public AudioClip swingingStarSFX;
    [Header("Background Tracks")]
    public AudioClip tragedyBGMusic;


    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this.gameObject); // Controller persists between scenes
    }

    // TODO: Replace the function names and SFX with the correct sounds and names
    public void PlayWalkSFX()
    {
        audioSource.PlayOneShot(walkSFX);
    }

    public void PlayJumpSFX()
    {
        audioSource.PlayOneShot(jumpSFX);
    }

    public void PlayTwirlSFX()
    {
        audioSource.PlayOneShot(twirlSFX);
    }

    public void PlayHurtSFX()
    {
        audioSource.PlayOneShot(hurtSFX);
    }
}
