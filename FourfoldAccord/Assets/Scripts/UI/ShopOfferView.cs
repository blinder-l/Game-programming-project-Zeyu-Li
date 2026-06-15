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
    [SerializeField] private Image cardVisualImage;

    private int offerIndex;
    private Action<int> clickedHandler;
    private Button button;

    private void Awake()
    {
        ResolveReferences();
    }

    public void Bind(ShopOffer offer, int index, Action<int> onClicked)
    {
        Bind(offer, index, onClicked, null);
    }

    public void Bind(ShopOffer offer, int index, Action<int> onClicked, JokerSpriteDatabase spriteDatabase)
    {
        Bind(offer, index, onClicked, spriteDatabase, null);
    }

    public void Bind(
        ShopOffer offer,
        int index,
        Action<int> onClicked,
        JokerSpriteDatabase spriteDatabase,
        JokerEffectContext effectContext)
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
        SetSprite(spriteDatabase != null ? spriteDatabase.GetSprite(offer.Joker) : null);
        UpdateTooltip(offer.Joker.Name, $"{offer.Joker.Description}\n\nCost: ${offer.Joker.Cost}", string.Empty, string.Empty);
        visualFeedback?.SetHasVisualContent(cardVisualImage != null && cardVisualImage.enabled && cardVisualImage.sprite != null);
        Debug.Log($"Shop offer tooltip updated: index {offerIndex}, {offer.Joker.Name}");
        SetInteractable(true);
    }

    public void BindPlanet(PlanetShopOffer offer, int index, Action<int> onClicked, PlanetSpriteDatabase spriteDatabase)
    {
        ResolveReferences();
        offerIndex = index;
        clickedHandler = onClicked;

        if (offer == null || offer.PlanetCard == null)
        {
            SetEmpty();
            return;
        }

        if (offer.IsSold)
        {
            SetSold();
            return;
        }

        PlanetCard planetCard = offer.PlanetCard;
        SetText($"{planetCard.Name}\n${planetCard.cost}\n{planetCard.targetHandType}");
        SetSprite(spriteDatabase != null ? spriteDatabase.GetSprite(planetCard) : null);
        UpdateTooltip(planetCard.Name, $"{planetCard.Description}\n\nCost: ${planetCard.cost}", string.Empty, string.Empty);
        visualFeedback?.SetHasVisualContent(true);
        Debug.Log($"Shop consumable tooltip updated: index {offerIndex}, {planetCard.Name}");
        SetInteractable(true);
    }

    public void SetEmpty()
    {
        ResolveReferences();
        SetText("Empty");
        SetSprite(null);
        UpdateTooltip("Empty", "No offer available.", string.Empty, string.Empty);
        visualFeedback?.SetHasVisualContent(false);
        Debug.Log($"Shop offer tooltip updated: index {offerIndex}, Empty");
        SetInteractable(false);
    }

    public void SetSold()
    {
        ResolveReferences();
        SetText("Sold");
        SetSprite(null);
        UpdateTooltip("Sold", "This offer has already been purchased.", string.Empty, string.Empty);
        visualFeedback?.SetHasVisualContent(false);
        Debug.Log($"Shop offer tooltip updated: index {offerIndex}, Sold");
        SetInteractable(false);
    }

    public void SetPlaceholder(string displayName, string effectText, string viewText, Action onClicked)
    {
        ResolveReferences();
        SetText(viewText);
        SetSprite(null);
        UpdateTooltip(displayName, effectText, string.Empty, string.Empty);
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
        EnsureCardVisualImage();

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

    private void EnsureCardVisualImage()
    {
        if (cardVisualRoot == null)
        {
            return;
        }

        if (cardVisualImage == null)
        {
            cardVisualImage = cardVisualRoot.GetComponent<Image>();
        }

        if (cardVisualImage == null)
        {
            cardVisualImage = cardVisualRoot.gameObject.AddComponent<Image>();
        }

        cardVisualImage.raycastTarget = false;
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

    private void SetSprite(Sprite sprite)
    {
        bool hasSeparateOfferImage = offerImage != null && offerImage != cardVisualImage;

        if (cardVisualImage != null)
        {
            cardVisualImage.sprite = sprite;
            cardVisualImage.enabled = sprite != null;
            cardVisualImage.color = hasSeparateOfferImage ? new Color(1f, 1f, 1f, 0.01f) : Color.white;
            cardVisualImage.raycastTarget = false;
        }

        if (hasSeparateOfferImage)
        {
            offerImage.sprite = sprite;
            offerImage.enabled = sprite != null;
            offerImage.color = Color.white;
            offerImage.raycastTarget = false;
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
        UpdateTooltip(displayName, effectText, string.Empty, string.Empty);
    }

    private void UpdateTooltip(
        string displayName,
        string effectText,
        string specificCurrentEffectText,
        string specificPlayCardEffectText)
    {
        tooltipTrigger?.SetTooltip(displayName, effectText, specificCurrentEffectText, specificPlayCardEffectText);
    }
}
