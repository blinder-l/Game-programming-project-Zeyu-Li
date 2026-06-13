using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private float selectedYOffset = 24f;

    private int handIndex = -1;
    private Action<int> clickedHandler;
    private RectTransform rectTransform;
    private Vector2 baseAnchoredPosition;
    private Vector3 baseScale;
    private bool hasCachedBaseTransform;
    private bool hasCard;

    public void SetCard(int index, PlayingCard card, CardSpriteDatabase spriteDatabase, Action<int> onClicked)
    {
        CacheBaseTransformIfNeeded();

        if (card == null)
        {
            Clear();
            return;
        }

        handIndex = index;
        clickedHandler = onClicked;
        hasCard = true;
        gameObject.SetActive(true);

        Sprite cardSprite = spriteDatabase != null ? spriteDatabase.GetSprite(card) : null;

        if (cardImage != null)
        {
            cardImage.sprite = cardSprite;
            cardImage.enabled = cardSprite != null;
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
        CacheBaseTransformIfNeeded();
        handIndex = -1;
        clickedHandler = null;
        hasCard = false;
        SetSelectedVisual(false);
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasCard || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        clickedHandler?.Invoke(handIndex);
    }

    private void SetSelectedVisual(bool isSelected)
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(isSelected);
        }

        if (rectTransform != null)
        {
            Vector2 selectedOffset = isSelected ? new Vector2(0f, selectedYOffset) : Vector2.zero;
            rectTransform.anchoredPosition = baseAnchoredPosition + selectedOffset;
        }
        else
        {
            transform.localScale = baseScale;
        }
    }

    private void CacheBaseTransformIfNeeded()
    {
        if (hasCachedBaseTransform)
        {
            return;
        }

        rectTransform = transform as RectTransform;

        if (rectTransform != null)
        {
            baseAnchoredPosition = rectTransform.anchoredPosition;
        }

        baseScale = transform.localScale;
        hasCachedBaseTransform = true;
    }
}
