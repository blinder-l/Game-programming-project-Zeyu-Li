using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string displayName = "Unknown";
    [SerializeField] private string effectText = "No effect.";

    private CardTooltipController tooltipController;
    private RectTransform rectTransform;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetTooltip(string newDisplayName, string newEffectText)
    {
        displayName = string.IsNullOrWhiteSpace(newDisplayName) ? "Unknown" : newDisplayName;
        effectText = string.IsNullOrWhiteSpace(newEffectText) ? "No effect." : newEffectText;
        ResolveReferences();
    }

    public void SetTooltipController(CardTooltipController controller)
    {
        tooltipController = controller;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ResolveReferences();
        tooltipController?.ShowTooltip(displayName, effectText, rectTransform);
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
