using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// Code was based on: "How to Make a Camera System (Like Hollow Knight's) in Unity using Cinemachine | 2D Tutorial" by Sasquatch B Studios on Youtube and built upon by the Jobless Jesters Team
public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}
public class CameraControlTrigger : MonoBehaviour
{


    [SerializeField] public Collider2D LookAtCollider;

    [Header("Camera Pan Settings")]
    [SerializeField] public PanDirection panDirection;
    [SerializeField] public float panDistance = 3f;
    [SerializeField] public float panTime = 0.35f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CameraManager.Instance.PanCameraOnContact(panDistance, panTime, panDirection, false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CameraManager.Instance.PanCameraOnContact(panDistance, panTime, panDirection, true);
        }
    }
}
