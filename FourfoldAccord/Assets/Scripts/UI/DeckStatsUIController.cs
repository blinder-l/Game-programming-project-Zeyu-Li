using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckStatsUIController : MonoBehaviour
{
    private enum DeckViewMode
    {
        None,
        CurrentHandStats,
        FullDeckStats
    }

    private readonly Suit[] suitDisplayOrder = { Suit.Hearts, Suit.Spades, Suit.Diamonds, Suit.Clubs };
    private readonly Rank[] rankDisplayOrder =
    {
        Rank.Ace,
        Rank.King,
        Rank.Queen,
        Rank.Jack,
        Rank.Ten,
        Rank.Nine,
        Rank.Eight,
        Rank.Seven,
        Rank.Six,
        Rank.Five,
        Rank.Four,
        Rank.Three,
        Rank.Two
    };

    private GameObject deckStatsPanel;
    private GameObject currentHandStatsPanel;
    private GameObject centerPlayArea;
    private GameObject bottomHandArea;
    private GameObject handSlotsContainer;
    private GameObject actionButtonsContainer;
    private TMP_Text deckStatsTitleText;
    private TMP_Text aFaceNumberSummaryText;
    private TMP_Text suitSummaryText;
    private TMP_Text rankCountText;
    private TMP_Text suitRankMatrixText;
    private TMP_Text currentHandStatsText;
    private readonly Dictionary<Suit, TMP_Text> currentSuitCountTexts = new Dictionary<Suit, TMP_Text>();
    private readonly Dictionary<Rank, TMP_Text> currentRankCountTexts = new Dictionary<Rank, TMP_Text>();
    private readonly Dictionary<Suit, Dictionary<Rank, TMP_Text>> currentSpecificCardCountTexts = new Dictionary<Suit, Dictionary<Rank, TMP_Text>>();
    private Button deckViewButton;
    private Button[] actionButtons;
    private Func<GameUIState> getCurrentState;
    private DeckManager deckManager;
    private HandManager handManager;
    private DeckViewMode currentMode = DeckViewMode.None;

    public bool IsDeckStatsOpen => currentMode != DeckViewMode.None;

    public void Initialize(Transform canvasRoot, Button[] boundActionButtons, Func<GameUIState> stateGetter)
    {
        actionButtons = boundActionButtons;
        getCurrentState = stateGetter;

        if (canvasRoot == null)
        {
            Debug.LogError("DeckStatsUIController failed: Canvas root is null");
            return;
        }

        BindPanels(canvasRoot);
        BindGameplayAreas(canvasRoot);
        BindTextFields(canvasRoot);
        BindDeckViewButton(canvasRoot);
        CloseDeckView(false);
        Debug.Log("DeckStatsUIController initialized");
    }

    public void SetDataSources(DeckManager currentDeckManager, HandManager currentHandManager)
    {
        deckManager = currentDeckManager;
        handManager = currentHandManager;

        if (currentMode == DeckViewMode.CurrentHandStats)
        {
            RefreshCurrentAvailableCardStats();
        }
    }

    private void ToggleDeckStats()
    {
        if (currentMode != DeckViewMode.None)
        {
            CloseDeckView(true);
        }
        else
        {
            OpenDeckView();
        }
    }

    private void OpenDeckView()
    {
        if (deckManager == null)
        {
            Debug.LogError("Cannot show deck stats: DeckManager is null");
            return;
        }

        if (handManager == null)
        {
            Debug.LogError("Cannot show deck stats: HandManager is null");
            return;
        }

        GameUIState currentState = getCurrentState != null ? getCurrentState() : GameUIState.PlayingBlind;

        if (currentState == GameUIState.RunFailed)
        {
            Debug.Log("Cannot open deck view: run has failed.");
            return;
        }

        if (currentState == GameUIState.PlayingBlind)
        {
            OpenCurrentHandStatsView();
        }
        else
        {
            OpenFullDeckStatsView(currentState);
        }
    }

    private void OpenCurrentHandStatsView()
    {
        currentMode = DeckViewMode.CurrentHandStats;
        SetPanelActive(currentHandStatsPanel, true);
        SetPanelActive(deckStatsPanel, false);
        SetPanelActive(centerPlayArea, false);
        SetPanelActive(bottomHandArea, true);
        SetPanelActive(handSlotsContainer, true);
        SetPanelActive(actionButtonsContainer, false);
        SetActionButtonsInteractable(false);
        RefreshCurrentAvailableCardStats();
        Debug.Log("Deck view mode: CurrentAvailableCards");
        Debug.Log("CurrentHandStatsPanel shown");
        Debug.Log("DeckStatsPanel hidden");
        Debug.Log("CenterPlayArea hidden");
        Debug.Log("ActionButtonsContainer hidden");
        Debug.Log("HandSlotsContainer kept visible");
    }

    private void OpenFullDeckStatsView(GameUIState currentState)
    {
        currentMode = DeckViewMode.FullDeckStats;
        List<PlayingCard> deckCards = GetDeckCardsForState(currentState);
        SetPanelActive(deckStatsPanel, true);
        SetPanelActive(currentHandStatsPanel, false);
        BringDeckStatsPanelToFront();
        UpdateDeckStats(deckCards, currentState);
        Debug.Log("Deck view mode: FullDeckStats");
        Debug.Log("DeckStatsPanel shown");
        Debug.Log("CurrentHandStatsPanel hidden");
        Debug.Log("DeckStatsPanel moved to top");
    }

    private List<PlayingCard> GetDeckCardsForState(GameUIState currentState)
    {
        if (currentState != GameUIState.PlayingBlind)
        {
            Debug.Log("Deck stats source: next blind standard deck");
            Debug.LogWarning("Deck stats using standard deck snapshot. Temporary until destroyed-card persistence is implemented.");
            // Temporary until destroyed-card persistence is implemented.
            return deckManager.GetStandardDeckSnapshot();
        }

        Debug.Log("Deck stats source: draw pile");
        return deckManager.GetDrawPileSnapshot();
    }

    public void CloseDeckStatsForStateChange()
    {
        if (currentMode != DeckViewMode.None)
        {
            Debug.Log("Deck view closed due to state change.");
        }

        CloseDeckView(false);
    }

    private void CloseDeckView(bool logClose)
    {
        DeckViewMode closingMode = currentMode;
        currentMode = DeckViewMode.None;

        if (closingMode == DeckViewMode.CurrentHandStats)
        {
            CloseCurrentHandStatsView(logClose);
        }
        else if (closingMode == DeckViewMode.FullDeckStats)
        {
            CloseFullDeckStatsView(logClose);
        }
        else
        {
            SetPanelActive(deckStatsPanel, false);
            SetPanelActive(currentHandStatsPanel, false);
        }

        SetActionButtonsInteractable(getCurrentState != null && getCurrentState() == GameUIState.PlayingBlind);
    }

    private void CloseCurrentHandStatsView(bool logClose)
    {
        SetPanelActive(currentHandStatsPanel, false);

        if (getCurrentState != null && getCurrentState() == GameUIState.PlayingBlind)
        {
            SetPanelActive(centerPlayArea, true);
            SetPanelActive(bottomHandArea, true);
            SetPanelActive(handSlotsContainer, true);
            SetPanelActive(actionButtonsContainer, true);
        }

        if (logClose)
        {
            Debug.Log("Deck view closed");
            Debug.Log("CurrentHandStatsPanel hidden");
            Debug.Log("CenterPlayArea restored");
            Debug.Log("ActionButtonsContainer restored");
        }
    }

    private void CloseFullDeckStatsView(bool logClose)
    {
        SetPanelActive(deckStatsPanel, false);

        if (logClose)
        {
            Debug.Log("Deck view closed");
            Debug.Log("DeckStatsPanel hidden");
        }
    }

    private void BringDeckStatsPanelToFront()
    {
        if (deckStatsPanel != null)
        {
            deckStatsPanel.transform.SetAsLastSibling();
        }
    }

    private void UpdateDeckStats(IReadOnlyList<PlayingCard> cards, GameUIState currentState)
    {
        DeckStats stats = BuildStats(cards);
        SetText(deckStatsTitleText, currentState == GameUIState.Shop ? "Next Blind Deck" : "Remaining Deck");
        SetText(aFaceNumberSummaryText, $"A: {stats.aceCount}\nFace: {stats.faceCount}\nNumber: {stats.numberCount}\nTotal: {stats.totalCount}");
        SetText(suitSummaryText, BuildSuitSummary(stats));
        SetText(rankCountText, BuildRankCountText(stats));
        SetText(suitRankMatrixText, BuildSuitRankMatrixText(stats));
        Debug.Log($"Deck stats updated: {stats.totalCount} cards");
    }

    private void RefreshCurrentAvailableCardStats()
    {
        if (deckManager == null)
        {
            Debug.LogError("Cannot update available card stats: draw pile snapshot unavailable.");
            return;
        }

        List<PlayingCard> cards = BuildCurrentAvailableCardsInBlind();
        DeckStats stats = BuildStats(cards);
        UpdateCurrentSuitCountTexts(stats);
        UpdateCurrentRankCountTexts(stats);
        UpdateCurrentSpecificCardCountTexts(stats);

        Debug.Log("Current available card stats updated.");
        Debug.Log($"Current hand count: {(handManager != null && handManager.CurrentHand != null ? handManager.CurrentHand.Count : 0)}");
        Debug.Log($"Draw pile count: {deckManager.DrawPileCount}");
        Debug.Log($"Available cards total: {stats.totalCount}");
        Debug.Log($"Suit counts: Hearts {stats.suitCounts[Suit.Hearts]}, Spades {stats.suitCounts[Suit.Spades]}, Diamonds {stats.suitCounts[Suit.Diamonds]}, Clubs {stats.suitCounts[Suit.Clubs]}");
        Debug.Log("Rank counts updated.");
        Debug.Log("Specific card counts updated.");
    }

    private void UpdateCurrentSuitCountTexts(DeckStats stats)
    {
        foreach (Suit suit in suitDisplayOrder)
        {
            TMP_Text countText = currentSuitCountTexts.ContainsKey(suit) ? currentSuitCountTexts[suit] : null;
            SetText(countText, stats.suitCounts[suit].ToString());
        }
    }

    private void UpdateCurrentRankCountTexts(DeckStats stats)
    {
        foreach (Rank rank in rankDisplayOrder)
        {
            TMP_Text countText = currentRankCountTexts.ContainsKey(rank) ? currentRankCountTexts[rank] : null;
            SetText(countText, stats.rankCounts[rank].ToString());
        }
    }

    private void UpdateCurrentSpecificCardCountTexts(DeckStats stats)
    {
        foreach (Suit suit in suitDisplayOrder)
        {
            if (!currentSpecificCardCountTexts.ContainsKey(suit))
            {
                continue;
            }

            foreach (Rank rank in rankDisplayOrder)
            {
                TMP_Text countText = currentSpecificCardCountTexts[suit].ContainsKey(rank)
                    ? currentSpecificCardCountTexts[suit][rank]
                    : null;
                SetText(countText, stats.suitRankCounts[suit][rank].ToString());
            }
        }
    }

    private List<PlayingCard> BuildCurrentAvailableCardsInBlind()
    {
        List<PlayingCard> availableCards = new List<PlayingCard>();
        int currentHandCount = 0;
        int drawPileCount = 0;

        if (handManager != null && handManager.CurrentHand != null)
        {
            currentHandCount = handManager.CurrentHand.Count;
            availableCards.AddRange(handManager.CurrentHand);
        }

        if (deckManager != null)
        {
            List<PlayingCard> drawPileSnapshot = deckManager.GetDrawPileSnapshot();
            drawPileCount = drawPileSnapshot.Count;
            availableCards.AddRange(drawPileSnapshot);
        }

        Debug.Log("Available cards source: current hand + draw pile");
        Debug.Log($"Current hand count: {currentHandCount}");
        Debug.Log($"Draw pile count: {drawPileCount}");
        Debug.Log($"Available cards total: {availableCards.Count}");
        return availableCards;
    }

    private DeckStats BuildStats(IEnumerable<PlayingCard> cards)
    {
        DeckStats stats = new DeckStats();

        foreach (Suit suit in suitDisplayOrder)
        {
            stats.suitCounts[suit] = 0;
            stats.suitRankCounts[suit] = new Dictionary<Rank, int>();

            foreach (Rank rank in rankDisplayOrder)
            {
                stats.suitRankCounts[suit][rank] = 0;
            }
        }

        foreach (Rank rank in rankDisplayOrder)
        {
            stats.rankCounts[rank] = 0;
        }

        if (cards == null)
        {
            return stats;
        }

        foreach (PlayingCard card in cards)
        {
            if (card == null)
            {
                continue;
            }

            stats.totalCount++;
            stats.suitCounts[card.suit]++;
            stats.rankCounts[card.rank]++;
            stats.suitRankCounts[card.suit][card.rank]++;

            if (card.rank == Rank.Ace)
            {
                stats.aceCount++;
            }
            else if (card.rank == Rank.Jack || card.rank == Rank.Queen || card.rank == Rank.King)
            {
                stats.faceCount++;
            }
            else
            {
                stats.numberCount++;
            }
        }

        return stats;
    }

    private string BuildSuitSummary(DeckStats stats)
    {
        StringBuilder builder = new StringBuilder();

        foreach (Suit suit in suitDisplayOrder)
        {
            builder.AppendLine($"{suit}: {stats.suitCounts[suit]}");
        }

        return builder.ToString();
    }

    private string BuildRankCountText(DeckStats stats)
    {
        StringBuilder builder = new StringBuilder();

        foreach (Rank rank in rankDisplayOrder)
        {
            builder.AppendLine($"{GetRankLabel(rank)}: {stats.rankCounts[rank]}");
        }

        return builder.ToString();
    }

    private string BuildSuitRankMatrixText(DeckStats stats)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("        ");

        foreach (Rank rank in rankDisplayOrder)
        {
            builder.Append($"{GetRankLabel(rank),3}");
        }

        builder.AppendLine();

        foreach (Suit suit in suitDisplayOrder)
        {
            builder.Append($"{suit,-8}");

            foreach (Rank rank in rankDisplayOrder)
            {
                builder.Append($"{stats.suitRankCounts[suit][rank],3}");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private string GetRankLabel(Rank rank)
    {
        switch (rank)
        {
            case Rank.Ace:
                return "A";
            case Rank.King:
                return "K";
            case Rank.Queen:
                return "Q";
            case Rank.Jack:
                return "J";
            case Rank.Ten:
                return "10";
            case Rank.Nine:
                return "9";
            case Rank.Eight:
                return "8";
            case Rank.Seven:
                return "7";
            case Rank.Six:
                return "6";
            case Rank.Five:
                return "5";
            case Rank.Four:
                return "4";
            case Rank.Three:
                return "3";
            default:
                return "2";
        }
    }

    private void BindPanels(Transform canvasRoot)
    {
        deckStatsPanel = FindObjectIncludingInactive(canvasRoot, "DeckStatsPanel");
        currentHandStatsPanel = FindObjectIncludingInactive(canvasRoot, "CurrentHandStatsPanel");

        if (deckStatsPanel == null)
        {
            Debug.LogError("Failed to bind DeckStatsPanel");
        }
        else
        {
            DisableDirectImageRaycast(deckStatsPanel);
            Debug.Log("Bound DeckStatsPanel");
        }

        if (currentHandStatsPanel == null)
        {
            Debug.LogError("Failed to bind CurrentHandStatsPanel");
        }
        else
        {
            DisableDirectImageRaycast(currentHandStatsPanel);
            Debug.Log("Bound CurrentHandStatsPanel");
        }
    }

    private void BindGameplayAreas(Transform canvasRoot)
    {
        centerPlayArea = FindObjectIncludingInactive(canvasRoot, "CenterPlayArea");
        bottomHandArea = FindObjectIncludingInactive(canvasRoot, "BottomHandArea");
        handSlotsContainer = FindObjectIncludingInactive(canvasRoot, "HandSlotsContainer");
        actionButtonsContainer = FindObjectIncludingInactive(canvasRoot, "ActionButtonsContainer");

        if (centerPlayArea == null)
        {
            Debug.LogError("Failed to bind CenterPlayArea");
        }
        else
        {
            Debug.Log("Bound CenterPlayArea");
        }

        if (bottomHandArea == null)
        {
            Debug.LogError("Failed to bind BottomHandArea");
        }
        else
        {
            Debug.Log("Bound BottomHandArea");
        }

        if (handSlotsContainer == null)
        {
            Debug.LogError("Failed to bind HandSlotsContainer");
        }
        else
        {
            Debug.Log("Bound HandSlotsContainer");
        }

        if (actionButtonsContainer == null)
        {
            Debug.LogError("Failed to bind ActionButtonsContainer");
        }
        else
        {
            Debug.Log("Bound ActionButtonsContainer");
        }
    }

    private void BindTextFields(Transform canvasRoot)
    {
        deckStatsTitleText = BindText(canvasRoot, "DeckStatsTitleText");
        aFaceNumberSummaryText = BindText(canvasRoot, "AFaceNumberSummaryText");
        suitSummaryText = BindText(canvasRoot, "SuitSummaryText");
        rankCountText = BindText(canvasRoot, "RankCountText");
        suitRankMatrixText = BindText(canvasRoot, "SuitRankMatrixText");
        currentHandStatsText = BindText(canvasRoot, "CurrentHandStatsText", false);
        BindCurrentHandStatsPanelTexts();
        Debug.Log("Bound DeckStats text fields");
    }

    private void BindCurrentHandStatsPanelTexts()
    {
        currentSuitCountTexts.Clear();
        currentRankCountTexts.Clear();
        currentSpecificCardCountTexts.Clear();

        if (currentHandStatsPanel == null)
        {
            Debug.LogError("Failed to bind CurrentHandStatsPanel count texts: panel is null");
            return;
        }

        Transform panelRoot = currentHandStatsPanel.transform;
        BindCurrentSuitCountTexts(panelRoot);
        BindCurrentRankCountTexts(panelRoot);
        BindSpecificCardCountTexts(panelRoot);
    }

    private void BindCurrentSuitCountTexts(Transform panelRoot)
    {
        currentSuitCountTexts[Suit.Hearts] = BindText(panelRoot, "HeartsNumberText");
        currentSuitCountTexts[Suit.Spades] = BindText(panelRoot, "SpadesNumberText");
        currentSuitCountTexts[Suit.Diamonds] = BindText(panelRoot, "DiamondsNumberText");
        currentSuitCountTexts[Suit.Clubs] = BindText(panelRoot, "ClubsNumberText");
        Debug.Log("Bound LeftSuitInfoModule count texts");
    }

    private void BindCurrentRankCountTexts(Transform panelRoot)
    {
        Transform rankModule = FindDeepChildIncludingInactive(panelRoot, "TopRankInfoModule");

        if (rankModule == null)
        {
            rankModule = FindDeepChildIncludingInactive(panelRoot, "TopRankInfoMudule");

            if (rankModule != null)
            {
                Debug.Log("Found TopRankInfoMudule fallback.");
            }
        }

        Transform searchRoot = rankModule != null ? rankModule : panelRoot;

        foreach (Rank rank in rankDisplayOrder)
        {
            string textName = $"{GetRankLabel(rank)}NumberText";
            currentRankCountTexts[rank] = BindText(searchRoot, textName);
        }

        Debug.Log("Bound TopRankInfoModule count texts");
    }

    private void BindSpecificCardCountTexts(Transform panelRoot)
    {
        Transform specificModule = FindDeepChildIncludingInactive(panelRoot, "SpecificCardCountModule");
        Transform searchRoot = specificModule != null ? specificModule : panelRoot;

        foreach (Suit suit in suitDisplayOrder)
        {
            currentSpecificCardCountTexts[suit] = new Dictionary<Rank, TMP_Text>();

            foreach (Rank rank in rankDisplayOrder)
            {
                TMP_Text countText = BindSpecificCardCountText(searchRoot, suit, rank);
                currentSpecificCardCountTexts[suit][rank] = countText;
            }
        }

        Debug.Log("Bound SpecificCardCountModule count texts");
    }

    private TMP_Text BindSpecificCardCountText(Transform searchRoot, Suit suit, Rank rank)
    {
        string rankLabel = GetRankLabel(rank);
        string[] prefixes = GetSpecificSuitPrefixes(suit);

        for (int i = 0; i < prefixes.Length; i++)
        {
            string textName = $"{prefixes[i]}{rankLabel}NumberText";
            TMP_Text countText = BindText(searchRoot, textName, false);

            if (countText != null)
            {
                return countText;
            }
        }

        Debug.LogWarning($"Warning: specific card count text not found: {prefixes[0]}{rankLabel}NumberText");
        return null;
    }

    private string[] GetSpecificSuitPrefixes(Suit suit)
    {
        switch (suit)
        {
            case Suit.Hearts:
                return new[] { "Heart", "Hearts" };
            case Suit.Spades:
                return new[] { "Spade", "Spades" };
            case Suit.Diamonds:
                return new[] { "Diamond", "Diamonds" };
            default:
                return new[] { "Club", "Clubs" };
        }
    }

    private TMP_Text BindText(Transform canvasRoot, string objectName)
    {
        return BindText(canvasRoot, objectName, true);
    }

    private TMP_Text BindText(Transform canvasRoot, string objectName, bool logErrorIfMissing)
    {
        GameObject textObject = FindObjectIncludingInactive(canvasRoot, objectName);

        if (textObject == null)
        {
            if (logErrorIfMissing)
            {
                Debug.LogError($"Failed to bind {objectName}");
            }

            return null;
        }

        TMP_Text text = textObject.GetComponent<TMP_Text>();

        if (text == null)
        {
            text = textObject.GetComponentInChildren<TMP_Text>(true);
        }

        if (text == null)
        {
            if (logErrorIfMissing)
            {
                Debug.LogError($"Failed to bind {objectName}");
            }

            return null;
        }

        text.raycastTarget = false;
        return text;
    }

    private void BindDeckViewButton(Transform canvasRoot)
    {
        GameObject buttonObject = FindObjectIncludingInactive(canvasRoot, "DeckViewButton");

        if (buttonObject == null)
        {
            Debug.LogError("Failed to bind DeckViewButton");
            return;
        }

        Debug.Log("Found DeckViewButton");
        deckViewButton = EnsureButton(buttonObject, "DeckViewButton");

        if (deckViewButton == null)
        {
            return;
        }

        deckViewButton.onClick.RemoveAllListeners();
        deckViewButton.onClick.AddListener(HandleDeckViewButtonClicked);
        deckViewButton.interactable = true;
        DisableTextRaycasts(buttonObject);
        Debug.Log("Bound DeckViewButton");
        Debug.Log("Bound DeckViewButton successfully");
    }

    private Button EnsureButton(GameObject buttonObject, string label)
    {
        Image image = buttonObject.GetComponent<Image>();

        if (image == null)
        {
            image = buttonObject.AddComponent<Image>();
        }

        image.raycastTarget = true;
        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        button.targetGraphic = image;
        button.interactable = true;
        return button;
    }

    private void HandleDeckViewButtonClicked()
    {
        Debug.Log("UI DeckViewButton clicked");
        ToggleDeckStats();
    }

    private void SetActionButtonsInteractable(bool isInteractable)
    {
        if (actionButtons == null)
        {
            return;
        }

        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null)
            {
                actionButtons[i].interactable = isInteractable;
            }
        }
    }

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
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

    private void DisableDirectImageRaycast(GameObject targetObject)
    {
        Image image = targetObject != null ? targetObject.GetComponent<Image>() : null;

        if (image != null)
        {
            image.raycastTarget = false;
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

    private class DeckStats
    {
        public int totalCount;
        public int aceCount;
        public int faceCount;
        public int numberCount;
        public readonly Dictionary<Suit, int> suitCounts = new Dictionary<Suit, int>();
        public readonly Dictionary<Rank, int> rankCounts = new Dictionary<Rank, int>();
        public readonly Dictionary<Suit, Dictionary<Rank, int>> suitRankCounts = new Dictionary<Suit, Dictionary<Rank, int>>();
    }
}
