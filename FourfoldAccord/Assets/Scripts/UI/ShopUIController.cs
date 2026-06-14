using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIController : MonoBehaviour
{
    private GameObject shopPanel;
    private Button nextBlindButton;
    private Button rerollButton;
    private TMP_Text rerollButtonText;
    private ShopOfferView[] jokerOfferViews;
    private ShopOfferView voucherOfferView;
    private ShopOfferView consumableOfferView1;
    private ShopOfferView consumableOfferView2;

    public event Action<int> JokerOfferClicked;
    public event Action RerollButtonClicked;
    public event Action NextBlindButtonClicked;

    public void Initialize(Transform canvasRoot)
    {
        if (canvasRoot == null)
        {
            Debug.LogError("Failed to bind ShopPanel: Canvas root is null");
            return;
        }

        BindShopPanel(canvasRoot);
        BindActionButtons(canvasRoot);
        BindJokerOffers(canvasRoot);
        BindPlaceholders(canvasRoot);
        Hide();
    }

    public void ShowShop(IReadOnlyList<ShopOffer> offers)
    {
        if (shopPanel == null)
        {
            Debug.LogError("Cannot show ShopPanel: ShopPanel is not bound");
            return;
        }

        shopPanel.SetActive(true);
        RefreshOffers(offers);
        RefreshRerollText();
        Debug.Log("Shop UI updated.");
    }

    public void Hide()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void RefreshOffers(IReadOnlyList<ShopOffer> offers)
    {
        if (jokerOfferViews == null)
        {
            return;
        }

        for (int i = 0; i < jokerOfferViews.Length; i++)
        {
            ShopOfferView offerView = jokerOfferViews[i];

            if (offerView == null)
            {
                continue;
            }

            if (offers != null && i < offers.Count)
            {
                offerView.Bind(offers[i], i, HandleJokerOfferClicked);
            }
            else
            {
                offerView.SetEmpty();
            }
        }

        Debug.Log("Shop UI updated.");
    }

    private void BindShopPanel(Transform canvasRoot)
    {
        shopPanel = FindObjectIncludingInactive(canvasRoot, "ShopPanel");

        if (shopPanel == null)
        {
            Debug.LogError("Failed to bind ShopPanel");
            return;
        }

        Debug.Log("Bound ShopPanel");
    }

    private void BindActionButtons(Transform canvasRoot)
    {
        nextBlindButton = BindButton(canvasRoot, "ShopNextBlindButton", HandleNextBlindButtonClicked);
        rerollButton = BindButton(canvasRoot, "ShopRerollButton", HandleRerollButtonClicked);
        rerollButtonText = rerollButton != null ? rerollButton.GetComponentInChildren<TMP_Text>(true) : null;
        RefreshRerollText();
    }

    private void BindJokerOffers(Transform canvasRoot)
    {
        jokerOfferViews = new ShopOfferView[ShopManager.ShopOptionCount];

        for (int i = 0; i < jokerOfferViews.Length; i++)
        {
            string offerName = $"ShopJokerOffer{i + 1}";
            GameObject offerObject = FindObjectIncludingInactive(canvasRoot, offerName);

            if (offerObject == null)
            {
                Debug.LogError($"Failed to bind {offerName}");
                continue;
            }

            ShopOfferView offerView = offerObject.GetComponent<ShopOfferView>();

            if (offerView == null)
            {
                offerView = offerObject.AddComponent<ShopOfferView>();
            }

            jokerOfferViews[i] = offerView;
            Debug.Log($"Bound {offerName}");
        }
    }

    private void BindPlaceholders(Transform canvasRoot)
    {
        voucherOfferView = BindPlaceholder(canvasRoot, "ShopVoucherOffer", "Voucher\nComing Soon", HandleVoucherClicked);
        consumableOfferView1 = BindPlaceholder(canvasRoot, "ShopConsumableOffer1", "Consumable\nComing Soon", HandleConsumableClicked);
        consumableOfferView2 = BindPlaceholder(canvasRoot, "ShopConsumableOffer2", "Consumable\nComing Soon", HandleConsumableClicked);
    }

    private ShopOfferView BindPlaceholder(Transform canvasRoot, string objectName, string text, Action onClicked)
    {
        GameObject offerObject = FindObjectIncludingInactive(canvasRoot, objectName);

        if (offerObject == null)
        {
            Debug.LogError($"Failed to bind {objectName}");
            return null;
        }

        ShopOfferView offerView = offerObject.GetComponent<ShopOfferView>();

        if (offerView == null)
        {
            offerView = offerObject.AddComponent<ShopOfferView>();
        }

        offerView.SetPlaceholder(text, onClicked);
        Debug.Log($"Bound {objectName}");
        return offerView;
    }

    private Button BindButton(Transform canvasRoot, string objectName, UnityEngine.Events.UnityAction onClicked)
    {
        GameObject buttonObject = FindObjectIncludingInactive(canvasRoot, objectName);

        if (buttonObject == null)
        {
            Debug.LogError($"Failed to bind {objectName}");
            return null;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        Image image = buttonObject.GetComponent<Image>();

        if (image == null)
        {
            image = buttonObject.AddComponent<Image>();
        }

        image.raycastTarget = true;
        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClicked);
        button.interactable = true;
        DisableTextRaycasts(buttonObject);
        Debug.Log($"Bound {objectName}");
        return button;
    }

    private void RefreshRerollText()
    {
        if (rerollButtonText != null)
        {
            rerollButtonText.text = $"Reroll\n${ShopManager.RerollCost}";
            rerollButtonText.raycastTarget = false;
        }
    }

    private void HandleJokerOfferClicked(int index)
    {
        Debug.Log($"UI ShopJokerOffer clicked: index {index}");
        JokerOfferClicked?.Invoke(index);
    }

    private void HandleRerollButtonClicked()
    {
        Debug.Log("UI ShopRerollButton clicked");
        RerollButtonClicked?.Invoke();
    }

    private void HandleNextBlindButtonClicked()
    {
        Debug.Log("UI ShopNextBlindButton clicked");
        NextBlindButtonClicked?.Invoke();
    }

    private void HandleVoucherClicked()
    {
        Debug.Log("Voucher system not implemented yet.");
    }

    private void HandleConsumableClicked()
    {
        Debug.Log("Consumable system not implemented yet.");
    }

    private void DisableTextRaycasts(GameObject rootObject)
    {
        TMP_Text[] textComponents = rootObject.GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < textComponents.Length; i++)
        {
            textComponents[i].raycastTarget = false;
        }
    }

    private GameObject FindObjectIncludingInactive(Transform root, string objectName)
    {
        Transform child = FindDeepChildIncludingInactive(root, objectName);
        return child != null ? child.gameObject : null;
    }

    private Transform FindDeepChildIncludingInactive(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }
}
