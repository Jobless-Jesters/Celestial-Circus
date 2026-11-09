using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Keeps the game in its intended 16:9 composition on any window / screen / browser canvas shape.

public class AspectRatioEnforcer : MonoBehaviour
{
    public const float TargetAspect = 16f / 9f;

    // Window aspects this close to 16:9 (e.g. 1366x768) just fill the window instead of getting hairline bars
    private const float Tolerance = 0.01f;

    // Distance of the UI plane from the camera. Keep it large (Unity's default): UI pieces such as the curtains
    // have z offsets that overlay canvases ignored, and a plane too close to the camera clips them at the near plane.
    // Draw order against sprites comes from the sorting layer below, not from this distance.
    private const float CanvasPlaneDistance = 100f;

    // Sorting layer added in Project Settings > Tags and Layers, listed after every gameplay layer
    // so Screen Space - Camera canvases draw over sprites like overlay canvases did
    private const string UILayerName = "UI";

    private static AspectRatioEnforcer instance;

    private Camera barsCamera;
    private int lastWidth, lastHeight;
    private Rect viewport = new Rect(0f, 0f, 1f, 1f); // normalized, shared by every camera

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        new GameObject("AspectRatioEnforcer").AddComponent<AspectRatioEnforcer>();
    }

    // True if a screen position is inside the 16:9 game area, not on a black bar.
    // Used so clicks on the bars don't advance dialogue.
    public static bool IsPointerInGameView(Vector2 screenPosition)
    {
        if (instance == null) return true;
        Rect r = instance.viewport;
        float x = screenPosition.x / Screen.width;
        float y = screenPosition.y / Screen.height;
        return x >= r.xMin && x <= r.xMax && y >= r.yMin && y <= r.yMax;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        CreateBarsCamera();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Refresh();
    }

    void OnDestroy()
    {
        if (instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Refresh();
    }

    // Browsers and fullscreen toggles don't give Unity a resize event, so just watch the size
    void LateUpdate()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight) Refresh();
    }

    private void CreateBarsCamera()
    {
        var go = new GameObject("LetterboxBarsCamera");
        go.transform.SetParent(transform, false);
        barsCamera = go.AddComponent<Camera>();
        barsCamera.clearFlags = CameraClearFlags.SolidColor;
        barsCamera.backgroundColor = Color.black;
        barsCamera.cullingMask = 0;   // draws nothing, only clears the whole window to black
        barsCamera.depth = -100f;     // before every scene camera
        barsCamera.orthographic = true;
        barsCamera.allowMSAA = false;
        barsCamera.allowHDR = false;
        var data = barsCamera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = false;
    }

    private void Refresh()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        if (lastWidth <= 0 || lastHeight <= 0) return;

        viewport = ComputeViewport(lastWidth, lastHeight);

        foreach (Camera cam in Camera.allCameras)
        {
            if (cam == barsCamera || cam.targetTexture != null) continue;
            cam.rect = viewport;
        }

        Camera gameCamera = FindGameCamera();
        bool hasUILayer = SortingLayer.IsValid(SortingLayer.NameToID(UILayerName));
        foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
        {
            if (canvas.isRootCanvas)
            {
                if (canvas.renderMode == RenderMode.WorldSpace) continue;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = gameCamera;
                canvas.planeDistance = CanvasPlaneDistance;
                if (hasUILayer) canvas.sortingLayerName = UILayerName;
            }
            else if (canvas.overrideSorting && hasUILayer)
            {
                // Nested canvases that sort themselves (e.g. the curtains) must move to the UI layer as well,
                // otherwise they would end up behind their own root canvas. Their sorting order is kept.
                canvas.sortingLayerName = UILayerName;
            }
        }
    }

    // Largest centred 16:9 rectangle that fits the window, snapped to whole pixels
    private static Rect ComputeViewport(int width, int height)
    {
        float windowAspect = (float)width / height;
        if (Mathf.Abs(windowAspect - TargetAspect) <= Tolerance) return new Rect(0f, 0f, 1f, 1f);

        if (windowAspect > TargetAspect)
        {
            // Window is wider than 16:9: bars on the left and right
            float w = Mathf.Round(height * TargetAspect);
            return new Rect((width - w) * 0.5f / width, 0f, w / width, 1f);
        }
        else
        {
            // Window is taller than 16:9: bars on the top and bottom
            float h = Mathf.Round(width / TargetAspect);
            return new Rect(0f, (height - h) * 0.5f / height, 1f, h / height);
        }
    }

    private Camera FindGameCamera()
    {
        Camera main = Camera.main;
        if (main != null && main != barsCamera) return main;
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam != barsCamera && cam.targetTexture == null) return cam;
        }
        return null;
    }
}
