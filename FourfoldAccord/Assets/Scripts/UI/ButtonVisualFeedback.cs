using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonVisualFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float clickPulseScale = 1.16f;
    [SerializeField] private float hoverSmoothSpeed = 12f;
    [SerializeField] private float clickPulseDuration = 0.12f;
    [SerializeField] private float clickShakeDuration = 0.1f;
    [SerializeField] private float clickShakeStrength = 3f;

    private RectTransform rootRectTransform;
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
        if (visualRoot == null)
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
        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            Vector3.one * desiredScale,
            Time.unscaledDeltaTime * hoverSmoothSpeed);
        visualRoot.anchoredPosition = baseAnchoredPosition + shakeOffset;
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

        if (visualRoot == null)
        {
            return;
        }

        baseAnchoredPosition = visualRoot.anchoredPosition - shakeOffset;
    }

    public void ResetVisualImmediate()
    {
        isHovering = false;
        isClickPulsing = false;
        isShaking = false;
        clickPulseTimer = 0f;
        clickShakeTimer = 0f;
        shakeOffset = Vector2.zero;

        if (visualRoot == null)
        {
            return;
        }

        visualRoot.localScale = Vector3.one;
        visualRoot.anchoredPosition = baseAnchoredPosition;
    }

    private void ResolveReferences()
    {
        if (rootRectTransform == null)
        {
            rootRectTransform = transform as RectTransform;
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        EnsureVisualRoot();
    }

    private void EnsureVisualRoot()
    {
        if (visualRoot != null)
        {
            return;
        }

        Transform existingVisual = transform.Find("ButtonVisual");

        if (existingVisual != null)
        {
            visualRoot = existingVisual as RectTransform;
            return;
        }

        GameObject visualObject = new GameObject("ButtonVisual", typeof(RectTransform));
        visualObject.transform.SetParent(transform, false);
        visualRoot = visualObject.GetComponent<RectTransform>();
        visualRoot.anchorMin = Vector2.zero;
        visualRoot.anchorMax = Vector2.one;
        visualRoot.offsetMin = Vector2.zero;
        visualRoot.offsetMax = Vector2.zero;
        visualRoot.SetAsFirstSibling();
        MoveExistingVisualChildren(visualRoot);
        CopyRootImageToVisual(visualObject);
    }

    private void MoveExistingVisualChildren(RectTransform destination)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (child == destination)
            {
                continue;
            }

            child.SetParent(destination, true);
        }
    }

    private void CopyRootImageToVisual(GameObject visualObject)
    {
        Image rootImage = GetComponent<Image>();

        if (rootImage == null)
        {
            return;
        }

        Image visualImage = visualObject.AddComponent<Image>();
        visualImage.sprite = rootImage.sprite;
        visualImage.type = rootImage.type;
        visualImage.preserveAspect = rootImage.preserveAspect;
        visualImage.color = rootImage.color;
        visualImage.material = rootImage.material;
        visualImage.raycastTarget = false;

        rootImage.sprite = null;
        rootImage.color = new Color(1f, 1f, 1f, 0.01f);
        rootImage.raycastTarget = true;
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
