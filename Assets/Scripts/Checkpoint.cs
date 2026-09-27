using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Update is called once per frame
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
            GameController.Instance.setLastCheckpointPosition(transform.position);
        }
    }
}
