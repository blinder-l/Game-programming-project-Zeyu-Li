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
            cardImage.raycastTarget = true;
        }

        if (cardNameText != null)
        {
            cardNameText.text = card.GetDisplayName();
            cardNameText.gameObject.SetActive(cardSprite == null);
        }

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

        SetSelectedVisual(false);
        gameObject.SetActive(false);
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
        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }

        if (cardImage == null)
        {
            cardImage = GetComponentInChildren<Image>(true);
        }

        if (cardImage != null)
        {
            cardImage.raycastTarget = true;
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

        if (button != null && button.targetGraphic == null)
        {
            button.targetGraphic = cardImage;
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
    }
}
