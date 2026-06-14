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
        SetSelectedVisual(false);
        gameObject.SetActive(false);
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
}
