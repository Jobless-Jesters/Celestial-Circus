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

    private Coroutine lerpYPanCoroutine;
    private Coroutine panCameraCoroutine;
    public CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;
    [SerializeField] private float zoomLensOrthoSize = 30f;
    [SerializeField] private float ogLensOrthoSize = 15.76f;
    private Vector2 startingTrackedObjectOffset;
    [SerializeField] Transform cameraLevelPos;
    [SerializeField] Vector3 camLevPos;
    [SerializeField] float lerpCamLevelTime = 0.25f;
    private Vector3 cameraLevelVelocity;
    [SerializeField] private float fallingViewOffset = 7.67f;

    void Awake()
    {
        // Simple singleton pattern
        if (_instance == null)
        {
            _instance = this;
        }

        framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        startingTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
        camLevPos = cameraLevelPos.position;
    }

    public void changeCameraLevel(Vector3 position)
    {
        camLevPos = position;
    }

    private void Update()
    {
        if (currentCamera.Follow == cameraLevelPos)
        {
            cameraLevelPos.position = Vector3.SmoothDamp(cameraLevelPos.position, camLevPos, ref cameraLevelVelocity, lerpCamLevelTime);
        }
    }

    public void setFollowPlayer(Transform transform)
    {
        // Swap only once
        if (currentCamera.Follow == cameraLevelPos)
        {
            framingTransposer.m_TrackedObjectOffset = startingTrackedObjectOffset + Vector2.down * fallingViewOffset;
            currentCamera.Follow = transform;
        }
    }

    public void setFollowCameraLevel()
    {
        // swap only once
        if (currentCamera.Follow != cameraLevelPos)
        {
            framingTransposer.m_TrackedObjectOffset = startingTrackedObjectOffset;
            currentCamera.Follow = cameraLevelPos;
        }
    }

    public float getCameraLevelPosY()
    {
        return cameraLevelPos.position.y;
    }

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