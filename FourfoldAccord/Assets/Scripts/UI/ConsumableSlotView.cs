using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableSlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text consumableNameText;
    [SerializeField] private CardTooltipTrigger tooltipTrigger;
    [SerializeField] private RectTransform cardVisualRoot;
    [SerializeField] private CardVisualFeedback visualFeedback;
    [SerializeField] private Image cardVisualImage;
    [SerializeField] private Image slotImage;
    [SerializeField] private Button slotButton;

    private int slotIndex = -1;
    private SpellCard currentSpell;
    private Action<int> onSlotClicked;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetSpell(SpellCard spell, SpellSpriteDatabase spriteDatabase)
    {
        ResolveReferences();
        currentSpell = spell;

        if (consumableNameText != null)
        {
            consumableNameText.text = string.Empty;
            consumableNameText.gameObject.SetActive(false);
        }

        Sprite spellSprite = spriteDatabase != null ? spriteDatabase.GetSprite(spell) : null;
        SetSprite(spellSprite);
        string tooltipName = spell != null ? spell.Name : "Empty Consumable Slot";
        string tooltipEffect = spell != null ? spell.Description : "No consumable card held.";
        tooltipTrigger?.SetTooltip(tooltipName, tooltipEffect, string.Empty, string.Empty);
        visualFeedback?.SetHasVisualContent(spellSprite != null);
        SetSlotRootVisible(spell != null);
        ConfigureButton(spell != null);
        Debug.Log($"ConsumableSlot tooltip updated: {tooltipName}");
    }

    public void SetClickHandler(int index, Action<int> clickHandler)
    {
        ResolveReferences();
        slotIndex = index;
        onSlotClicked = clickHandler;
        ConfigureButton(currentSpell != null);
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

        if (consumableNameText == null)
        {
            consumableNameText = GetComponentInChildren<TMP_Text>(true);
        }

        if (consumableNameText != null)
        {
            consumableNameText.raycastTarget = false;
            MoveTextIntoCardVisual(consumableNameText);
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

        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }

        if (slotButton == null)
        {
            slotButton = gameObject.AddComponent<Button>();
        }

        slotButton.targetGraphic = slotImage;
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

    private void SetSlotRootVisible(bool hasSpell)
    {
        if (slotImage != null)
        {
            Color color = slotImage.color;
            color.a = hasSpell ? 0.01f : 0f;
            slotImage.color = color;
            slotImage.raycastTarget = hasSpell;
        }

        if (tooltipTrigger != null)
        {
            tooltipTrigger.enabled = hasSpell;
        }

        if (cardVisualRoot != null)
        {
            cardVisualRoot.gameObject.SetActive(hasSpell);
        }
    }

    private void ConfigureButton(bool hasSpell)
    {
        if (slotButton == null)
        {
            return;
        }

        slotButton.onClick.RemoveAllListeners();

        if (hasSpell)
        {
            slotButton.onClick.AddListener(HandleSlotClicked);
        }

        slotButton.interactable = hasSpell;
    }

    private void HandleSlotClicked()
    {
        if (currentSpell == null)
        {
            return;
        }

        visualFeedback?.PlayScorePulse();
        Debug.Log($"ConsumableSlot{slotIndex + 1} clicked: {currentSpell.Name}");
        onSlotClicked?.Invoke(slotIndex);
    }
}
