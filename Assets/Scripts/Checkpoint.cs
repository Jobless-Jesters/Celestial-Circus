using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    public int checkpointID;
    public TMP_Text debugIDText;
    // Update is called once per frame

    void Start() 
    {
        debugIDText.text = "ID: " + checkpointID.ToString();
    }

    void Update()
    {
        // Temporary code to test respawning works
        if (Input.GetKey(KeyCode.K))
        {
            GameController.Instance.respawnAtCheckpoint();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player hits the checkpoint, update the last checkpoint position in GameController
        if (collision.CompareTag("Player"))
        {
            GameController.Instance.setLastCheckpoint(checkpointID, transform.position);
        }
    }
}
