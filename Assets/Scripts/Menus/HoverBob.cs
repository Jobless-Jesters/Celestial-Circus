using UnityEngine;

// Gently floats a UI element up and down around its starting position
public class HoverBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 12f;  // how far it moves up/down
    [SerializeField] private float speed = 0.4f;     // cycles per second
    [SerializeField] private float phaseOffset = 0f; // 0-1, so multiple objects don't move in sync

    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    // Snap back so the start position stays correct if the menu is hidden and shown again
    void OnDisable()
    {
        rectTransform.anchoredPosition = startPosition;
    }

    void Update()
    {
        // Unscaled so it keeps floating even if the game was paused before returning to the menu
        float offset = Mathf.Sin((Time.unscaledTime * speed + phaseOffset) * 2f * Mathf.PI) * amplitude;
        rectTransform.anchoredPosition = startPosition + new Vector2(0f, offset);
    }
}
