using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardTooltipController : MonoBehaviour
{
    public static CardTooltipController Instance { get; private set; }

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipNameText;
    [SerializeField] private TMP_Text tooltipEffectText;

    private RectTransform tooltipRectTransform;
    private RectTransform canvasRectTransform;
    private Canvas canvas;
    private bool isInitialized;

    public void Initialize(Transform canvasRoot)
    {
        Instance = this;
        canvas = canvasRoot != null ? canvasRoot.GetComponent<Canvas>() : null;
        canvasRectTransform = canvasRoot as RectTransform;
        tooltipPanel = gameObject;
        tooltipRectTransform = transform as RectTransform;

        if (tooltipPanel == null)
        {
            Debug.LogError("Failed to bind CardTooltipPanel");
            return;
        }

        Debug.Log("Bound CardTooltipPanel");
        tooltipNameText = BindText("TooltipNameText");
        tooltipEffectText = BindText("TooltipEffectText");

        DisableTooltipRaycasts();
        HideTooltip();
        isInitialized = tooltipNameText != null && tooltipEffectText != null;
        Debug.Log("CardTooltipController initialized");
    }

    public void ShowTooltip(string displayName, string effectText, RectTransform anchor)
    {
        if (!isInitialized)
        {
            Debug.LogError("Cannot show tooltip: CardTooltipController not initialized");
            return;
        }

        string safeDisplayName = string.IsNullOrWhiteSpace(displayName) ? "Unknown" : displayName;
        string safeEffectText = string.IsNullOrWhiteSpace(effectText) ? "No effect." : effectText;
        tooltipNameText.text = safeDisplayName;
        tooltipEffectText.text = safeEffectText;
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.SetAsLastSibling();
        PositionTooltip(anchor);
        Debug.Log($"Tooltip shown: {safeDisplayName}");
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        Debug.Log("Tooltip hidden");
    }

    private TMP_Text BindText(string objectName)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == objectName)
            {
                texts[i].raycastTarget = false;
                Debug.Log($"Bound {objectName}");
                return texts[i];
            }
        }

        Debug.LogError($"Failed to bind {objectName}");
        return null;
    }

    private void PositionTooltip(RectTransform anchor)
    {
        if (anchor == null || tooltipRectTransform == null || canvasRectTransform == null)
        {
            return;
        }

        Vector3[] anchorCorners = new Vector3[4];
        anchor.GetWorldCorners(anchorCorners);
        Vector3 anchorBottomCenter = (anchorCorners[0] + anchorCorners[3]) * 0.5f;
        Vector3 anchorTopCenter = (anchorCorners[1] + anchorCorners[2]) * 0.5f;
        Vector3 preferredWorldPosition = anchorBottomCenter + new Vector3(0f, -12f, 0f);
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                RectTransformUtility.WorldToScreenPoint(eventCamera, preferredWorldPosition),
                eventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        tooltipRectTransform.anchoredPosition = localPoint;
        Vector3[] tooltipCorners = new Vector3[4];
        tooltipRectTransform.GetWorldCorners(tooltipCorners);

        if (tooltipCorners[0].y < 0f)
        {
            Vector3 fallbackWorldPosition = anchorTopCenter + new Vector3(0f, 12f, 0f);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    RectTransformUtility.WorldToScreenPoint(eventCamera, fallbackWorldPosition),
                    eventCamera,
                    out Vector2 fallbackPoint))
            {
                tooltipRectTransform.anchoredPosition = fallbackPoint;
            }
        }
    }

    private void DisableTooltipRaycasts()
    {
        Image panelImage = GetComponent<Image>();

        if (panelImage != null)
        {
            panelImage.raycastTarget = false;
        }

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].raycastTarget = false;
        }
    }
}
