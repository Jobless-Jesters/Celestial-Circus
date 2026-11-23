using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

public class CameraManager : MonoBehaviour
{
    //Singleton pattern
    private static CameraManager _instance;
    public static CameraManager Instance { get { return _instance; } }

    [Header("Damping Controls During Jump/Fall")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallYPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -15f;

    public bool isLerpingYDamping { get; private set; }
    public bool lerpedFromPlayerFalling { get; set; }

    private Coroutine lerpYPanCoroutine;
    private Coroutine panCameraCoroutine;
    public CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;
    private Vector2 startingTrackedObjectOffset;

    void Awake()
    {
        // Simple singleton pattern
        if (_instance == null)
        {
            _instance = this;
        }

        framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        normYPanAmount = framingTransposer.m_YDamping;
        startingTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
    }

    public void LerpYDamping(bool isPlayerFalling)
    {
        if (lerpYPanCoroutine != null)
        {
            StopCoroutine(lerpYPanCoroutine);
        }

        isLerpingYDamping = false;
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        isLerpingYDamping = true;

        float startDampAmount = framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            lerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }

        float elapsedTime = 0f;
        while(elapsedTime <fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / fallYPanTime);
            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, t);
            framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        framingTransposer.m_YDamping = endDampAmount;
        isLerpingYDamping = false;
        lerpYPanCoroutine = null;
    }

    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        StartCoroutine(CameraPanCoroutine(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator CameraPanCoroutine(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startPos = Vector2.zero;

        // handle pan from trigger
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
            startPos = startingTrackedObjectOffset;
            endPos += startPos;
        }
        else
        {
            startPos = framingTransposer.m_TrackedObjectOffset;
            endPos = startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while (elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startPos, endPos, elapsedTime / panTime);
            framingTransposer.m_TrackedObjectOffset = panLerp;

            yield return null;
        }
    }
}