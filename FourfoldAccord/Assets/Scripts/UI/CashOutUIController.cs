using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CashOutUIController : MonoBehaviour
{
    private GameObject cashOutPanel;
    private Button cashOutButton;
    private TMP_Text cashOutButtonText;
    private TMP_Text targetScoreText;
    private TMP_Text blindRewardText;
    private TMP_Text suitGoldText;
    private TMP_Text interestText;
    private TMP_Text discardBonusText;

    public event Action CashOutButtonClicked;

    public void Initialize(Transform canvasRoot)
    {
        if (canvasRoot == null)
        {
            Debug.LogError("Failed to bind CashOutPanel: Canvas root is null");
            return;
        }

        BindCashOutPanel(canvasRoot);
        Hide();
    }

    public void ShowCashOut(
        int targetScore,
        int currentScore,
        int fixedBlindReward,
        int suitGoldThisBlind,
        int interest,
        int discardBonus,
        int cashOutTotal)
    {
        if (cashOutPanel == null)
        {
            Debug.LogError("Cannot show CashOutPanel: CashOutPanel is not bound");
            return;
        }

        cashOutPanel.SetActive(true);
        SetText(cashOutButtonText, $"Cash Out: ${cashOutTotal}");
        SetText(targetScoreText, $"Target: {targetScore}\nScore: {currentScore}");
        SetText(blindRewardText, new string('$', fixedBlindReward));
        SetText(suitGoldText, $"Suit Gold: ${suitGoldThisBlind}");
        SetText(interestText, $"Interest: ${interest}");
        SetText(discardBonusText, $"Discard Bonus: ${discardBonus}");

        if (cashOutButton != null)
        {
            cashOutButton.interactable = true;
        }
    }

    public void Hide()
    {
        if (cashOutPanel != null)
        {
            cashOutPanel.SetActive(false);
        }
    }

    public void SetButtonInteractable(bool isInteractable)
    {
        if (cashOutButton != null)
        {
            cashOutButton.interactable = isInteractable;
        }
    }

    private void BindCashOutPanel(Transform canvasRoot)
    {
        cashOutPanel = FindObjectIncludingInactive(canvasRoot, "CashOutPanel");

        if (cashOutPanel == null)
        {
            Debug.LogError("Failed to bind CashOutPanel");
            return;
        }

        Debug.Log("Bound CashOutPanel");
        cashOutButton = BindButton(canvasRoot, "CashOutButton");
        cashOutButtonText = BindButtonText(cashOutButton);
        targetScoreText = BindText(canvasRoot, "CashOutTargetScoreText");
        blindRewardText = BindText(canvasRoot, "CashOutBlindRewardText");
        suitGoldText = BindText(canvasRoot, "CashOutSuitGoldText");
        interestText = BindText(canvasRoot, "CashOutInterestText");
        discardBonusText = BindText(canvasRoot, "CashOutDiscardBonusText");
    }

    private Button BindButton(Transform canvasRoot, string objectName)
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
        button.onClick.AddListener(HandleCashOutButtonClicked);
        button.interactable = true;
        DisableTextRaycasts(buttonObject);
        Debug.Log("Bound CashOutButton");
        return button;
    }

    private TMP_Text BindButtonText(Button button)
    {
        if (button == null)
        {
            Debug.LogError("Failed to bind CashOutButton text");
            return null;
        }

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);

        if (text == null)
        {
            Debug.LogError("Failed to bind CashOutButton text");
            return null;
        }

        text.raycastTarget = false;
        Debug.Log("Bound CashOutButton text");
        return text;
    }

    private TMP_Text BindText(Transform canvasRoot, string objectName)
    {
        GameObject textObject = FindObjectIncludingInactive(canvasRoot, objectName);

        if (textObject == null)
        {
            Debug.LogError($"Failed to bind {objectName}");
            return null;
        }

        TMP_Text text = textObject.GetComponent<TMP_Text>();

        if (text == null)
        {
            text = textObject.GetComponentInChildren<TMP_Text>(true);
        }

        if (text == null)
        {
            Debug.LogError($"Failed to bind {objectName}");
            return null;
        }

        text.raycastTarget = false;
        Debug.Log($"Bound {objectName}");
        return text;
    }

    private void HandleCashOutButtonClicked()
    {
        Debug.Log("UI CashOutButton clicked");
        CashOutButtonClicked?.Invoke();
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
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
