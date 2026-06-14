using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    private const string PlayButtonPath = "Canvas/PlayStateRoot/BottomHandArea/ActionButtonsContainer/PlayButton";

    [SerializeField] private GameObject playStateRoot;
    [SerializeField] private GameObject cashOutPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject deckStatsPanel;
    [SerializeField] private GameObject currentHandStatsPanel;
    [SerializeField] private GameObject cardTooltipPanel;
    [SerializeField] private GameObject actionButtonsContainer;
    [SerializeField] private Transform jokerSlotsContainer;
    [SerializeField] private DeckStatsUIController deckStatsUIController;
    [SerializeField] private CashOutUIController cashOutUIController;
    [SerializeField] private ShopUIController shopUIController;
    [SerializeField] private CardSpriteDatabase cardSpriteDatabase;
    [SerializeField] private Transform handSlotsContainer;
    [SerializeField] private JokerSlotView[] jokerSlotViews;
    [SerializeField] private HandCardView[] handCardViews;
    [SerializeField] private Button playButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button sortBySuitButton;
    [SerializeField] private Button sortByRankButton;
    [SerializeField] private TMP_Text blindNameText;
    [SerializeField] private TMP_Text targetScoreText;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text handTypeText;
    [SerializeField] private TMP_Text handsText;
    [SerializeField] private TMP_Text discardsText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text anteNumberText;
    [SerializeField] private GameObject resolutionInfoArea;
    [SerializeField] private GameObject playedCardsArea;
    [SerializeField] private Image[] playedCardImages;
    [SerializeField] private HandCardView[] playedCardViews;
    [SerializeField] private TMP_Text resolutionInfoText;

    public event Action<PlayingCard> HandCardClicked;
    public event Action PlayButtonClicked;
    public event Action DiscardButtonClicked;
    public event Action SortBySuitButtonClicked;
    public event Action SortByRankButtonClicked;
    public event Action CashOutButtonClicked;
    public event Action<int> ShopJokerOfferClicked;
    public event Action ShopRerollButtonClicked;
    public event Action ShopNextBlindButtonClicked;

    public GameUIState CurrentState { get; private set; }
    public bool IsDeckStatsOpen => deckStatsUIController != null && deckStatsUIController.IsDeckStatsOpen;
    public bool CanAcceptGameplayInput => CurrentState == GameUIState.PlayingBlind && !IsDeckStatsOpen;

    private bool hasInitialized;

    private void Awake()
    {
        InitializeRuntimeBindings();
        Debug.Log("GameUIController initialized");
    }

    public void InitializeRuntimeBindings()
    {
        if (hasInitialized)
        {
            return;
        }

        ResolveStateRoots();
        ResolveCardSpriteDatabaseIfNeeded();
        BindLeftStatusUI();
        BindPlayedCardsUI();
        BindResolutionInfoUI();
        BindJokerBarUI();
        BindHandCards();
        DisableKnownBackgroundRaycasts();
        BindActionButtons();
        BindDeckStatsController();
        BindCashOutController();
        BindShopController();
        EnsurePointerInputSupport();
        LogPlayButtonDiagnostics();
        SetState(GameUIState.PlayingBlind);
        hasInitialized = true;
    }

    public void SetState(GameUIState newState)
    {
        bool enteringRunFailed = CurrentState != GameUIState.RunFailed && newState == GameUIState.RunFailed;
        CurrentState = newState;
        deckStatsUIController?.CloseDeckStatsForStateChange();

        SetActiveIfAssigned(playStateRoot, newState == GameUIState.PlayingBlind || newState == GameUIState.RunFailed);
        SetActiveIfAssigned(cashOutPanel, newState == GameUIState.CashOut);
        SetActiveIfAssigned(shopPanel, newState == GameUIState.Shop);
        SetActiveIfAssigned(deckStatsPanel, false);
        SetActiveIfAssigned(currentHandStatsPanel, false);
        SetActiveIfAssigned(cardTooltipPanel, false);
        SetActiveIfAssigned(actionButtonsContainer, newState == GameUIState.PlayingBlind);
        SetActionButtonsInteractable(CanAcceptGameplayInput);

        if (newState != GameUIState.CashOut)
        {
            cashOutUIController?.Hide();
        }

        if (newState != GameUIState.Shop)
        {
            shopUIController?.Hide();
        }

        if (enteringRunFailed)
        {
            SetText(blindNameText, "Run Failed");
            Debug.Log("Run failed.");
        }
    }

    public void RefreshHand(IReadOnlyList<PlayingCard> currentHand)
    {
        ResolveCardSpriteDatabaseIfNeeded();
        EnsureHandCardsBound();

        if (handCardViews == null)
        {
            return;
        }

        for (int i = 0; i < handCardViews.Length; i++)
        {
            HandCardView cardView = handCardViews[i];

            if (cardView == null)
            {
                continue;
            }

            if (currentHand != null && i < currentHand.Count)
            {
                cardView.SetCard(i, currentHand[i], cardSpriteDatabase, HandleHandCardClicked);
            }
            else
            {
                cardView.Clear();
            }
        }
    }

    public void RefreshJokerBar(IReadOnlyList<JokerBase> equippedJokers)
    {
        EnsureJokerSlotsBound();

        if (jokerSlotViews == null)
        {
            Debug.LogError("Cannot refresh Joker bar: Joker slots are not bound");
            return;
        }

        int equippedCount = equippedJokers != null ? equippedJokers.Count : 0;

        for (int i = 0; i < jokerSlotViews.Length; i++)
        {
            JokerSlotView slotView = jokerSlotViews[i];

            if (slotView == null)
            {
                continue;
            }

            JokerBase joker = equippedJokers != null && i < equippedJokers.Count ? equippedJokers[i] : null;
            slotView.SetJoker(joker);
        }

        Debug.Log($"Joker bar updated: {equippedCount} equipped jokers");
    }

    public void SetDeckStatsSources(DeckManager deckManager, HandManager handManager)
    {
        if (deckStatsUIController == null)
        {
            BindDeckStatsController();
        }

        if (deckStatsUIController != null)
        {
            deckStatsUIController.SetDataSources(deckManager, handManager);
        }
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
        if (cashOutUIController == null)
        {
            BindCashOutController();
        }

        cashOutUIController?.ShowCashOut(
            targetScore,
            currentScore,
            fixedBlindReward,
            suitGoldThisBlind,
            interest,
            discardBonus,
            cashOutTotal);
    }

    public void SetCashOutButtonInteractable(bool isInteractable)
    {
        cashOutUIController?.SetButtonInteractable(isInteractable);
    }

    public void ShowShop(IReadOnlyList<ShopOffer> offers)
    {
        if (shopUIController == null)
        {
            BindShopController();
        }

        shopUIController?.ShowShop(offers);
    }

    public void RefreshShopOffers(IReadOnlyList<ShopOffer> offers)
    {
        shopUIController?.RefreshOffers(offers);
    }

    public void RefreshBlindStatus(
        string blindName,
        int targetScore,
        int currentScore,
        string latestHandType,
        int handsRemaining,
        int discardsRemaining,
        int currentGold,
        int anteNumber)
    {
        SetText(blindNameText, blindName);
        SetText(targetScoreText, $"Target: {targetScore}");
        SetText(currentScoreText, $"Score: {currentScore}");
        SetText(handTypeText, $"Hand Type: {latestHandType}");
        SetText(handsText, $"Hands: {handsRemaining}");
        SetText(discardsText, $"Discards: {discardsRemaining}");
        SetText(goldText, $"Gold: ${currentGold}");
        SetText(anteNumberText, $"Ante: {anteNumber}");
        Debug.Log($"Left status UI updated: score = {currentScore}, hands = {handsRemaining}");
    }

    public void RefreshHandTypeText(string handTypeTextValue)
    {
        SetText(handTypeText, $"Hand Type: {handTypeTextValue}");
    }

    public void RefreshPlayedCards(IReadOnlyList<PlayingCard> playedCards)
    {
        ResolveCardSpriteDatabaseIfNeeded();
        EnsurePlayedCardsBound();

        if (playedCardImages == null)
        {
            return;
        }

        int displayedCount = 0;

        for (int i = 0; i < playedCardImages.Length; i++)
        {
            Image cardImage = playedCardImages[i];

            if (cardImage == null)
            {
                continue;
            }

            if (playedCards != null && i < playedCards.Count)
            {
                cardImage.gameObject.SetActive(true);
                cardImage.enabled = true;
                cardImage.sprite = cardSpriteDatabase != null ? cardSpriteDatabase.GetSprite(playedCards[i]) : null;
                cardImage.raycastTarget = false;
                displayedCount++;
            }
            else
            {
                cardImage.sprite = null;
                cardImage.gameObject.SetActive(false);
            }
        }

        Debug.Log($"Played cards UI updated: {displayedCount} cards");
    }

    public void RefreshResolutionInfo(ScoreContext scoreContext)
    {
        EnsureResolutionInfoBound();

        if (scoreContext == null)
        {
            SetText(resolutionInfoText, "No hand played yet.");
            Debug.Log("Resolution info updated");
            return;
        }

        string resolutionText =
            $"Hand Type: {scoreContext.handType}\n" +
            $"Base Chips: {scoreContext.baseChips}\n" +
            $"Card Chips: {scoreContext.rankChips}\n" +
            $"Total Chips: {scoreContext.chips}\n" +
            $"Mult: {scoreContext.mult}\n" +
            $"Final Score: {scoreContext.finalScore}\n\n" +
            $"Suit Effects:\n{scoreContext.GetSuitEffectDebugText()}\n\n" +
            $"Joker Effects:\n{scoreContext.GetJokerEffectDebugText()}\n\n" +
            $"Gold Reward: {scoreContext.goldReward}";

        SetText(resolutionInfoText, resolutionText);
        Debug.Log("Resolution info updated");
    }

    private void HandleHandCardClicked(PlayingCard card)
    {
        if (!CanAcceptGameplayInput)
        {
            Debug.Log("Cannot select card: gameplay input is blocked while deck view is open.");
            return;
        }

        Debug.Log($"UI HandCard clicked: {card}");
        HandCardClicked?.Invoke(card);
    }

    public void HandlePlayButtonClicked()
    {
        Debug.Log("UI PlayButton clicked");

        if (!CanAcceptGameplayInput)
        {
            if (IsDeckStatsOpen)
            {
                Debug.Log("Cannot play: gameplay input is blocked while deck view is open.");
            }
            else
            {
                Debug.Log($"Cannot play: current state is {CurrentState}");
            }

            return;
        }

        if (CurrentState == GameUIState.PlayingBlind)
        {
            PlayButtonClicked?.Invoke();
            return;
        }

        Debug.Log($"Play failed: current UI state is {CurrentState}.");
    }

    private void HandleDiscardButtonClicked()
    {
        Debug.Log("UI DiscardButton clicked");

        if (!CanAcceptGameplayInput)
        {
            if (IsDeckStatsOpen)
            {
                Debug.Log("Cannot discard: gameplay input is blocked while deck view is open.");
            }
            else
            {
                Debug.Log($"Discard failed: current UI state is {CurrentState}.");
            }

            return;
        }

        if (CurrentState == GameUIState.PlayingBlind)
        {
            DiscardButtonClicked?.Invoke();
            return;
        }

        Debug.Log($"Discard failed: current UI state is {CurrentState}.");
    }

    private void HandleSortBySuitButtonClicked()
    {
        Debug.Log("UI SortBySuitButton clicked");

        if (!CanAcceptGameplayInput)
        {
            if (IsDeckStatsOpen)
            {
                Debug.Log("Cannot sort: gameplay input is blocked while deck view is open.");
            }
            else
            {
                Debug.Log($"Sort by suit ignored: current UI state is {CurrentState}.");
            }

            return;
        }

        if (CurrentState == GameUIState.PlayingBlind)
        {
            SortBySuitButtonClicked?.Invoke();
            return;
        }

        Debug.Log($"Sort by suit ignored: current UI state is {CurrentState}.");
    }

    private void HandleSortByRankButtonClicked()
    {
        Debug.Log("UI SortByRankButton clicked");

        if (!CanAcceptGameplayInput)
        {
            if (IsDeckStatsOpen)
            {
                Debug.Log("Cannot sort: gameplay input is blocked while deck view is open.");
            }
            else
            {
                Debug.Log($"Sort by rank ignored: current UI state is {CurrentState}.");
            }

            return;
        }

        if (CurrentState == GameUIState.PlayingBlind)
        {
            SortByRankButtonClicked?.Invoke();
            return;
        }

        Debug.Log($"Sort by rank ignored: current UI state is {CurrentState}.");
    }

    private void SetActionButtonsInteractable(bool isInteractable)
    {
        SetButtonInteractable(playButton, isInteractable);
        SetButtonInteractable(discardButton, isInteractable);
        SetButtonInteractable(sortBySuitButton, isInteractable);
        SetButtonInteractable(sortByRankButton, isInteractable);
    }

    private void ResolveCardSpriteDatabaseIfNeeded()
    {
        if (cardSpriteDatabase != null)
        {
            return;
        }

        cardSpriteDatabase = FindFirstObjectByType<CardSpriteDatabase>();

        if (cardSpriteDatabase == null)
        {
            cardSpriteDatabase = gameObject.AddComponent<CardSpriteDatabase>();
        }
    }

    private void ResolveStateRoots()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        Transform canvasRoot = canvas != null ? canvas.transform : null;

        playStateRoot = FindGameObjectByPath("Canvas/PlayStateRoot", playStateRoot);
        cashOutPanel = FindGameObjectIncludingInactive(canvasRoot, "CashOutPanel", cashOutPanel);
        shopPanel = FindGameObjectIncludingInactive(canvasRoot, "ShopPanel", shopPanel);
        deckStatsPanel = FindGameObjectIncludingInactive(canvasRoot, "DeckStatsPanel", deckStatsPanel);
        currentHandStatsPanel = FindGameObjectIncludingInactive(canvasRoot, "CurrentHandStatsPanel", currentHandStatsPanel);
        cardTooltipPanel = FindGameObjectIncludingInactive(canvasRoot, "CardTooltipPanel", cardTooltipPanel);
        actionButtonsContainer = FindGameObjectByPath("Canvas/PlayStateRoot/BottomHandArea/ActionButtonsContainer", actionButtonsContainer);
    }

    private void BindLeftStatusUI()
    {
        blindNameText = BindText("LeftBlindPanel/BlindNameText", "BlindNameText");
        targetScoreText = BindText("LeftBlindPanel/TargetScoreText", "TargetScoreText");
        currentScoreText = BindText("LeftBlindPanel/CurrentScoreText", "CurrentScoreText");
        handTypeText = BindText("LeftBlindPanel/HandTypeText", "HandTypeText");
        handsText = BindText("LeftBlindPanel/HandsText", "HandsText");
        discardsText = BindText("LeftBlindPanel/DiscardsText", "DiscardsText");
        goldText = BindText("LeftBlindPanel/GoldText", "GoldText");
        anteNumberText = BindText("LeftBlindPanel/AnteNumberText", "AnteNumberText");
        Debug.Log("Bound left status UI");
    }

    private void BindJokerBarUI()
    {
        GameObject containerObject = GameObject.Find("Canvas/TopJokerBar/JokerSlotsContainer");

        if (containerObject == null)
        {
            Debug.LogError("Failed to find TopJokerBar/JokerSlotsContainer");
            return;
        }

        jokerSlotsContainer = containerObject.transform;
        jokerSlotViews = new JokerSlotView[5];

        for (int i = 0; i < jokerSlotViews.Length; i++)
        {
            string slotName = $"JokerSlot{i + 1}";
            Transform slotTransform = jokerSlotsContainer.Find(slotName);

            if (slotTransform == null)
            {
                Debug.LogError($"Failed to bind {slotName}");
                continue;
            }

            JokerSlotView slotView = slotTransform.GetComponent<JokerSlotView>();

            if (slotView == null)
            {
                slotView = slotTransform.gameObject.AddComponent<JokerSlotView>();
            }

            slotView.SetJoker(null);
            jokerSlotViews[i] = slotView;
            Debug.Log($"Bound {slotName}");
        }
    }

    private void EnsureJokerSlotsBound()
    {
        if (jokerSlotViews != null && jokerSlotViews.Length == 5)
        {
            bool hasAllSlots = true;

            for (int i = 0; i < jokerSlotViews.Length; i++)
            {
                if (jokerSlotViews[i] == null)
                {
                    hasAllSlots = false;
                    break;
                }
            }

            if (hasAllSlots)
            {
                return;
            }
        }

        BindJokerBarUI();
    }

    private TMP_Text BindText(string relativePath, string label)
    {
        GameObject textObject = GameObject.Find($"Canvas/{relativePath}");

        if (textObject == null)
        {
            Debug.LogError($"Failed to find {relativePath}");
            Debug.LogError($"Failed to bind {label}");
            return null;
        }

        TMP_Text boundText = textObject.GetComponent<TMP_Text>();

        if (boundText == null)
        {
            boundText = textObject.GetComponentInChildren<TMP_Text>(true);
        }

        if (boundText == null)
        {
            Debug.LogError($"Failed to bind {label}");
            return null;
        }

        boundText.raycastTarget = false;
        Debug.Log($"Bound {label}");
        return boundText;
    }

    private void BindPlayedCardsUI()
    {
        playedCardsArea = GameObject.Find("Canvas/PlayStateRoot/CenterPlayArea/PlayedCardsArea");

        if (playedCardsArea == null)
        {
            Debug.LogError("Failed to find PlayStateRoot/CenterPlayArea/PlayedCardsArea");
            return;
        }

        Debug.Log("Bound PlayedCardsArea");
        playedCardImages = new Image[5];

        for (int i = 0; i < playedCardImages.Length; i++)
        {
            string cardName = $"PlayedCard{i + 1}";
            Transform cardTransform = playedCardsArea.transform.Find(cardName);

            if (cardTransform == null)
            {
                Debug.LogError($"Failed to bind {cardName}");
                continue;
            }

            Image cardImage = cardTransform.GetComponent<Image>();

            if (cardImage == null)
            {
                Debug.LogError($"Failed to bind {cardName}");
                continue;
            }

            cardImage.raycastTarget = false;
            playedCardImages[i] = cardImage;
            Debug.Log($"Bound {cardName}");
        }

        Debug.Log("Bound played cards UI");
    }

    private void EnsurePlayedCardsBound()
    {
        if (playedCardImages != null && playedCardImages.Length == 5)
        {
            bool hasAllCards = true;

            for (int i = 0; i < playedCardImages.Length; i++)
            {
                if (playedCardImages[i] == null)
                {
                    hasAllCards = false;
                    break;
                }
            }

            if (hasAllCards)
            {
                return;
            }
        }

        BindPlayedCardsUI();
    }

    private void BindResolutionInfoUI()
    {
        resolutionInfoArea = GameObject.Find("Canvas/PlayStateRoot/CenterPlayArea/ResolutionInfoArea");

        if (resolutionInfoArea == null)
        {
            Debug.LogError("Failed to find PlayStateRoot/CenterPlayArea/ResolutionInfoArea");
            Debug.LogError("Failed to bind ResolutionInfoText");
            return;
        }

        Debug.Log("Bound ResolutionInfoArea");
        resolutionInfoText = resolutionInfoArea.GetComponent<TMP_Text>();

        if (resolutionInfoText == null)
        {
            resolutionInfoText = resolutionInfoArea.GetComponentInChildren<TMP_Text>(true);
        }

        if (resolutionInfoText == null)
        {
            resolutionInfoText = CreateResolutionInfoText(resolutionInfoArea.transform);
        }

        if (resolutionInfoText == null)
        {
            Debug.LogError("Failed to bind ResolutionInfoText");
            return;
        }

        resolutionInfoText.raycastTarget = false;
        Debug.Log("Bound ResolutionInfoText");
        Debug.Log("Bound resolution info UI");
    }

    private void EnsureResolutionInfoBound()
    {
        if (resolutionInfoText != null)
        {
            return;
        }

        BindResolutionInfoUI();
    }

    private TMP_Text CreateResolutionInfoText(Transform parent)
    {
        GameObject textObject = new GameObject("ResolutionInfoText", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.fontSize = 22f;
        text.color = Color.white;
        text.text = "No hand played yet.";
        Debug.Log("Created ResolutionInfoText");
        return text;
    }

    private void BindHandCards()
    {
        List<HandCardView> resolvedViews = new List<HandCardView>();

        if (handSlotsContainer == null)
        {
            GameObject foundContainer = GameObject.Find("Canvas/PlayStateRoot/BottomHandArea/HandSlotsContainer");

            if (foundContainer != null)
            {
                handSlotsContainer = foundContainer.transform;
            }
        }

        if (handSlotsContainer == null)
        {
            Debug.LogWarning("HandSlotsContainer was not assigned or found. Hand UI cannot refresh yet.");
            return;
        }

        for (int i = 1; i <= 8; i++)
        {
            string cardName = $"HandCard{i}";
            Transform cardTransform = handSlotsContainer.Find(cardName);

            if (cardTransform == null)
            {
                Debug.LogError($"{cardName} was not found under HandSlotsContainer.");
                continue;
            }

            Image cardImage = cardTransform.GetComponent<Image>();

            if (cardImage == null)
            {
                Debug.LogError($"{cardName} is missing an Image component.");
                continue;
            }

            cardImage.raycastTarget = true;

            Button cardButton = cardTransform.GetComponent<Button>();

            if (cardButton == null)
            {
                cardButton = cardTransform.gameObject.AddComponent<Button>();
            }

            cardButton.targetGraphic = cardImage;
            cardButton.interactable = true;

            HandCardView cardView = cardTransform.GetComponent<HandCardView>();

            if (cardView == null)
            {
                cardView = cardTransform.gameObject.AddComponent<HandCardView>();
            }

            resolvedViews.Add(cardView);
            Debug.Log($"Bound {cardName}");
        }

        handCardViews = resolvedViews.ToArray();
    }

    private void EnsureHandCardsBound()
    {
        if (HasHandCardViews())
        {
            return;
        }

        BindHandCards();
    }

    private bool HasHandCardViews()
    {
        if (handCardViews == null || handCardViews.Length < 8)
        {
            return false;
        }

        for (int i = 0; i < 8; i++)
        {
            if (handCardViews[i] == null)
            {
                return false;
            }
        }

        return true;
    }

    private void EnsurePointerInputSupport()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null && handSlotsContainer != null)
        {
            canvas = handSlotsContainer.GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        if (EventSystem.current == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }

    private void BindActionButtons()
    {
        playButton = BindPlayButton();

        discardButton = BindButton(
            "Canvas/PlayStateRoot/BottomHandArea/ActionButtonsContainer/DiscardButton",
            "DiscardButton",
            HandleDiscardButtonClicked);

        sortBySuitButton = BindButton(
            "Canvas/PlayStateRoot/BottomHandArea/ActionButtonsContainer/SortPanel/SortButtonsRow/SortBySuitButton",
            "SortBySuitButton",
            HandleSortBySuitButtonClicked);

        sortByRankButton = BindButton(
            "Canvas/PlayStateRoot/BottomHandArea/ActionButtonsContainer/SortPanel/SortButtonsRow/SortByRankButton",
            "SortByRankButton",
            HandleSortByRankButtonClicked);
    }

    private void BindDeckStatsController()
    {
        if (deckStatsUIController == null)
        {
            deckStatsUIController = GetComponent<DeckStatsUIController>();
        }

        if (deckStatsUIController == null)
        {
            deckStatsUIController = gameObject.AddComponent<DeckStatsUIController>();
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Failed to bind DeckStatsUIController: Canvas not found");
            return;
        }

        Button[] actionButtons =
        {
            playButton,
            discardButton,
            sortBySuitButton,
            sortByRankButton
        };

        deckStatsUIController.Initialize(canvas.transform, actionButtons, () => CurrentState);
    }

    private void BindCashOutController()
    {
        if (cashOutUIController == null)
        {
            cashOutUIController = GetComponent<CashOutUIController>();
        }

        if (cashOutUIController == null)
        {
            cashOutUIController = gameObject.AddComponent<CashOutUIController>();
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Failed to bind CashOutUIController: Canvas not found");
            return;
        }

        cashOutUIController.Initialize(canvas.transform);
        cashOutUIController.CashOutButtonClicked -= HandleCashOutButtonClicked;
        cashOutUIController.CashOutButtonClicked += HandleCashOutButtonClicked;
    }

    private void BindShopController()
    {
        if (shopUIController == null)
        {
            shopUIController = GetComponent<ShopUIController>();
        }

        if (shopUIController == null)
        {
            shopUIController = gameObject.AddComponent<ShopUIController>();
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Failed to bind ShopUIController: Canvas not found");
            return;
        }

        shopUIController.Initialize(canvas.transform);
        shopUIController.JokerOfferClicked -= HandleShopJokerOfferClicked;
        shopUIController.JokerOfferClicked += HandleShopJokerOfferClicked;
        shopUIController.RerollButtonClicked -= HandleShopRerollButtonClicked;
        shopUIController.RerollButtonClicked += HandleShopRerollButtonClicked;
        shopUIController.NextBlindButtonClicked -= HandleShopNextBlindButtonClicked;
        shopUIController.NextBlindButtonClicked += HandleShopNextBlindButtonClicked;
    }

    private void HandleShopJokerOfferClicked(int index)
    {
        ShopJokerOfferClicked?.Invoke(index);
    }

    private void HandleShopRerollButtonClicked()
    {
        ShopRerollButtonClicked?.Invoke();
    }

    private void HandleShopNextBlindButtonClicked()
    {
        ShopNextBlindButtonClicked?.Invoke();
    }

    private void HandleCashOutButtonClicked()
    {
        CashOutButtonClicked?.Invoke();
    }

    private Button BindPlayButton()
    {
        GameObject buttonObject = GameObject.Find(PlayButtonPath);

        if (buttonObject == null)
        {
            Debug.LogError($"Failed to find PlayButton at {PlayButtonPath}");
            return null;
        }

        Debug.Log($"Found PlayButton: {GetFullPath(buttonObject.transform)}");

        Image buttonImage = buttonObject.GetComponent<Image>();

        if (buttonImage == null)
        {
            buttonImage = buttonObject.AddComponent<Image>();
            Debug.LogWarning("PlayButton was missing Image. Added Image component at runtime.");
        }

        buttonImage.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
            Debug.LogWarning("PlayButton was missing Button. Added Button component at runtime.");
        }

        button.targetGraphic = buttonImage;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandlePlayButtonClicked);
        button.interactable = true;
        DisableTextRaycasts(buttonObject);
        Debug.Log("Bound PlayButton successfully");
        return button;
    }

    private Button BindButton(string path, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = GameObject.Find(path);

        if (buttonObject == null)
        {
            if (label == "PlayButton")
            {
                Debug.LogError("Failed to find PlayButton");
            }

            Debug.LogError($"{label} was not found at path: {path}");
            return null;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError($"{label} is missing a Button component.");
            return null;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        button.interactable = true;
        DisableTextRaycasts(buttonObject);
        Debug.Log($"Bound {label}");
        return button;
    }

    private void DisableKnownBackgroundRaycasts()
    {
        DisableDirectImageRaycast("Canvas/PlayStateRoot/BottomHandArea");
        DisableDirectImageRaycast("Canvas/PlayStateRoot/BottomHandArea/HandSlotsContainer");
        DisableDirectImageRaycast("Canvas/PlayStateRoot/BottomHandArea/ActionButtonsContainer");
        DisableDirectImageRaycast("Canvas/PlayStateRoot/BottomHandArea/ActionButtonsContainer/SortPanel");
    }

    private void DisableDirectImageRaycast(string path)
    {
        GameObject targetObject = GameObject.Find(path);

        if (targetObject == null)
        {
            return;
        }

        Image image = targetObject.GetComponent<Image>();

        if (image == null || !image.raycastTarget)
        {
            return;
        }

        image.raycastTarget = false;
        Debug.Log($"Disabled background raycast target: {GetFullPath(targetObject.transform)}");
    }

    private void LogPlayButtonDiagnostics()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GraphicRaycaster graphicRaycaster = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
        EventSystem eventSystem = EventSystem.current;
        GameObject buttonObject = GameObject.Find(PlayButtonPath);

        Debug.Log($"Canvas GraphicRaycaster: {(graphicRaycaster != null ? "present" : "missing")}");
        Debug.Log($"EventSystem: {(eventSystem != null ? "present" : "missing")}");
        Debug.Log($"PlayButton found: {(buttonObject != null ? "yes" : "no")}");

        if (buttonObject == null)
        {
            return;
        }

        Button button = buttonObject.GetComponent<Button>();
        Image image = buttonObject.GetComponent<Image>();
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();

        Debug.Log($"PlayButton activeInHierarchy: {buttonObject.activeInHierarchy}");
        Debug.Log($"PlayButton Button.interactable: {(button != null && button.interactable)}");
        Debug.Log($"PlayButton Image.raycastTarget: {(image != null && image.raycastTarget)}");
        Debug.Log($"PlayButton persistent listener count: {(button != null ? button.onClick.GetPersistentEventCount() : 0)}");

        if (rectTransform != null)
        {
            Debug.Log($"PlayButton RectTransform anchoredPosition: {rectTransform.anchoredPosition}, sizeDelta: {rectTransform.sizeDelta}");
        }

        LogPotentialPlayButtonRaycastBlockers(buttonObject, canvas);
    }

    private void LogPotentialPlayButtonRaycastBlockers(GameObject buttonObject, Canvas canvas)
    {
        RectTransform playButtonRect = buttonObject.GetComponent<RectTransform>();

        if (playButtonRect == null || canvas == null)
        {
            return;
        }

        Vector3[] worldCorners = new Vector3[4];
        playButtonRect.GetWorldCorners(worldCorners);
        Vector2 playButtonCenter = (worldCorners[0] + worldCorners[2]) * 0.5f;
        Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Image[] images = canvas.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];

            if (image == null || !image.raycastTarget || !image.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (image.gameObject == buttonObject || image.transform.IsChildOf(buttonObject.transform))
            {
                continue;
            }

            RectTransform imageRect = image.transform as RectTransform;

            if (imageRect == null)
            {
                continue;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(imageRect, playButtonCenter, eventCamera))
            {
                Debug.LogWarning($"Potential PlayButton raycast blocker: {GetFullPath(image.transform)}");
            }
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

    private GameObject FindGameObjectByPath(string path, GameObject fallback)
    {
        GameObject foundObject = GameObject.Find(path);
        return foundObject != null ? foundObject : fallback;
    }

    private GameObject FindGameObjectIncludingInactive(Transform root, string objectName, GameObject fallback)
    {
        if (root == null)
        {
            return fallback;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child.gameObject;
            }
        }

        return fallback;
    }

    private string GetFullPath(Transform target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        string path = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    private void SetActiveIfAssigned(GameObject target, bool isActive)
    {
        if (target == null)
        {
            return;
        }

        target.SetActive(isActive);
    }

    private void SetButtonInteractable(Button button, bool isInteractable)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = isInteractable;
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.text = value;
    }

}
