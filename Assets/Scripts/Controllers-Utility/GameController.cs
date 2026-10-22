using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    //Singleton pattern
    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }

    //Block reference for spawning
    [Header("Player Info")]
    private bool playerIsAlive = true;

    [Header("Masks Info")]
    [SerializeField] private bool hasTragedyMask = false;
    [SerializeField] private bool hasComedyMask = false;

    [Header("Checkpoints")]
    private Vector3 lastCheckpointPosition;
    private int lastCheckpointID = -1;  // default ID

    [Header("Dialogue Flags")]
    [SerializeField] private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    public GameObject mainmenu;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    private bool isPaused = false;
    public bool canPause = true;

    void Awake()
    {
        // Prevent duplicate GameManagers by checking if there is more than one singleton
        // This is also nice because all our testing scenes can have a manager
        // without it ultimately impacting all the other scenes by creating duplicates
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(this.gameObject);  // Controller persists between scenes

    }

    void Update()
    {
        // Pause the game at any time
        if (Input.GetKeyDown(KeyCode.Escape) && canPause)
        {
            isPaused = !isPaused;

            // stop and restart time
            if (isPaused)
            {
                Time.timeScale = 0.0f;
                AudioController.Instance.loopedSFXSource.Pause();

            }
            else
            {
                Time.timeScale = 1.0f;
                AudioController.Instance.loopedSFXSource.UnPause();
            }

            // Show curtains only on level scenes
            if (SceneManager.GetActiveScene().name != "MainMenu1" && SceneManager.GetActiveScene().name != "EndMenu")
            {
                // Get the curtains and play the pause curtain animation
                GameObject curtains = GameObject.Find("CurtainContainer");
                TEMP_curtains curtainScript = curtains.GetComponent<TEMP_curtains>();
                curtainScript.toggleCurtain(TEMP_curtains.CurtainMode.Pause);

                StartCoroutine(showPauseText());
            }
            else
            {
                // Don't play curtain animation on menu scenes
                pauseMenu.SetActive(isPaused);
            }

        }
    }

    private IEnumerator showPauseText()
    {
        // Wait specfied time for curtain drop animation to finish
        yield return new WaitForSecondsRealtime(.13f);

        // Show pause text after wait
        pauseMenu.SetActive(isPaused);
    }

    // Setters and Getters
    public void setHasTragedyMask(bool hasMask)
    {
        hasTragedyMask = hasMask;
    }

    public bool getHasTragedyMask()
    {
        return hasTragedyMask;
    }

    public void setHasComedyMask(bool hasMask)
    {
        hasComedyMask = hasMask;
    }

    public bool getHasComedyMask()
    {
        return hasComedyMask;
    }

    public void setMainMenu()
    {
        mainmenu = GameObject.Find("MainMenu");
    }

    public void setIsPaused(bool paused)
    {
        isPaused = paused;
    }

    public bool getIsPaused()
    {
        return isPaused;
    }

    public void setLastCheckpoint(int newID, Vector3 pos)
    {
        // Only update the checkpoint if it is further along the level than the previous checkpoint
        if (lastCheckpointID < newID)
        {
            lastCheckpointID = newID;
            lastCheckpointPosition = pos;
        }
    }

    public Vector3 getLastCheckpointPosition()
    {
        return lastCheckpointPosition;
    }

    public void clearCheckpoints()
    {
        lastCheckpointID = -1;
        lastCheckpointPosition = Vector3.zero;
    }

    public void setPlayerIsAlive(bool isAlive)
    {
        playerIsAlive = isAlive;
    }

    public bool getPlayerIsAlive()
    {
        return playerIsAlive;
    }

    public void respawnAtCheckpoint()
    {
        if (getLastCheckpointPosition() != Vector3.zero)
        {

            Debug.Log("DIED");

            // disable pause to avoid animator conflicts
            canPause = false;

            // Get all components
            GameObject curtains = GameObject.Find("CurtainContainer");
            TEMP_curtains curtainScript = curtains.GetComponent<TEMP_curtains>();
            GameObject player = GameObject.Find("Player");
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            PlayerInput input = player.GetComponent<PlayerInput>();

            // Freeze player movement
            AudioController.Instance.PlayHurtSFX();
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            input.enabled = false;

            // Toggle death curtain
            curtainScript.toggleCurtain(TEMP_curtains.CurtainMode.Death);
            StartCoroutine(RespawnAfterCurtain(player, rb, curtainScript, input));
        }
    }

    private IEnumerator RespawnAfterCurtain(GameObject player, Rigidbody2D rb, TEMP_curtains cs, PlayerInput ip)
    {
        // Wait specfied time for curtain close animation to finish before resetting player
        yield return new WaitForSecondsRealtime(2.0f);

        // Reset player pos: offset by [0, 0, -2] so that player respawns in front of checkpoint
        player.transform.position = getLastCheckpointPosition() + new Vector3(0, 0, -2);
        setPlayerIsAlive(true);  // end the death animation
        rb.constraints = RigidbodyConstraints2D.None;  // Let the player fall to ground
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // give player time to reset and hide animation changes
        yield return new WaitForSecondsRealtime(0.4f);

        // Open curtains
        cs.toggleCurtain(TEMP_curtains.CurtainMode.Death);

        // Re-enable pause and movement after opening animation is done
        yield return new WaitForSecondsRealtime(1.4f);
        canPause = true;
        ip.enabled = true;
    }

    // dealing with dialogue flags
    // reused code and comments from DialogueFlags.cs from canvas base unity 

    /// Static fuctions to shorten calls to the singleton instance
    /// ie. DialogueFlags.SetFlag(...) is better than DialogueFlags.Instance.SetFlag(...)

    /// <summary>
    /// Sets the flag.
    /// </summary>
    /// <param name="flag">The flag ID</param>
    /// <param name="value">Value to set the flag to.</param>
    public static void SetFlag(string flag, bool value)
    {
        _instance.flags[flag] = value;

    }

    /// <summary>
    /// Obtains the flag value. Doesn't error check, leave to users to catch exceptions.
    /// </summary>
    /// <param name="flag">Which flag to retrieve.</param>
    /// <returns>The contents of the given flag.</returns>
    public static bool GetFlagValue(string flag)
    {
        if (_instance.flags != null)
        {
            if (_instance.flags.ContainsKey(flag))
            {
                return _instance.flags[flag];
            }
        }
        return false;

    }
}