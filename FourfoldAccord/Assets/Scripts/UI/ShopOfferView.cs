using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopOfferView : MonoBehaviour
{
    [SerializeField] private TMP_Text offerText;
    [SerializeField] private Image offerImage;

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
        SetInteractable(true);
    }

    public void SetEmpty()
    {
        ResolveReferences();
        SetText("Empty");
        SetInteractable(false);
    }

    public void SetSold()
    {
        ResolveReferences();
        SetText("Sold");
        SetInteractable(false);
    }

    public void SetPlaceholder(string value, Action onClicked)
    {
        ResolveReferences();
        SetText(value);
        clickedHandler = _ => onClicked?.Invoke();
        SetInteractable(true);
    }

    private void ResolveReferences()
    {
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

        if (offerImage == null)
        {
            offerImage = GetComponent<Image>();
        }

        if (offerImage == null)
        {
            offerImage = gameObject.AddComponent<Image>();
            offerImage.color = new Color(1f, 1f, 1f, 0.1f);
        }

        if (offerImage != null)
        {
            offerImage.raycastTarget = true;
        }

        if (offerText != null)
        {
            offerText.raycastTarget = false;
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        button.targetGraphic = offerImage;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClicked);
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
}
