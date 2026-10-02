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
    public GameObject player;
    private bool playerIsAlive = true;

    [Header("Masks Info")]
    private bool hasTragedyMask = false;
    private bool hasComedyMask = false;

    [Header("Checkpoints")]
    private Vector3 lastCheckpointPosition;
    private int lastCheckpointID = -1;  // default ID

    void Awake()
    {
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
            player.transform.position = getLastCheckpointPosition() + new Vector3(0, 0, -2);
        }
    }
}