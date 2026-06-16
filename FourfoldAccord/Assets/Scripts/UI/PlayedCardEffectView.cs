using TMPro;
using UnityEngine;

public class PlayedCardEffectView : MonoBehaviour
{
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private float holdDuration = 0.45f;
    [SerializeField] private float fadeDuration = 0.55f;

    private RectTransform effectRectTransform;
    private Vector2 baseAnchoredPosition;
    private float timer;
    private bool isPlaying;
    private bool hasBasePosition;

    public Vector2 BaseAnchoredPosition => baseAnchoredPosition;

    private void Awake()
    {
        ResolveReferences();
        CaptureBasePosition();
        ClearImmediate();
    }

    private void OnEnable()
    {
        ResolveReferences();
        CaptureBasePosition();
        RestoreBasePosition();
    }

    private void Update()
    {
        if (!isPlaying || effectText == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer <= holdDuration)
        {
            SetAlpha(1f);
            return;
        }

        float fadeProgress = Mathf.Clamp01((timer - holdDuration) / Mathf.Max(0.01f, fadeDuration));
        SetAlpha(1f - fadeProgress);

        if (fadeProgress >= 1f)
        {
            ClearImmediate();
        }
    }

    public void PlayChipEffect(int chipValue)
    {
        PlayEffect($"+{chipValue}");
    }

    public void PlayEffect(string displayText)
    {
        ResolveReferences();

        if (effectText == null)
        {
            Debug.LogWarning($"{name} is missing TMP text for played card effect.");
            return;
        }

        isPlaying = false;
        timer = 0f;
        effectText.gameObject.SetActive(true);
        effectText.text = displayText;
        RestoreBasePosition();
        SetAlpha(1f);
        isPlaying = true;
    }

    public void ClearImmediate()
    {
        ResolveReferences();
        isPlaying = false;
        timer = 0f;

        if (effectText != null)
        {
            effectText.text = string.Empty;
            RestoreBasePosition();
            SetAlpha(0f);
        }
    }

    private void ResolveReferences()
    {
        if (effectText != null)
        {
            return;
        }

        effectText = GetComponent<TMP_Text>();

        if (effectText == null)
        {
            effectText = GetComponentInChildren<TMP_Text>(true);
        }

        if (effectText != null)
        {
            effectText.raycastTarget = false;
            effectRectTransform = effectText.transform as RectTransform;
        }
    }

    private void CaptureBasePosition()
    {
        if (effectRectTransform == null)
        {
            return;
        }

        if (hasBasePosition)
        {
            return;
        }

        baseAnchoredPosition = effectRectTransform.anchoredPosition;
        hasBasePosition = true;
    }

    private void RestoreBasePosition()
    {
        if (effectRectTransform == null || !hasBasePosition)
        {
            return;
        }

        effectRectTransform.anchoredPosition = baseAnchoredPosition;
    }

    private void SetAlpha(float alpha)
    {
        if (effectText == null)
        {
            return;
        }

        Color color = effectText.color;
        color.a = alpha;
        effectText.color = color;
    }
}
