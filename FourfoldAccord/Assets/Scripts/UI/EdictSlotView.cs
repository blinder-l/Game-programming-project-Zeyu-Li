using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EdictSlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text edictNameText;
    [SerializeField] private CardTooltipTrigger tooltipTrigger;
    [SerializeField] private RectTransform cardVisualRoot;
    [SerializeField] private CardVisualFeedback visualFeedback;
    [SerializeField] private Image cardVisualImage;
    [SerializeField] private Image slotImage;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetEdict(EdictCard edictCard, EdictSpriteDatabase spriteDatabase)
    {
        ResolveReferences();

        if (edictNameText != null)
        {
            edictNameText.text = string.Empty;
            edictNameText.gameObject.SetActive(false);
        }

        Sprite edictSprite = spriteDatabase != null ? spriteDatabase.GetSprite(edictCard) : null;
        SetSprite(edictSprite);

        if (edictCard != null)
        {
            tooltipTrigger?.SetTooltip(edictCard.Name, edictCard.Description, string.Empty, string.Empty);
        }
        else
        {
            tooltipTrigger?.SetTooltip("Empty Edict Slot", "No Edict purchased.", string.Empty, string.Empty);
        }

        visualFeedback?.SetHasVisualContent(edictSprite != null);
        SetSlotRootVisible(edictCard != null);
        Debug.Log($"EdictSlot tooltip updated: {(edictCard != null ? edictCard.Name : "Empty")}");
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

        if (edictNameText == null)
        {
            edictNameText = GetComponentInChildren<TMP_Text>(true);
        }

        if (edictNameText != null)
        {
            edictNameText.raycastTarget = false;
            MoveTextIntoCardVisual(edictNameText);
        }

        slotImage = GetComponent<Image>();

        if (slotImage == null)
        {
            slotImage = gameObject.AddComponent<Image>();
            slotImage.color = new Color(1f, 1f, 1f, 0.01f);
        }

        if (tooltipTrigger == null)
        {
            tooltipTrigger = GetComponent<CardTooltipTrigger>();
        }

        if (tooltipTrigger == null)
        {
            tooltipTrigger = gameObject.AddComponent<CardTooltipTrigger>();
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

    private void MoveTextIntoCardVisual(TMP_Text text)
    {
        if (text == null || cardVisualRoot == null || text.transform.parent == cardVisualRoot)
        {
            return;
        }

        text.transform.SetParent(cardVisualRoot, false);
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

    private void SetSlotRootVisible(bool hasEdict)
    {
        if (slotImage != null)
        {
            Color color = slotImage.color;
            color.a = hasEdict ? 0.01f : 0f;
            slotImage.color = color;
            slotImage.raycastTarget = hasEdict;
        }

        if (tooltipTrigger != null)
        {
            tooltipTrigger.enabled = hasEdict;
        }

        if (cardVisualRoot != null)
        {
            cardVisualRoot.gameObject.SetActive(hasEdict);
        }
    }
}
