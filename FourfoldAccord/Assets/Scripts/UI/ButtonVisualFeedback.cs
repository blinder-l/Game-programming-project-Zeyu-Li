using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonVisualFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float clickPulseScale = 1.16f;
    [SerializeField] private float hoverSmoothSpeed = 12f;
    [SerializeField] private float clickPulseDuration = 0.12f;
    [SerializeField] private float clickShakeDuration = 0.1f;
    [SerializeField] private float clickShakeStrength = 3f;

    private RectTransform rectTransform;
    private Button button;
    private Vector2 baseAnchoredPosition;
    private Vector2 shakeOffset;
    private bool isHovering;
    private bool isClickPulsing;
    private bool isShaking;
    private float clickPulseTimer;
    private float clickShakeTimer;

    private void Awake()
    {
        ResolveReferences();
        RefreshBasePosition();
    }

    private void OnEnable()
    {
        ResolveReferences();
        RefreshBasePosition();

        if (!IsInteractable())
        {
            ResetVisualImmediate();
        }
    }

    private void OnDisable()
    {
        ResetVisualImmediate();
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }

        if (!IsInteractable())
        {
            ResetVisualImmediate();
            return;
        }

        UpdateShakeOffset();
        float desiredScale = GetDesiredScale();
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            Vector3.one * desiredScale,
            Time.unscaledDeltaTime * hoverSmoothSpeed);
        rectTransform.anchoredPosition = baseAnchoredPosition + shakeOffset;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        isClickPulsing = true;
        isShaking = true;
        clickPulseTimer = 0f;
        clickShakeTimer = 0f;
    }

    public void RefreshBasePosition()
    {
        ResolveReferences();

        if (rectTransform == null)
        {
            return;
        }

        baseAnchoredPosition = rectTransform.anchoredPosition - shakeOffset;
    }

    public void ResetVisualImmediate()
    {
        isHovering = false;
        isClickPulsing = false;
        isShaking = false;
        clickPulseTimer = 0f;
        clickShakeTimer = 0f;
        shakeOffset = Vector2.zero;

        if (rectTransform == null)
        {
            return;
        }

        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = baseAnchoredPosition;
    }

    private void ResolveReferences()
    {
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private bool IsInteractable()
    {
        return button != null && button.interactable && gameObject.activeInHierarchy;
    }

    private float GetDesiredScale()
    {
        float baseScale = isHovering ? hoverScale : 1f;

        if (!isClickPulsing)
        {
            return baseScale;
        }

        clickPulseTimer += Time.unscaledDeltaTime;
        float duration = Mathf.Max(0.01f, clickPulseDuration);
        float progress = Mathf.Clamp01(clickPulseTimer / duration);
        float pulseAmount = Mathf.Sin(progress * Mathf.PI);
        float pulseScale = Mathf.Lerp(baseScale, clickPulseScale, pulseAmount);

        if (progress >= 1f)
        {
            isClickPulsing = false;
            return baseScale;
        }

        return pulseScale;
    }

    private void UpdateShakeOffset()
    {
        if (!isShaking)
        {
            shakeOffset = Vector2.zero;
            return;
        }

        clickShakeTimer += Time.unscaledDeltaTime;
        float duration = Mathf.Max(0.01f, clickShakeDuration);
        float progress = Mathf.Clamp01(clickShakeTimer / duration);
        float falloff = 1f - progress;
        float angle = clickShakeTimer * 110f;
        shakeOffset = new Vector2(
            Mathf.Sin(angle) * clickShakeStrength * falloff,
            Mathf.Cos(angle * 0.8f) * clickShakeStrength * 0.35f * falloff);

        if (progress >= 1f)
        {
            isShaking = false;
            shakeOffset = Vector2.zero;
        }
    }
}
