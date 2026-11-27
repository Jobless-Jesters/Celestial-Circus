using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine.Video;
using System;

public class CameraManager : MonoBehaviour
{
    //Singleton pattern
    private static CameraManager _instance;
    public static CameraManager Instance { get { return _instance; } }

    private Coroutine panCameraCoroutine;
    public CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;
    [SerializeField] private float zoomLensOrthoSize = 30f;
    [SerializeField] private float ogLensOrthoSize = 15.76f;
    private Vector2 startingTrackedObjectOffset;

    [Header("Camera Tracking Variables")]
    // A semi-static anchor for the camera. Does not change until we jump on a new platform, changes fluidly if we are tracking the player
    [SerializeField] Transform cameraLevelTransform;
    // The current target of the camera. Either the cameraLevel or the Player
    [SerializeField] private Transform currTarget;
    // The transform of the player we follow
    private Transform playerFollowTarget;
    // Our target position. Our camera will be trying to get to this position
    [SerializeField] Vector3 cameraTargetPos;
    // How long a lerp takes to complete while tracking player movements across the level
    [SerializeField] float lerpCamLevelTime = 0.25f;
    [SerializeField] float lerpOffsetTime;
    private Coroutine lerpOffsetCoroutine;
    // How fast the camera is moving during the smooth damp. Initialized to zero during swaps
    private Vector3 cameraLevelVelocity;
    // Needed to smoothly map camera changes
    [SerializeField] private Vector3 fallingViewOffset = new Vector3(0, 7.67f, 0);
    [SerializeField] private Vector3 risingViewOffset = new Vector3(0, 7.67f, 0);


    void Awake()
    {
        // Simple singleton pattern
        if (_instance == null)
        {
            _instance = this;
        }

        // Get components
        framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        startingTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;

        // Set the current camera target position to the camera level's transform.
        cameraTargetPos = cameraLevelTransform.position;
    }

    private void Update()
    {
        currTarget = playerFollowTarget != null ? playerFollowTarget : cameraLevelTransform;

        // If we swapped to the player, set them as our goal target position
        if (playerFollowTarget != null)
            cameraTargetPos = playerFollowTarget.position;


        // Either go to the next platform or smooth onto the player, depending on the cameraTargetPos
        cameraLevelTransform.position = Vector3.SmoothDamp(cameraLevelTransform.position, cameraTargetPos, ref cameraLevelVelocity, lerpCamLevelTime);
    }


    public void changeCameraLevel(Vector3 position)
    {
        // When a player jumps on a platform
        // Make the camera's target position the new position they landed on
        cameraTargetPos = position;
    }

    public void setFollowPlayer(Transform transform, bool onTightrope, bool gliding)
    {
        // Swap to the player
        // Swap only once
        if (currentCamera.Follow == cameraLevelTransform && playerFollowTarget != transform)
        {
            playerFollowTarget = transform;  // set the transform we'll be tracking
            if (!onTightrope && !gliding)
            {
                // Only if truly falling change the offset
                // Do not change offsets on tightropes
                LerpOffsetsFunc(fallingViewOffset);
            }
            else if (gliding)
            {
                // Camera stays at regular offset during glides, otherwise it looks weird
                LerpOffsetsFunc(risingViewOffset);
            }
            cameraTargetPos = transform.position;  // set our goal target position
        }
    }

    public void setFollowCameraLevel()
    {
        // Swap the camera follow target to the cameraLevel
        // Swap only once
        if (playerFollowTarget != null || currentCamera.Follow != cameraLevelTransform)
        {
            playerFollowTarget = null;  // unlink the player
            cameraLevelVelocity = Vector3.zero;  // init smoothDamp velocity
            LerpOffsetsFunc(risingViewOffset);
            currentCamera.Follow = cameraLevelTransform;  // follow the ground level again
        }
    }

    private void LerpOffsetsFunc(Vector3 newOffset)
    {
        if (lerpOffsetCoroutine != null)
        {
            StopCoroutine(lerpOffsetCoroutine);
        }

        lerpOffsetCoroutine = StartCoroutine(LerpOffsets(newOffset));
    }

    private IEnumerator LerpOffsets(Vector3 newOffset)
    {
        Vector3 startOffset = framingTransposer.m_TrackedObjectOffset;
        float duration = Mathf.Max(0f, lerpOffsetTime);
        float elapsedTime = 0f;

        Debug.Log("Entered LerpOffsets. New offset is " + newOffset);

        if (duration == 0f)
        {
            framingTransposer.m_TrackedObjectOffset = newOffset;
            lerpOffsetCoroutine = null;
            yield break;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsedTime / duration));
            framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(startOffset, newOffset, t);
            yield return null;
        }

        framingTransposer.m_TrackedObjectOffset = newOffset;
        lerpOffsetCoroutine = null;
    }

    public float getCameraLevelPosY()
    {
        return cameraLevelTransform.position.y;
    }


    // Methods related to camera trigger panning
    public void PanCameraOnContact(float panDistance, float panTime, float zoomTime, PanDirection panDirection, bool panToStartingPos)
    {
        if (panCameraCoroutine != null)
        {
            StopCoroutine(panCameraCoroutine);
        }

        panCameraCoroutine = StartCoroutine(CameraPanCoroutine(panDistance, panTime, zoomTime, panDirection, panToStartingPos));
    }

    private IEnumerator CameraPanCoroutine(float panDistance, float panTime, float zoomTime, PanDirection panDirection, bool panToStartingPos)
    {
        // init start and end points
        Vector2 endPos = Vector2.zero;
        Vector2 startPos = Vector2.zero;
        float startLensOrthoSize = 0f;
        float endLensOrthoSize = 0f;

        // handle pan from trigger (pan/zoom out)
        if (!panToStartingPos)
        {
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.left;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.right;
                    break;
            }

            endPos *= panDistance;
            startPos = framingTransposer.m_TrackedObjectOffset;
            endPos += startingTrackedObjectOffset;

            // Always start on the current camera size to avoid jarring switches between sizes
            startLensOrthoSize = currentCamera.m_Lens.OrthographicSize;
            endLensOrthoSize = zoomLensOrthoSize;
        }
        else // pan/zoom back to original position
        {
            startPos = framingTransposer.m_TrackedObjectOffset;
            endPos = startingTrackedObjectOffset;

            // Always start on the current camera size to avoid jarring switches between sizes
            startLensOrthoSize = currentCamera.m_Lens.OrthographicSize;
            endLensOrthoSize = ogLensOrthoSize;
        }

        float elapsedTime = 0f;
        // Allow for the coroutine to run as long as the larger of the the two times
        while (elapsedTime < panTime || elapsedTime < zoomTime)
        {
            elapsedTime += Time.deltaTime;

            // Calculate pan and zoom transformations
            float panT = panTime <= 0f
                ? 1f
                : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsedTime / panTime));
            float zoomT = zoomTime <= 0f
                ? 1f
                : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsedTime / zoomTime));

            // Lerp between the two positions/lens sizes
            framingTransposer.m_TrackedObjectOffset = Vector2.Lerp(startPos, endPos, panT);
            currentCamera.m_Lens.OrthographicSize = Mathf.Lerp(startLensOrthoSize, endLensOrthoSize, zoomT);
            yield return null;
        }

        framingTransposer.m_TrackedObjectOffset = endPos;
        currentCamera.m_Lens.OrthographicSize = endLensOrthoSize;
    }
}