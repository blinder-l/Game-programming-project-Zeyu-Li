using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JokerSlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text jokerNameText;
    [SerializeField] private CardTooltipTrigger tooltipTrigger;
    [SerializeField] private RectTransform cardVisualRoot;
    [SerializeField] private CardVisualFeedback visualFeedback;
    [SerializeField] private Image cardVisualImage;
    [SerializeField] private Image slotImage;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetJoker(JokerBase joker)
    {
        SetJoker(joker, null);
    }

    public void SetJoker(JokerBase joker, JokerSpriteDatabase spriteDatabase)
    {
        SetJoker(joker, spriteDatabase, null);
    }

    public void SetJoker(JokerBase joker, JokerSpriteDatabase spriteDatabase, JokerEffectContext effectContext)
    {
        ResolveReferences();

        if (jokerNameText == null)
        {
            return;
        }

        jokerNameText.text = string.Empty;
        jokerNameText.gameObject.SetActive(false);
        Sprite jokerSprite = spriteDatabase != null ? spriteDatabase.GetSprite(joker) : null;
        SetSprite(jokerSprite);
        string tooltipName = joker != null ? joker.Name : "Empty Joker Slot";
        string tooltipEffect = joker != null ? joker.Description : "No Joker equipped.";
        string tooltipCurrentEffect = joker != null ? joker.GetCurrentEffectText(effectContext) : string.Empty;
        tooltipTrigger?.SetTooltip(tooltipName, tooltipEffect, tooltipCurrentEffect, string.Empty);
        visualFeedback?.SetHasVisualContent(jokerSprite != null);
        SetSlotRootVisible(joker != null);
        Debug.Log($"JokerSlot tooltip updated: {tooltipName}");
    }

    public void SetTooltipController(CardTooltipController tooltipController)
    {
        ResolveReferences();
        tooltipTrigger?.SetTooltipController(tooltipController);
    }

    public void PlayScorePulse()
    {
        ResolveReferences();
        visualFeedback?.PlayScorePulse();
    }

    private void ResolveReferences()
    {
        EnsureCardVisualRoot();
        EnsureCardVisualImage();

        if (jokerNameText == null)
        {
            jokerNameText = GetComponentInChildren<TMP_Text>(true);
        }

        if (jokerNameText == null)
        {
            jokerNameText = CreateNameText();
        }

        if (jokerNameText != null)
        {
            jokerNameText.raycastTarget = false;
            MoveTextIntoCardVisual(jokerNameText);
        }

        slotImage = GetComponent<Image>();

        if (slotImage == null)
        {
            slotImage = gameObject.AddComponent<Image>();
            slotImage.color = new Color(1f, 1f, 1f, 0.01f);
        }

        if (slotImage != null)
        {
            slotImage.raycastTarget = true;
        }

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

    private void MoveTextIntoCardVisual(TMP_Text text)
    {
        if (text == null || cardVisualRoot == null || text.transform.parent == cardVisualRoot)
        {
            return;
        }

        text.transform.SetParent(cardVisualRoot, false);
        RectTransform rectTransform = text.transform as RectTransform;

        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private TMP_Text CreateNameText()
    {
        GameObject textObject = new GameObject("JokerNameText", typeof(RectTransform));
        textObject.transform.SetParent(transform, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.fontSize = 18f;
        text.color = Color.white;
        text.text = "Empty";
        return text;
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

    private void SetSprite(Sprite sprite)
    {
        if (cardVisualImage == null)
        {
            return;
        }

        cardVisualImage.sprite = sprite;
        cardVisualImage.enabled = sprite != null;
        cardVisualImage.color = Color.white;
        cardVisualImage.raycastTarget = false;
    }

    private void SetSlotRootVisible(bool hasJoker)
    {
        if (slotImage != null)
        {
            Color color = slotImage.color;
            color.a = hasJoker ? 0.01f : 0f;
            slotImage.color = color;
            slotImage.raycastTarget = hasJoker;
        }

        if (tooltipTrigger != null)
        {
            tooltipTrigger.enabled = hasJoker;
        }

        if (cardVisualRoot != null)
        {
            cardVisualRoot.gameObject.SetActive(hasJoker);
        }
    }
}
