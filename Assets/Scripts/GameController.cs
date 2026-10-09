using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

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

    void Awake()
    {
        // Prevent duplicate GameManagers by checking if there is more than one singleton
        // This is also nice because all our testing scenes can have a manager
        // without it ultimately impacting all the other scenes by creating duplicates
        if (_instance != null && _instance!= this)
        {
            Destroy(gameObject);
            return; 
        }

        _instance = this;

        DontDestroyOnLoad(this.gameObject);  // Controller persists between scenes
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
        // Puts the player at the same spot as the checkpoint.
        // Offset by [0, 0, -2] so that player respawns in front of checkpoint
        // May want to add xy offset as well for a certain effect/feel
        if (getLastCheckpointPosition() != Vector3.zero)
        {
            GameObject player = GameObject.Find("Player");
            player.transform.position = getLastCheckpointPosition() + new Vector3(0, 0, -2);
        }
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
        if (_instance.flags!=null){
            if (_instance.flags.ContainsKey(flag)){
                return _instance.flags[flag];    
            }
        }
        return false;
        
    }
}