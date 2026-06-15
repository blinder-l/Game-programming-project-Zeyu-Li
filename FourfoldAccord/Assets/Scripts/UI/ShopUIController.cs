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
    private CardTooltipController tooltipController;
    private JokerSpriteDatabase jokerSpriteDatabase;
    private PlanetSpriteDatabase planetSpriteDatabase;
    private JokerEffectContext jokerEffectContext = new JokerEffectContext();
    private Func<bool> canUseShopInput;
    private Func<string, string> getBlockedMessage;

    public event Action<int> JokerOfferClicked;
    public event Action<int> ConsumableOfferClicked;
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
        ResolveJokerSpriteDatabase();
        ResolvePlanetSpriteDatabase();
        BindActionButtons(canvasRoot);
        BindJokerOffers(canvasRoot);
        BindPlaceholders(canvasRoot);
        Hide();
    }

    public void ShowShop(IReadOnlyList<ShopOffer> offers)
    {
        ShowShop(offers, null);
    }

    public void ShowShop(IReadOnlyList<ShopOffer> offers, IReadOnlyList<PlanetShopOffer> consumableOffers)
    {
        if (shopPanel == null)
        {
            Debug.LogError("Cannot show ShopPanel: ShopPanel is not bound");
            return;
        }

        shopPanel.SetActive(true);
        RefreshOffers(offers);
        RefreshConsumableOffers(consumableOffers);
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
        ResolveJokerSpriteDatabase();

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
                offerView.SetTooltipController(tooltipController);
                offerView.Bind(offers[i], i, HandleJokerOfferClicked, jokerSpriteDatabase, jokerEffectContext);
            }
            else
            {
                offerView.SetEmpty();
            }
        }

        Debug.Log("Shop UI updated.");
    }

    public void RefreshConsumableOffers(IReadOnlyList<PlanetShopOffer> consumableOffers)
    {
        ResolvePlanetSpriteDatabase();
        ShopOfferView[] consumableViews = GetConsumableOfferViews();

        for (int i = 0; i < consumableViews.Length; i++)
        {
            ShopOfferView offerView = consumableViews[i];

            if (offerView == null)
            {
                continue;
            }

            offerView.SetTooltipController(tooltipController);

            if (consumableOffers != null && i < consumableOffers.Count)
            {
                offerView.BindPlanet(consumableOffers[i], i, HandleConsumableOfferClicked, planetSpriteDatabase);
            }
            else
            {
                offerView.SetEmpty();
            }
        }

        Debug.Log("Shop consumable UI updated.");
    }

    public void SetTooltipController(CardTooltipController controller)
    {
        tooltipController = controller;

        if (jokerOfferViews != null)
        {
            for (int i = 0; i < jokerOfferViews.Length; i++)
            {
                jokerOfferViews[i]?.SetTooltipController(tooltipController);
            }
        }

        voucherOfferView?.SetTooltipController(tooltipController);
        consumableOfferView1?.SetTooltipController(tooltipController);
        consumableOfferView2?.SetTooltipController(tooltipController);
    }

    public void SetJokerTooltipContext(JokerEffectContext context)
    {
        if (context != null)
        {
            jokerEffectContext = context;
        }
    }

    public void SetInputGuard(Func<bool> canUseInput, Func<string, string> blockedMessageGetter)
    {
        canUseShopInput = canUseInput;
        getBlockedMessage = blockedMessageGetter;
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
        voucherOfferView = BindPlaceholder(
            canvasRoot,
            "ShopVoucherOffer",
            "Voucher",
            "Future upgrade card. Coming soon.",
            "Voucher\nComing Soon",
            HandleVoucherClicked);
        consumableOfferView1 = BindConsumableOffer(canvasRoot, "ShopConsumableOffer1");
        consumableOfferView2 = BindConsumableOffer(canvasRoot, "ShopConsumableOffer2");
    }

    private ShopOfferView BindPlaceholder(
        Transform canvasRoot,
        string objectName,
        string displayName,
        string effectText,
        string viewText,
        Action onClicked)
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

        offerView.SetTooltipController(tooltipController);
        offerView.SetPlaceholder(displayName, effectText, viewText, onClicked);
        Debug.Log($"Bound {objectName}");
        return offerView;
    }

    private ShopOfferView BindConsumableOffer(Transform canvasRoot, string objectName)
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

        offerView.SetTooltipController(tooltipController);
        offerView.SetEmpty();
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

        if (!CanUseShopInput("purchase"))
        {
            return;
        }

        JokerOfferClicked?.Invoke(index);
    }

    private void HandleRerollButtonClicked()
    {
        Debug.Log("UI ShopRerollButton clicked");

        if (!CanUseShopInput("reroll"))
        {
            return;
        }

        RerollButtonClicked?.Invoke();
    }

    private void HandleNextBlindButtonClicked()
    {
        Debug.Log("UI ShopNextBlindButton clicked");

        if (!CanUseShopInput("go to next blind"))
        {
            return;
        }

        NextBlindButtonClicked?.Invoke();
    }

    private void HandleVoucherClicked()
    {
        if (!CanUseShopInput("purchase"))
        {
            return;
        }

        Debug.Log("Voucher system not implemented yet.");
    }

    private void HandleConsumableOfferClicked(int index)
    {
        Debug.Log($"UI ShopConsumableOffer clicked: index {index}");

        if (!CanUseShopInput("purchase"))
        {
            return;
        }

        ConsumableOfferClicked?.Invoke(index);
    }

    private ShopOfferView[] GetConsumableOfferViews()
    {
        return new[] { consumableOfferView1, consumableOfferView2 };
    }

    private void ResolvePlanetSpriteDatabase()
    {
        if (planetSpriteDatabase != null)
        {
            return;
        }

        planetSpriteDatabase = FindFirstObjectByType<PlanetSpriteDatabase>();

        if (planetSpriteDatabase == null)
        {
            planetSpriteDatabase = gameObject.AddComponent<PlanetSpriteDatabase>();
        }
    }

    private bool CanUseShopInput(string action)
    {
        if (canUseShopInput == null || canUseShopInput())
        {
            return true;
        }

        string blockedMessage = getBlockedMessage != null ? getBlockedMessage(action) : $"Cannot {action}: shop input is blocked.";
        Debug.Log(blockedMessage);
        return false;
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

    private void ResolveJokerSpriteDatabase()
    {
        if (jokerSpriteDatabase != null)
        {
            return;
        }

        jokerSpriteDatabase = FindFirstObjectByType<JokerSpriteDatabase>();

        if (jokerSpriteDatabase == null)
        {
            jokerSpriteDatabase = gameObject.AddComponent<JokerSpriteDatabase>();
        }
    }
}
