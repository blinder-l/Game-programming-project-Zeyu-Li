using UnityEngine;
using UnityEngine.EventSystems;

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

    private Vector2 baseAnchoredPosition;
    private Vector2 targetOffset;
    private Vector2 shakeOffset;
    private Vector3 targetScale = Vector3.one;
    private bool isSelected;
    private bool hasVisualContent;
    private bool isShaking;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;

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

        Vector2 desiredPosition = baseAnchoredPosition + targetOffset + shakeOffset;
        visualRoot.anchoredPosition = Vector2.Lerp(
            visualRoot.anchoredPosition,
            desiredPosition,
            Time.unscaledDeltaTime * smoothSpeed);
        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            targetScale,
            Time.unscaledDeltaTime * smoothSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hasVisualContent)
        {
            return;
        }

        StartShake(hoverShakeDuration, hoverShakeMagnitude);
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

        if (!hasVisualContent)
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

    public void ResetVisualImmediate()
    {
        isSelected = false;
        isShaking = false;
        shakeTimer = 0f;
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
            return;
        }

        Transform visualTransform = transform.Find("CardVisual");
        visualRoot = visualTransform as RectTransform;

        if (visualRoot == null)
        {
            visualRoot = transform as RectTransform;
        }
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
}
