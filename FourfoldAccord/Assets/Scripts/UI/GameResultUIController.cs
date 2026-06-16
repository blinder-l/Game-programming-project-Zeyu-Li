using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUIController : MonoBehaviour
{
    private GameObject panelObject;
    private TMP_Text titleText;
    private TMP_Text bestScoreText;
    private TMP_Text mostUsedHandText;
    private TMP_Text cardsPlayedText;
    private TMP_Text cardsDiscardedText;
    private TMP_Text cardsPurchasedText;
    private TMP_Text totalRerollsText;
    private TMP_Text anteNumberText;
    private TMP_Text lastBlindNameText;
    private Button restartButton;
    private Button mainPageButton;

    public bool IsVisible => panelObject != null && panelObject.activeSelf;

    public void Initialize(Transform canvasRoot)
    {
        panelObject = FindGameObjectIncludingInactive(canvasRoot, "GameResultPanel");

        if (panelObject == null)
        {
            Debug.LogWarning("GameResultPanel was not found. Result summary will be logged only.");
            return;
        }

        Transform panelRoot = panelObject.transform;
        titleText = BindText(panelRoot, "GameResultTitleText", "GameResultTitleText");
        bestScoreText = BindNumberText(panelRoot, "BestScoreContainer");
        mostUsedHandText = BindMostUsedValueText(panelRoot);
        cardsPlayedText = BindNumberText(panelRoot, "CardsPlayedContainer");
        cardsDiscardedText = BindNumberText(panelRoot, "CardsDiscardedContainer");
        cardsPurchasedText = BindNumberText(panelRoot, "CardsPurchasedContainer");
        totalRerollsText = BindNumberText(panelRoot, "TotalRerollsUsedContainer");
        anteNumberText = BindNumberText(panelRoot, "AnteNumberContainer");
        lastBlindNameText = BindNumberText(panelRoot, "LastBlindNameContainer");
        restartButton = BindOptionalButton(panelRoot, "RestartButton");
        mainPageButton = BindOptionalButton(panelRoot, "MainPageButton");

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartCurrentScene);
        }

        if (mainPageButton != null)
        {
            mainPageButton.onClick.RemoveAllListeners();
            mainPageButton.onClick.AddListener(() => Debug.Log("MainPageButton clicked. Main page scene is not configured yet."));
        }

        Hide();
        Debug.Log("Bound GameResultPanel");
    }

    public void Show(GameResultType resultType, GameResultStats stats)
    {
        if (panelObject == null)
        {
            Debug.LogWarning($"Game result: {resultType}. GameResultPanel is not bound.");
            return;
        }

        panelObject.SetActive(true);
        SetText(titleText, resultType.ToString());
        SetText(bestScoreText, stats != null ? stats.bestScore.ToString() : "0");
        SetText(mostUsedHandText, stats != null ? stats.GetMostUsedHandTypeText() : "None");
        SetText(cardsPlayedText, stats != null ? stats.cardsPlayed.ToString() : "0");
        SetText(cardsDiscardedText, stats != null ? stats.cardsDiscarded.ToString() : "0");
        SetText(cardsPurchasedText, stats != null ? stats.cardsPurchased.ToString() : "0");
        SetText(totalRerollsText, stats != null ? stats.totalRerollsUsed.ToString() : "0");
        SetText(anteNumberText, stats != null ? stats.anteNumber.ToString() : "1");
        SetText(lastBlindNameText, stats != null && !string.IsNullOrWhiteSpace(stats.lastBlindName) ? stats.lastBlindName : "Unknown");
        Debug.Log($"GameResultPanel shown: {resultType}");
    }

    public void Hide()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    private TMP_Text BindMostUsedValueText(Transform panelRoot)
    {
        Transform container = FindChildIncludingInactive(panelRoot, "MostUsedHandsContainer");

        if (container == null)
        {
            Debug.LogWarning("MostUsedHandsContainer was not found.");
            return null;
        }

        TMP_Text valueText = FindTextByName(container, "Text (TMP)");

        if (valueText != null)
        {
            return valueText;
        }

        TMP_Text[] texts = container.GetComponentsInChildren<TMP_Text>(true);
        return texts != null && texts.Length > 1 ? texts[1] : null;
    }

    private TMP_Text BindNumberText(Transform panelRoot, string containerName)
    {
        Transform container = FindChildIncludingInactive(panelRoot, containerName);

        if (container == null)
        {
            Debug.LogWarning($"{containerName} was not found.");
            return null;
        }

        return FindTextByName(container, "NumberText");
    }

    private TMP_Text BindText(Transform root, string objectName, string label)
    {
        TMP_Text text = FindTextByName(root, objectName);

        if (text == null)
        {
            Debug.LogWarning($"{label} was not found.");
        }

        return text;
    }

    private TMP_Text FindTextByName(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text != null && text.gameObject.name == objectName)
            {
                return text;
            }
        }

        return null;
    }

    private Button BindOptionalButton(Transform root, string objectName)
    {
        Transform buttonTransform = FindChildIncludingInactive(root, objectName);
        Button button = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;

        if (buttonTransform != null && button == null)
        {
            button = buttonTransform.gameObject.AddComponent<Button>();
            Image image = buttonTransform.GetComponent<Image>();

            if (image != null)
            {
                image.raycastTarget = true;
                button.targetGraphic = image;
            }
        }

        return button;
    }

    private GameObject FindGameObjectIncludingInactive(Transform root, string objectName)
    {
        Transform child = FindChildIncludingInactive(root, objectName);
        return child != null ? child.gameObject : null;
    }

    private Transform FindChildIncludingInactive(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child;
            }
        }

        return null;
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void RestartCurrentScene()
    {
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(activeScene.name);
    }
}
