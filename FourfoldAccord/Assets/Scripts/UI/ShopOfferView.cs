using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopOfferView : MonoBehaviour
{
    [SerializeField] private TMP_Text offerText;
    [SerializeField] private Image offerImage;
    [SerializeField] private CardTooltipTrigger tooltipTrigger;
    [SerializeField] private RectTransform cardVisualRoot;
    [SerializeField] private CardVisualFeedback visualFeedback;

    private int offerIndex;
    private Action<int> clickedHandler;
    private Button button;

    private void Awake()
    {
        ResolveReferences();
    }

    public void Bind(ShopOffer offer, int index, Action<int> onClicked)
    {
        ResolveReferences();
        offerIndex = index;
        clickedHandler = onClicked;

        if (offer == null || offer.Joker == null)
        {
            SetEmpty();
            return;
        }

        if (offer.IsSold)
        {
            SetSold();
            return;
        }

        SetText($"{offer.Joker.Name}\n${offer.Joker.Cost}\n{offer.Joker.Description}");
        UpdateTooltip(offer.Joker.Name, $"{offer.Joker.Description}\n\nCost: ${offer.Joker.Cost}");
        visualFeedback?.SetHasVisualContent(true);
        Debug.Log($"Shop offer tooltip updated: index {offerIndex}, {offer.Joker.Name}");
        SetInteractable(true);
    }

    public void SetEmpty()
    {
        ResolveReferences();
        SetText("Empty");
        UpdateTooltip("Empty", "No offer available.");
        visualFeedback?.SetHasVisualContent(false);
        Debug.Log($"Shop offer tooltip updated: index {offerIndex}, Empty");
        SetInteractable(false);
    }

    public void SetSold()
    {
        ResolveReferences();
        SetText("Sold");
        UpdateTooltip("Sold", "This offer has already been purchased.");
        visualFeedback?.SetHasVisualContent(false);
        Debug.Log($"Shop offer tooltip updated: index {offerIndex}, Sold");
        SetInteractable(false);
    }

    public void SetPlaceholder(string displayName, string effectText, string viewText, Action onClicked)
    {
        ResolveReferences();
        SetText(viewText);
        UpdateTooltip(displayName, effectText);
        clickedHandler = _ => onClicked?.Invoke();
        visualFeedback?.SetHasVisualContent(true);
        SetInteractable(true);
    }

    public void SetTooltipController(CardTooltipController tooltipController)
    {
        ResolveReferences();
        tooltipTrigger?.SetTooltipController(tooltipController);
    }

    private void ResolveReferences()
    {
        EnsureCardVisualRoot();

        if (offerText == null)
        {
            Transform offerTextTransform = transform.Find("OfferText");
            offerText = offerTextTransform != null ? offerTextTransform.GetComponent<TMP_Text>() : null;
        }

        if (offerText == null)
        {
            offerText = GetComponentInChildren<TMP_Text>(true);
        }

        if (offerImage == null)
        {
            Transform offerImageTransform = transform.Find("OfferImage");
            offerImage = offerImageTransform != null ? offerImageTransform.GetComponent<Image>() : null;
        }

        Image rootImage = GetComponent<Image>();

        if (rootImage == null)
        {
            rootImage = gameObject.AddComponent<Image>();
            rootImage.color = new Color(1f, 1f, 1f, 0.1f);
        }

        rootImage.raycastTarget = true;

        MoveImageIntoCardVisualIfNeeded();

        if (offerText != null)
        {
            offerText.raycastTarget = false;
            MoveTextIntoCardVisual(offerText);
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        button.targetGraphic = rootImage;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClicked);

        if (tooltipTrigger == null)
        {
            tooltipTrigger = GetComponent<CardTooltipTrigger>();
        }

        if (tooltipTrigger == null)
        {
            tooltipTrigger = gameObject.AddComponent<CardTooltipTrigger>();
            Debug.Log($"Bound tooltip trigger: {gameObject.name}");
        }

        if (visualFeedback == null)
        {
            visualFeedback = GetComponent<CardVisualFeedback>();
        }

        if (visualFeedback == null)
        {
            visualFeedback = gameObject.AddComponent<CardVisualFeedback>();
        }

        visualFeedback.SetVisualRoot(cardVisualRoot);
    }

    private void EnsureCardVisualRoot()
    {
        if (cardVisualRoot != null)
        {
            return;
        }

        Transform visualTransform = transform.Find("CardVisual");

        if (visualTransform == null)
        {
            GameObject visualObject = new GameObject("CardVisual", typeof(RectTransform));
            visualObject.transform.SetParent(transform, false);
            cardVisualRoot = visualObject.GetComponent<RectTransform>();
            cardVisualRoot.anchorMin = Vector2.zero;
            cardVisualRoot.anchorMax = Vector2.one;
            cardVisualRoot.offsetMin = Vector2.zero;
            cardVisualRoot.offsetMax = Vector2.zero;
        }
        else
        {
            cardVisualRoot = visualTransform as RectTransform;
        }
    }

    private void MoveImageIntoCardVisualIfNeeded()
    {
        if (offerImage == null || cardVisualRoot == null || offerImage.gameObject == gameObject)
        {
            return;
        }

        offerImage.raycastTarget = false;

        if (offerImage.transform.parent == cardVisualRoot)
        {
            return;
        }

        offerImage.transform.SetParent(cardVisualRoot, false);
    }

    private void MoveTextIntoCardVisual(TMP_Text text)
    {
        if (text == null || cardVisualRoot == null || text.transform.parent == cardVisualRoot)
        {
            return;
        }

        text.transform.SetParent(cardVisualRoot, false);
    }

    private void HandleClicked()
    {
        clickedHandler?.Invoke(offerIndex);
    }

    private void SetText(string value)
    {
        if (offerText != null)
        {
            offerText.text = value;
        }
    }

    private void SetInteractable(bool isInteractable)
    {
        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }

    private void UpdateTooltip(string displayName, string effectText)
    {
        tooltipTrigger?.SetTooltip(displayName, effectText);
    }
}
