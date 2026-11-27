using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLockOn : MonoBehaviour
{
    [Header("Lock On Height")]
    [SerializeField] private Vector3 lockOnLevel;

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Pick a level to lock on to during wall jump sequences
        if (collision.CompareTag("Player"))
        {
            CameraManager.Instance.changeCameraLevel(lockOnLevel);
            CameraManager.Instance.setFollowCameraLevel();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Restore normal camera tracking
        if (collision.CompareTag("Player"))
        {
            CameraManager.Instance.changeCameraLevel(collision.transform.position);
            CameraManager.Instance.setFollowPlayer(collision.transform, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(lockOnLevel - new Vector3(50, 0, 0), lockOnLevel + new Vector3(50, 0, 0));
    }
}
