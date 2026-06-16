using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string displayName = "Unknown";
    [SerializeField] private string effectText = "No effect.";
    [SerializeField] private string specificCurrentEffectText = string.Empty;
    [SerializeField] private string specificPlayCardEffectText = string.Empty;

    private CardTooltipController tooltipController;
    private RectTransform rectTransform;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetTooltip(string newDisplayName, string newEffectText)
    {
        SetTooltip(newDisplayName, newEffectText, string.Empty, string.Empty);
    }

    public void SetTooltip(
        string newDisplayName,
        string newEffectText,
        string newSpecificCurrentEffectText,
        string newSpecificPlayCardEffectText)
    {
        displayName = string.IsNullOrWhiteSpace(newDisplayName) ? "Unknown" : newDisplayName;
        bool hasSpecificText =
            !string.IsNullOrWhiteSpace(newSpecificCurrentEffectText) ||
            !string.IsNullOrWhiteSpace(newSpecificPlayCardEffectText);
        effectText = string.IsNullOrWhiteSpace(newEffectText) && !hasSpecificText ? "No effect." : newEffectText;
        specificCurrentEffectText = string.IsNullOrWhiteSpace(newSpecificCurrentEffectText) ? string.Empty : newSpecificCurrentEffectText;
        specificPlayCardEffectText = string.IsNullOrWhiteSpace(newSpecificPlayCardEffectText) ? string.Empty : newSpecificPlayCardEffectText;
        ResolveReferences();
    }

    public void SetTooltipController(CardTooltipController controller)
    {
        tooltipController = controller;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ResolveReferences();
        tooltipController?.ShowTooltip(displayName, effectText, specificCurrentEffectText, specificPlayCardEffectText, rectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResolveReferences();
        tooltipController?.HideTooltip();
    }

    private void ResolveReferences()
    {
        if (tooltipController == null)
        {
            tooltipController = CardTooltipController.Instance;
        }

        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        Image image = GetComponent<Image>();

        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.01f);
        }

        image.raycastTarget = true;
    }
}
