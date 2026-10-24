using UnityEngine;

// Gently floats a UI element up and down around its starting position
public class HoverBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 12f;  // how far it moves up/down
    [SerializeField] private float speed = 0.4f;     // cycles per second
    [SerializeField] private float phaseOffset = 0f; // 0-1, so multiple objects don't move in sync

    private RectTransform rectTransform;
    private Vector2 startPositionUI;
    private Vector3 startPositionGameObj;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            startPositionUI = rectTransform.anchoredPosition;
        }
        else
        {
            startPositionGameObj = transform.position;
        }
    }

    // Snap back so the start position stays correct if the menu is hidden and shown again
    void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPositionUI;
        }
        else
        {
            transform.position = startPositionGameObj;
        }
    }

    void Update()
    {
        // Unscaled so it keeps floating even if the game was paused before returning to the menu
        float offset = Mathf.Sin((Time.unscaledTime * speed + phaseOffset) * 2f * Mathf.PI) * amplitude;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPositionUI + new Vector2(0f, offset);
        }
        else
        {
            transform.position = startPositionGameObj + new Vector3(0f, offset, 0f);
        }
    }
}
