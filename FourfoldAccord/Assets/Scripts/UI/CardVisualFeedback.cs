using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardVisualFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private float selectedYOffset = 28f;
    [SerializeField] private float selectedScale = 1.06f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float smoothSpeed = 12f;
    [SerializeField] private float hoverShakeDuration = 0.1f;
    [SerializeField] private float selectShakeDuration = 0.14f;
    [SerializeField] private float hoverShakeMagnitude = 2f;
    [SerializeField] private float selectShakeMagnitude = 4f;
    [SerializeField] private float scoreShakeMagnitude = 6f;
    [SerializeField] private float hoverPulseScale = 1.1f;
    [SerializeField] private float hoverPulseDuration = 0.12f;
    [SerializeField] private float scorePulseScale = 1.18f;
    [SerializeField] private float scorePulseDuration = 0.45f;

    private Vector2 baseAnchoredPosition;
    private Vector2 targetOffset;
    private Vector2 shakeOffset;
    private Vector3 targetScale = Vector3.one;
    private Image visualImage;
    private bool isSelected;
    private bool hasVisualContent;
    private bool isShaking;
    private bool isPulsing;
    private bool isScorePulsing;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;
    private float pulseTimer;
    private float scorePulseTimer;

    private void Awake()
    {
        ResolveVisualRoot();
        RefreshBasePosition();
        ResetVisualImmediate();
    }

    private void OnEnable()
    {
        ResolveVisualRoot();
        RefreshBasePosition();
    }

    private void Update()
    {
        if (visualRoot == null)
        {
            return;
        }

        UpdateShakeOffset();
        float pulseMultiplier = UpdatePulseMultiplier();
        pulseMultiplier *= UpdateScorePulseMultiplier();

        Vector2 desiredPosition = baseAnchoredPosition + targetOffset + shakeOffset;
        visualRoot.anchoredPosition = Vector2.Lerp(
            visualRoot.anchoredPosition,
            desiredPosition,
            Time.unscaledDeltaTime * smoothSpeed);
        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            targetScale * pulseMultiplier,
            Time.unscaledDeltaTime * smoothSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!HasValidVisualContent())
        {
            return;
        }

        StartShake(hoverShakeDuration, hoverShakeMagnitude);
        StartHoverPulse();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void SetVisualRoot(RectTransform root)
    {
        visualRoot = root;
        RefreshBasePosition();
        ApplyTargets();
    }

    public void SetHasVisualContent(bool hasContent)
    {
        hasVisualContent = hasContent;

        if (!HasValidVisualContent())
        {
            ResetVisualImmediate();
        }
    }

    public void SetSelected(bool selected)
    {
        if (isSelected == selected)
        {
            ApplyTargets();
            return;
        }

        isSelected = selected;
        ApplyTargets();

        if (hasVisualContent)
        {
            StartShake(selectShakeDuration, selectShakeMagnitude);
        }
    }

    public void PlayScorePulse()
    {
        if (!HasValidVisualContent())
        {
            return;
        }

        StartShake(scorePulseDuration, scoreShakeMagnitude);
        isScorePulsing = true;
        scorePulseTimer = 0f;
    }

    public void ResetVisualImmediate()
    {
        isSelected = false;
        isShaking = false;
        isPulsing = false;
        isScorePulsing = false;
        shakeTimer = 0f;
        pulseTimer = 0f;
        scorePulseTimer = 0f;
        shakeOffset = Vector2.zero;
        targetOffset = Vector2.zero;
        targetScale = Vector3.one * normalScale;

        if (visualRoot != null)
        {
            visualRoot.anchoredPosition = baseAnchoredPosition;
            visualRoot.localScale = targetScale;
        }
    }

    public void RefreshBasePosition()
    {
        ResolveVisualRoot();

        if (visualRoot == null)
        {
            return;
        }

        if (isSelected || isShaking)
        {
            return;
        }

        baseAnchoredPosition = visualRoot.anchoredPosition;
        ApplyTargets();
    }

    private void ResolveVisualRoot()
    {
        if (visualRoot != null)
        {
            ResolveVisualImage();
            return;
        }

        Transform visualTransform = transform.Find("CardVisual");
        visualRoot = visualTransform as RectTransform;

        if (visualRoot == null)
        {
            visualRoot = transform as RectTransform;
        }

        ResolveVisualImage();
    }

    private void ResolveVisualImage()
    {
        visualImage = visualRoot != null ? visualRoot.GetComponent<Image>() : null;
    }

    private bool HasValidVisualContent()
    {
        ResolveVisualRoot();
        return hasVisualContent && visualImage != null && visualImage.enabled && visualImage.sprite != null;
    }

    private void ApplyTargets()
    {
        targetOffset = isSelected ? new Vector2(0f, selectedYOffset) : Vector2.zero;
        targetScale = Vector3.one * (isSelected ? selectedScale : normalScale);
    }

    private void StartShake(float duration, float magnitude)
    {
        isShaking = true;
        shakeTimer = 0f;
        shakeDuration = Mathf.Max(0.01f, duration);
        shakeMagnitude = magnitude;
    }

    private void StartHoverPulse()
    {
        isPulsing = true;
        pulseTimer = 0f;
    }

    private void UpdateShakeOffset()
    {
        if (!isShaking)
        {
            shakeOffset = Vector2.zero;
            return;
        }

        shakeTimer += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(shakeTimer / shakeDuration);
        float falloff = 1f - progress;
        float angle = shakeTimer * 90f;
        shakeOffset = new Vector2(
            Mathf.Sin(angle) * shakeMagnitude * falloff,
            Mathf.Cos(angle * 0.7f) * shakeMagnitude * 0.35f * falloff);

        if (progress >= 1f)
        {
            isShaking = false;
            shakeOffset = Vector2.zero;
        }
    }

    private float UpdatePulseMultiplier()
    {
        if (!isPulsing)
        {
            return 1f;
        }

        pulseTimer += Time.unscaledDeltaTime;
        float duration = Mathf.Max(0.01f, hoverPulseDuration);
        float progress = Mathf.Clamp01(pulseTimer / duration);
        float pulseAmount = Mathf.Sin(progress * Mathf.PI);
        float multiplier = Mathf.Lerp(1f, hoverPulseScale, pulseAmount);

        if (progress >= 1f)
        {
            isPulsing = false;
            return 1f;
        }

        return multiplier;
    }

    private float UpdateScorePulseMultiplier()
    {
        if (!isScorePulsing)
        {
            return 1f;
        }

        scorePulseTimer += Time.unscaledDeltaTime;
        float duration = Mathf.Max(0.01f, scorePulseDuration);
        float progress = Mathf.Clamp01(scorePulseTimer / duration);
        float pulseAmount = Mathf.Sin(progress * Mathf.PI);
        float multiplier = Mathf.Lerp(1f, scorePulseScale, pulseAmount);

        if (progress >= 1f)
        {
            isScorePulsing = false;
            return 1f;
        }

        return multiplier;
    }
}
