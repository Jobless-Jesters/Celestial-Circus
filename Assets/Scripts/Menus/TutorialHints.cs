using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Shows control hints in the corner of the screen as the player reaches parts of the level.
// Each hint shows once per level load.
public class TutorialHints : MonoBehaviour
{
    [System.Serializable]
    public class Hint
    {
        public Sprite sprite;
        public bool showOnStart;    // shown startDelay seconds after the level loads instead of by position
        public Vector2 triggerPoint; // player position inside LevelMenu (the values shown in the Inspector)
        [HideInInspector] public bool shown;
    }

    [SerializeField] private Transform player;
    [SerializeField] private Image hintImage;
    [SerializeField] private Hint[] hints;

    [Header("Timing")]
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float triggerRadius = 2f;
    [SerializeField] private float showTime = 5f;
    [SerializeField] private float fadeTime = 0.4f;

    [Header("Motion")]
    [SerializeField] private float riseDistance = 20f; // slides up this far while fading in
    [SerializeField] private float bobAmount = 6f;
    [SerializeField] private float bobSpeed = 0.5f;

    private RectTransform rect;
    private Vector2 restPosition;
    private Coroutine current;

    void Awake()
    {
        rect = hintImage.rectTransform;
        restPosition = rect.anchoredPosition;
        SetAlpha(0f);
    }

    void Start()
    {
        StartCoroutine(ShowStartHints());
    }

    void Update()
    {
        foreach (Hint hint in hints)
        {
            if (hint.shown || hint.showOnStart) continue;
            if (Vector2.Distance(player.localPosition, hint.triggerPoint) <= triggerRadius) Show(hint);
        }
    }

    private IEnumerator ShowStartHints()
    {
        yield return new WaitForSeconds(startDelay);
        foreach (Hint hint in hints)
        {
            if (hint.showOnStart && !hint.shown) Show(hint);
        }
    }

    // A new hint replaces whatever is currently showing
    private void Show(Hint hint)
    {
        hint.shown = true;
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(Play(hint.sprite));
    }

    private IEnumerator Play(Sprite sprite)
    {
        // Quickly fade out the previous hint if one is still visible
        float startAlpha = hintImage.color.a;
        for (float t = 0f; startAlpha > 0f && t < fadeTime / 2f; t += Time.deltaTime)
        {
            SetAlpha(Mathf.Lerp(startAlpha, 0f, t / (fadeTime / 2f)));
            yield return null;
        }

        hintImage.sprite = sprite;
        float elapsed = 0f;
        float total = fadeTime + showTime + fadeTime;
        while (elapsed < total)
        {
            float alpha = elapsed < fadeTime ? elapsed / fadeTime
                        : elapsed > fadeTime + showTime ? 1f - (elapsed - fadeTime - showTime) / fadeTime
                        : 1f;
            float rise = Mathf.SmoothStep(-riseDistance, 0f, Mathf.Clamp01(elapsed / fadeTime));
            float bob = Mathf.Sin(elapsed * bobSpeed * 2f * Mathf.PI) * bobAmount;
            rect.anchoredPosition = restPosition + new Vector2(0f, rise + bob);
            SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetAlpha(0f);
        current = null;
    }

    private void SetAlpha(float alpha)
    {
        Color c = hintImage.color;
        c.a = alpha;
        hintImage.color = c;
    }
}
