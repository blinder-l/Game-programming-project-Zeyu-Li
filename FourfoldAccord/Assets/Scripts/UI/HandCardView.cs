using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandCardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private Color normalTint = Color.white;
    [SerializeField] private Color selectedTint = new Color(1f, 0.9f, 0.35f, 1f);
    [SerializeField] private RectTransform cardVisualRoot;
    [SerializeField] private CardVisualFeedback visualFeedback;
    [SerializeField] private CardTooltipTrigger tooltipTrigger;

    private PlayingCard boundCard;
    private Action<PlayingCard> clickedHandler;
    private Button button;
    private bool hasCard;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (button != null)
        {
            button.onClick.AddListener(HandleButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleButtonClicked);
        }
    }

    public void SetCard(int index, PlayingCard card, CardSpriteDatabase spriteDatabase, Action<PlayingCard> onClicked)
    {
        ResolveReferences();

        if (card == null)
        {
            Clear();
            return;
        }

        boundCard = card;
        clickedHandler = onClicked;
        hasCard = true;
        gameObject.SetActive(true);
        SetButtonInteractable(clickedHandler != null);

        Sprite cardSprite = spriteDatabase != null ? spriteDatabase.GetSprite(card) : null;

        if (cardImage != null)
        {
            cardImage.sprite = cardSprite;
            cardImage.enabled = true;
            cardImage.raycastTarget = false;
        }

        if (cardNameText != null)
        {
            cardNameText.text = card.GetDisplayName();
            cardNameText.gameObject.SetActive(cardSprite == null);
        }

        visualFeedback?.SetHasVisualContent(cardSprite != null);
        UpdatePlayingCardTooltip(card);
        SetSelectedVisual(card.isSelected);
    }

    public void Clear()
    {
        ResolveReferences();
        boundCard = null;
        clickedHandler = null;
        hasCard = false;
        SetButtonInteractable(false);

        if (cardImage != null)
        {
            cardImage.sprite = null;
        }

        visualFeedback?.SetHasVisualContent(false);
        visualFeedback?.ResetVisualImmediate();
        tooltipTrigger?.SetTooltip("Empty", "No card.", string.Empty, string.Empty);
        SetSelectedVisual(false);
        gameObject.SetActive(false);
    }

    public void ShowEmptySlot()
    {
        ResolveReferences();
        boundCard = null;
        clickedHandler = null;
        hasCard = false;
        gameObject.SetActive(true);
        SetButtonInteractable(false);

        if (cardImage != null)
        {
            cardImage.sprite = null;
            cardImage.enabled = false;
        }

        if (cardNameText != null)
        {
            cardNameText.text = string.Empty;
            cardNameText.gameObject.SetActive(false);
        }

        visualFeedback?.SetHasVisualContent(false);
        visualFeedback?.ResetVisualImmediate();
        tooltipTrigger?.SetTooltip("Empty", "No card.", string.Empty, string.Empty);
        SetSelectedVisual(false);
    }

    public void RefreshBasePosition()
    {
        ResolveReferences();
        visualFeedback?.RefreshBasePosition();
    }

    private void HandleButtonClicked()
    {
        if (!hasCard)
        {
            return;
        }

        clickedHandler?.Invoke(boundCard);
    }

    private void ResolveReferences()
    {
        EnsureCardVisualRoot();

        Image visualImage = cardVisualRoot != null ? cardVisualRoot.GetComponent<Image>() : null;

        if (visualImage == null && cardVisualRoot != null)
        {
            visualImage = cardVisualRoot.gameObject.AddComponent<Image>();
        }

        cardImage = visualImage;

        Image rootImage = GetComponent<Image>();

        if (rootImage == null)
        {
            rootImage = gameObject.AddComponent<Image>();
        }

        rootImage.sprite = null;
        rootImage.color = new Color(1f, 1f, 1f, 0.01f);
        rootImage.raycastTarget = true;

        if (cardImage != null && cardImage != rootImage)
        {
            cardImage.raycastTarget = false;
        }

        if (visualFeedback == null)
        {
            visualFeedback = GetComponent<CardVisualFeedback>();
        }

        if (visualFeedback == null)
        {
            visualFeedback = gameObject.AddComponent<CardVisualFeedback>();
        }

        if (visualFeedback != null)
        {
            visualFeedback.SetVisualRoot(cardVisualRoot);
        }

        if (tooltipTrigger == null)
        {
            tooltipTrigger = GetComponent<CardTooltipTrigger>();
        }

        if (tooltipTrigger == null)
        {
            tooltipTrigger = gameObject.AddComponent<CardTooltipTrigger>();
        }

        if (cardNameText != null)
        {
            cardNameText.raycastTarget = false;
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        if (button != null)
        {
            button.targetGraphic = rootImage;
        }
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

    private void SetButtonInteractable(bool isInteractable)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = isInteractable;
    }

    private void SetSelectedVisual(bool isSelected)
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(isSelected);
        }

        if (cardImage != null)
        {
            cardImage.color = isSelected ? selectedTint : normalTint;
        }

        visualFeedback?.SetSelected(isSelected);
    }

    private void UpdatePlayingCardTooltip(PlayingCard card)
    {
        if (tooltipTrigger == null || card == null)
        {
            return;
        }

        tooltipTrigger.SetTooltip(
            GetModifierNameText(card),
            string.Empty,
            string.Empty,
            GetModifierEffectText(card));
    }

    private string GetModifierNameText(PlayingCard card)
    {
        if (card == null)
        {
            return "Unknown Card";
        }

        string modifierName = string.Empty;

        if (card.enhancement != CardEnhancement.None)
        {
            modifierName = GetEnhancementDisplayName(card.enhancement);
        }

        if (card.seal != CardSeal.None)
        {
            modifierName = AppendTooltipPart(modifierName, GetSealDisplayName(card.seal));
        }

        if (card.permanentBonusChips != 0)
        {
            modifierName = AppendTooltipPart(modifierName, $"{FormatSignedNumber(card.permanentBonusChips)} Chips");
        }

        if (card.isDebuffed)
        {
            modifierName = AppendTooltipPart(modifierName, "Debuffed");
        }

        return string.IsNullOrEmpty(modifierName) ? "Normal Card" : modifierName;
    }

    private string GetModifierEffectText(PlayingCard card)
    {
        if (card == null)
        {
            return "No card.";
        }

        string effectText = string.Empty;

        if (card.enhancement != CardEnhancement.None)
        {
            effectText = AppendTooltipLine(effectText, GetEnhancementEffectText(card.enhancement));
        }

        if (card.seal != CardSeal.None)
        {
            effectText = AppendTooltipLine(effectText, GetSealEffectText(card.seal));
        }

        if (card.permanentBonusChips != 0)
        {
            effectText = AppendTooltipLine(effectText, $"Adds {FormatSignedNumber(card.permanentBonusChips)} Chips when scored.");
        }

        if (card.isDebuffed)
        {
            effectText = AppendTooltipLine(effectText, "Debuffed: special effects may be disabled.");
        }

        return string.IsNullOrEmpty(effectText) ? "No special effects." : effectText;
    }

    private string GetEnhancementDisplayName(CardEnhancement enhancement)
    {
        switch (enhancement)
        {
            case CardEnhancement.Gold:
                return "Gold Card";
            case CardEnhancement.Stone:
                return "Stone Card";
            case CardEnhancement.Lucky:
                return "Lucky Card";
            case CardEnhancement.Bonus:
                return "Bonus Card";
            case CardEnhancement.Mult:
                return "Mult Card";
            case CardEnhancement.Wild:
                return "Wild Card";
            case CardEnhancement.Glass:
                return "Glass Card";
            case CardEnhancement.Steel:
                return "Steel Card";
            default:
                return "Normal Card";
        }
    }

    private string GetEnhancementEffectText(CardEnhancement enhancement)
    {
        switch (enhancement)
        {
            case CardEnhancement.Gold:
                return "Held at Cash Out: +$3.";
            case CardEnhancement.Stone:
                return "No rank or suit. Scores +50 Chips.";
            case CardEnhancement.Lucky:
                return "When scored: 1/5 +20 Mult, 1/15 +$20.";
            case CardEnhancement.Bonus:
            case CardEnhancement.Mult:
            case CardEnhancement.Wild:
            case CardEnhancement.Glass:
            case CardEnhancement.Steel:
                return "Reserved effect; not active yet.";
            default:
                return string.Empty;
        }
    }

    private string GetSealDisplayName(CardSeal seal)
    {
        switch (seal)
        {
            case CardSeal.Gold:
                return "Gold Seal";
            case CardSeal.Red:
                return "Red Seal";
            case CardSeal.Blue:
                return "Blue Seal";
            case CardSeal.Purple:
                return "Purple Seal";
            default:
                return string.Empty;
        }
    }

    private string GetSealEffectText(CardSeal seal)
    {
        switch (seal)
        {
            case CardSeal.Gold:
                return "When scored: +$3.";
            case CardSeal.Red:
                return "When scored: retrigger this card once.";
            case CardSeal.Blue:
                return "Held at Cash Out: would create a Planet card.";
            case CardSeal.Purple:
                return "When discarded: would create a Tarot card.";
            default:
                return string.Empty;
        }
    }

    private string AppendTooltipPart(string currentText, string nextPart)
    {
        if (string.IsNullOrWhiteSpace(nextPart))
        {
            return currentText;
        }

        return string.IsNullOrEmpty(currentText) ? nextPart : $"{currentText} / {nextPart}";
    }

    private string AppendTooltipLine(string currentText, string nextLine)
    {
        if (string.IsNullOrWhiteSpace(nextLine))
        {
            return currentText;
        }

        return string.IsNullOrEmpty(currentText) ? nextLine : $"{currentText}\n{nextLine}";
    }

    private string FormatSignedNumber(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }
}
