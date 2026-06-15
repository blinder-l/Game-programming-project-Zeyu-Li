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
    [SerializeField] private RunInfoUIController runInfoUIController;
    [SerializeField] private CardTooltipController cardTooltipController;
    [SerializeField] private CardSpriteDatabase cardSpriteDatabase;
    [SerializeField] private JokerSpriteDatabase jokerSpriteDatabase;
    [SerializeField] private Transform handSlotsContainer;
    [SerializeField] private JokerSlotView[] jokerSlotViews;
    [SerializeField] private PlayedCardEffectView[] jokerEffectViews;
    [SerializeField] private GameObject jokerSaleArea;
    [SerializeField] private Button[] jokerSaleButtons;
    [SerializeField] private HandCardView[] handCardViews;
    [SerializeField] private Button playButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button sortBySuitButton;
    [SerializeField] private Button sortByRankButton;
    [SerializeField] private TMP_Text blindNameText;
    [SerializeField] private TMP_Text targetScoreText;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text handTypeText;
    [SerializeField] private TMP_Text handTypeRankText;
    [SerializeField] private TMP_Text handsText;
    [SerializeField] private TMP_Text discardsText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text anteNumberText;
    [SerializeField] private TMP_Text scoreCalculationChipsText;
    [SerializeField] private TMP_Text scoreCalculationMultText;
    [SerializeField] private Button runInfoButton;
    [SerializeField] private GameObject resolutionInfoArea;
    [SerializeField] private GameObject playedCardsArea;
    [SerializeField] private Image[] playedCardImages;
    [SerializeField] private CardVisualFeedback[] playedCardFeedbacks;
    [SerializeField] private PlayedCardEffectView[] playedCardEffectViews;
    private PlayingCard[] playedCardSlotCards;
    [SerializeField] private HandCardView[] playedCardViews;
    [SerializeField] private TMP_Text resolutionInfoText;

    public event Action<PlayingCard> HandCardClicked;
    public event Action PlayButtonClicked;
    public event Action DiscardButtonClicked;
    public event Action SortBySuitButtonClicked;
    public event Action SortByRankButtonClicked;
    public event Action CashOutButtonClicked;
    public event Action<int> ShopJokerOfferClicked;
    public event Action<int> ShopConsumableOfferClicked;
    public event Action ShopRerollButtonClicked;
    public event Action ShopNextBlindButtonClicked;
    public event Action RunInfoButtonClicked;
    public event Action<int> JokerSaleButtonClicked;

    public GameUIState CurrentState { get; private set; }
    public bool IsDeckStatsOpen => deckStatsUIController != null && deckStatsUIController.IsDeckStatsOpen;
    public bool CanAcceptGameplayInput => CurrentState == GameUIState.PlayingBlind && !IsDeckStatsOpen && !isGameplayInputLocked;
    public bool CanUseCashOut => CurrentState == GameUIState.CashOut && !IsDeckStatsOpen;
    public bool CanUseShop => CurrentState == GameUIState.Shop && !IsDeckStatsOpen;

    private bool hasInitialized;
    private bool isGameplayInputLocked;
    private JokerEffectContext jokerEffectContext = new JokerEffectContext();
    private IReadOnlyList<JokerBase> currentEquippedJokers;
    private int selectedJokerSaleSlotIndex = -1;

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
        BindCardTooltipController();
        BindJokerBarUI();
        ClearTopConsumableSlots();
        BindHandCards();
        DisableKnownBackgroundRaycasts();
        BindActionButtons();
        BindDeckStatsController();
        BindCashOutController();
        BindShopController();
        BindRunInfoController();
        EnsurePointerInputSupport();
        LogPlayButtonDiagnostics();
        SetState(GameUIState.PlayingBlind);
        hasInitialized = true;
    }

    public void SetState(GameUIState newState)
    {
        bool enteringRunFailed = CurrentState != GameUIState.RunFailed && newState == GameUIState.RunFailed;
        CurrentState = newState;
        Debug.Log($"UI state changed: {newState}");
        deckStatsUIController?.CloseDeckStatsForStateChange();
        HideTooltipForStateChange();

        if (newState != GameUIState.PlayingBlind)
        {
            isGameplayInputLocked = false;
        }

        SetActiveIfAssigned(playStateRoot, newState == GameUIState.PlayingBlind || newState == GameUIState.RunFailed);
        SetActiveIfAssigned(cashOutPanel, newState == GameUIState.CashOut);
        SetActiveIfAssigned(shopPanel, newState == GameUIState.Shop);
        SetActiveIfAssigned(deckStatsPanel, false);
        SetActiveIfAssigned(currentHandStatsPanel, false);
        SetActiveIfAssigned(actionButtonsContainer, newState == GameUIState.PlayingBlind);
        SetActionButtonsInteractable(CanAcceptGameplayInput);
        runInfoUIController?.Hide();

        if (newState != GameUIState.CashOut)
        {
            cashOutUIController?.Hide();
        }

        if (newState != GameUIState.Shop)
        {
            shopUIController?.Hide();
        }

        if (newState == GameUIState.CashOut)
        {
            cashOutUIController?.SetButtonInteractable(true);
        }
        else
        {
            cashOutUIController?.SetButtonInteractable(false);
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

    public void RefreshHandWithTemporarilyHiddenCards(
        IReadOnlyList<PlayingCard> currentHand,
        IReadOnlyCollection<PlayingCard> hiddenCards)
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
                PlayingCard card = currentHand[i];

                if (ContainsCard(hiddenCards, card))
                {
                    cardView.ShowEmptySlot();
                }
                else
                {
                    cardView.SetCard(i, card, cardSpriteDatabase, HandleHandCardClicked);
                }
            }
            else
            {
                cardView.Clear();
            }
        }
    }

    public void RefreshScoreCalculation(int chips, float mult)
    {
        SetText(scoreCalculationChipsText, chips.ToString());
        SetText(scoreCalculationMultText, FormatMult(mult));
        Debug.Log($"Score calculation UI updated: chips = {chips}, mult = {FormatMult(mult)}");
    }

    public void SetGameplayInputLocked(bool isLocked)
    {
        isGameplayInputLocked = isLocked;
        SetActionButtonsInteractable(CanAcceptGameplayInput);
        Debug.Log($"Gameplay input locked: {isGameplayInputLocked}");
    }

    public void RefreshJokerBar(IReadOnlyList<JokerBase> equippedJokers)
    {
        EnsureJokerSlotsBound();
        EnsureJokerSaleButtonsBound();
        ResolveJokerSpriteDatabaseIfNeeded();
        currentEquippedJokers = equippedJokers;

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
            slotView.SetJoker(joker, jokerSpriteDatabase, jokerEffectContext);
            slotView.SetClickHandler(i, HandleJokerSlotClicked);
        }

        UpdateVisibleJokerSaleButton();
        Debug.Log($"Joker bar updated: {equippedCount} equipped jokers");
    }

    public void HideJokerSaleButtons()
    {
        EnsureJokerSaleButtonsBound();
        selectedJokerSaleSlotIndex = -1;

        if (jokerSaleButtons == null)
        {
            return;
        }

        for (int i = 0; i < jokerSaleButtons.Length; i++)
        {
            if (jokerSaleButtons[i] != null)
            {
                jokerSaleButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetJokerTooltipContext(int ownedStoneCardCount)
    {
        if (jokerEffectContext == null)
        {
            jokerEffectContext = new JokerEffectContext();
        }

        jokerEffectContext.ownedStoneCardCount = ownedStoneCardCount;

        if (shopUIController != null)
        {
            shopUIController.SetJokerTooltipContext(jokerEffectContext);
        }
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

    public void SetRunInfoSources(
        HandTypeLevelManager handTypeLevelManager,
        SuitMasteryManager suitMasteryManager,
        IReadOnlyDictionary<PokerHandType, int> handTypePlayCounts)
    {
        if (runInfoUIController == null)
        {
            BindRunInfoController();
        }

        runInfoUIController?.SetDataSources(handTypeLevelManager, suitMasteryManager, handTypePlayCounts);
        runInfoUIController?.Refresh();
    }

    public void ShowCashOut(
        int targetScore,
        int currentScore,
        int fixedBlindReward,
        int suitGoldThisBlind,
        int bonusCardGoldThisBlind,
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
            bonusCardGoldThisBlind,
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
        ShowShop(offers, null);
    }

    public void ShowShop(IReadOnlyList<ShopOffer> offers, IReadOnlyList<PlanetShopOffer> consumableOffers)
    {
        if (shopUIController == null)
        {
            BindShopController();
        }

        shopUIController?.ShowShop(offers, consumableOffers);
    }

    public void RefreshShopOffers(IReadOnlyList<ShopOffer> offers)
    {
        shopUIController?.RefreshOffers(offers);
    }

    public void RefreshShopConsumableOffers(IReadOnlyList<PlanetShopOffer> consumableOffers)
    {
        shopUIController?.RefreshConsumableOffers(consumableOffers);
    }

    public void RefreshBlindStatus(
        string blindName,
        int targetScore,
        int currentScore,
        string latestHandType,
        string handTypeRank,
        int handsRemaining,
        int discardsRemaining,
        int currentGold,
        int anteNumber)
    {
        SetText(blindNameText, CurrentState == GameUIState.RunFailed ? "Run Failed" : blindName);
        SetText(targetScoreText, targetScore.ToString());
        SetText(currentScoreText, currentScore.ToString());
        SetText(handTypeText, latestHandType);
        SetText(handTypeRankText, handTypeRank);
        SetText(handsText, handsRemaining.ToString());
        SetText(discardsText, discardsRemaining.ToString());
        SetText(goldText, currentGold.ToString());
        SetText(anteNumberText, anteNumber.ToString());
        Debug.Log($"Left status UI updated: score = {currentScore}, hands = {handsRemaining}");
    }

    public void RefreshHandTypeText(string handTypeTextValue)
    {
        RefreshHandTypeText(handTypeTextValue, "-");
    }

    public void RefreshHandTypeText(string handTypeTextValue, string handTypeRankValue)
    {
        SetText(handTypeText, handTypeTextValue);
        SetText(handTypeRankText, handTypeRankValue);
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
                playedCardSlotCards[i] = playedCards[i];
                cardImage.gameObject.SetActive(true);
                cardImage.enabled = true;
                cardImage.sprite = cardSpriteDatabase != null ? cardSpriteDatabase.GetSprite(playedCards[i]) : null;
                cardImage.raycastTarget = false;
                SetPlayedCardVisualContent(i, cardImage.sprite != null);
                displayedCount++;
            }
            else
            {
                playedCardSlotCards[i] = null;
                cardImage.sprite = null;
                cardImage.enabled = false;
                SetPlayedCardVisualContent(i, false);
                cardImage.gameObject.SetActive(false);
            }
        }

        Debug.Log($"Played cards UI updated: {displayedCount} cards");
    }

    public List<PlayingCard> ShowPendingPlayedCards(IReadOnlyList<PlayingCard> playedCards)
    {
        List<PlayingCard> sortedCards = GetCardsSortedForPlayedArea(playedCards);
        RefreshPlayedCards(sortedCards);
        ClearPlayedCardEffects();
        ClearJokerEffects();
        return sortedCards;
    }

    public void PlayCardChipEffect(int slotIndex, PlayingCard card, int chipValue)
    {
        PlayCardChipEffect(slotIndex, card, chipValue, $"+{chipValue}");
    }

    public void PlayCardScoreEvent(CardScoreEvent scoreEvent)
    {
        if (scoreEvent == null)
        {
            Debug.LogWarning("Cannot play card score event: event is null.");
            return;
        }

        PlayCardChipEffect(
            scoreEvent.playedCardSlotIndex,
            scoreEvent.card,
            scoreEvent.chipsAdded,
            string.IsNullOrEmpty(scoreEvent.effectText) ? $"+{scoreEvent.chipsAdded}" : scoreEvent.effectText);
    }

    public void PlayCardChipEffect(int slotIndex, PlayingCard card, int chipValue, string effectText)
    {
        EnsurePlayedCardsBound();
        EnsureResolutionInfoBound();

        if (!IsValidPlayedCardSlot(slotIndex))
        {
            Debug.LogWarning($"Cannot play card chip effect: invalid played card slot {slotIndex}.");
            return;
        }

        PlayingCard slotCard = playedCardSlotCards != null ? playedCardSlotCards[slotIndex] : null;

        if (slotCard != card)
        {
            Debug.LogWarning($"Cannot play card chip effect: PlayedCard{slotIndex + 1} contains {slotCard}, but requested effect for {card}.");
            return;
        }

        if (playedCardFeedbacks != null && slotIndex >= 0 && slotIndex < playedCardFeedbacks.Length && playedCardFeedbacks[slotIndex] != null)
        {
            playedCardFeedbacks[slotIndex].PlayScorePulse();
        }

        if (playedCardEffectViews != null && slotIndex >= 0 && slotIndex < playedCardEffectViews.Length && playedCardEffectViews[slotIndex] != null)
        {
            playedCardEffectViews[slotIndex].PlayEffect(effectText);
        }

        Debug.Log($"PlayedCard{slotIndex + 1} scored {card.GetDisplayName()}; PlayedCardEffect{slotIndex + 1}: {effectText}");
    }

    public void PlayJokerScoreEvent(JokerScoreEvent scoreEvent)
    {
        EnsureJokerSlotsBound();
        EnsureJokerEffectsBound();

        if (scoreEvent == null)
        {
            Debug.LogWarning("Cannot play Joker score event: event is null.");
            return;
        }

        int slotIndex = scoreEvent.jokerSlotIndex;

        if (slotIndex < 0 || slotIndex >= 5)
        {
            Debug.LogWarning($"Cannot play Joker score event: invalid Joker slot {slotIndex}.");
            return;
        }

        if (jokerSlotViews != null && slotIndex < jokerSlotViews.Length && jokerSlotViews[slotIndex] != null)
        {
            jokerSlotViews[slotIndex].PlayScorePulse();
        }

        if (jokerEffectViews != null && slotIndex < jokerEffectViews.Length && jokerEffectViews[slotIndex] != null)
        {
            string effectText = string.IsNullOrEmpty(scoreEvent.effectText) ? "!" : scoreEvent.effectText;
            jokerEffectViews[slotIndex].PlayEffect(effectText);
            Debug.Log($"JokerSlot{slotIndex + 1} triggered {scoreEvent.effectSource}; JokerEffect{slotIndex + 1}: {effectText}");
        }
    }

    public void ClearPlayedCardEffects()
    {
        EnsureResolutionInfoBound();

        if (playedCardEffectViews == null)
        {
            return;
        }

        for (int i = 0; i < playedCardEffectViews.Length; i++)
        {
            if (playedCardEffectViews[i] != null)
            {
                playedCardEffectViews[i].ClearImmediate();
            }
        }
    }

    public void ClearJokerEffects()
    {
        EnsureJokerEffectsBound();

        if (jokerEffectViews == null)
        {
            return;
        }

        for (int i = 0; i < jokerEffectViews.Length; i++)
        {
            jokerEffectViews[i]?.ClearImmediate();
        }
    }

    public void ClearPlayedCards()
    {
        EnsurePlayedCardsBound();

        if (playedCardImages == null)
        {
            return;
        }

        for (int i = 0; i < playedCardImages.Length; i++)
        {
            Image cardImage = playedCardImages[i];

            if (cardImage == null)
            {
                continue;
            }

            cardImage.sprite = null;
            cardImage.enabled = false;
            if (playedCardSlotCards != null && i < playedCardSlotCards.Length)
            {
                playedCardSlotCards[i] = null;
            }
            SetPlayedCardVisualContent(i, false);
        }

        Debug.Log("Played cards UI cleared");
    }

    public void RefreshResolutionInfo(ScoreContext scoreContext)
    {
        EnsureResolutionInfoBound();

        if (resolutionInfoText != null)
        {
            resolutionInfoText.text = string.Empty;
        }

        if (scoreContext == null)
        {
            ClearPlayedCardEffects();
        }

        Debug.Log("Resolution info updated");
    }

    private void HandleHandCardClicked(PlayingCard card)
    {
        if (!CanAcceptGameplayInput)
        {
            Debug.Log(GetGameplayBlockedMessage("select card"));
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
            Debug.Log(GetGameplayBlockedMessage("play"));
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
            Debug.Log(GetGameplayBlockedMessage("discard"));
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
            Debug.Log(GetGameplayBlockedMessage("sort"));
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
            Debug.Log(GetGameplayBlockedMessage("sort"));
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
        Canvas canvas = FindFirstObjectByType<Canvas>();
        Transform canvasRoot = canvas != null ? canvas.transform : null;
        GameObject leftPanelObject = FindGameObjectIncludingInactive(canvasRoot, "LeftBlindPanel", null);
        Transform leftPanelRoot = leftPanelObject != null ? leftPanelObject.transform : null;

        if (leftPanelRoot == null)
        {
            Debug.LogError("Failed to bind LeftBlindPanel");
            return;
        }

        blindNameText = BindTextInRoot(leftPanelRoot, "BlindNameText", "BlindNameText");
        targetScoreText = BindTextInRoot(leftPanelRoot, "TargetScoreText", "TargetScoreText");
        currentScoreText = BindTextInRoot(leftPanelRoot, "CurrentScoreText", "CurrentScoreText");
        handTypeText = BindTextInRoot(leftPanelRoot, "HandTypeText", "HandTypeText");
        handTypeRankText = BindTextInRoot(leftPanelRoot, "HandTypeRankText", "HandTypeRankText");
        handsText = BindTextInRoot(leftPanelRoot, "HandsNumberText", "HandsNumberText");
        discardsText = BindTextInRoot(leftPanelRoot, "DiscardsNumberText", "DiscardsNumberText", false);

        if (discardsText == null)
        {
            discardsText = BindTextInRoot(leftPanelRoot, "DiscradsNumberText", "DiscradsNumberText");

            if (discardsText != null)
            {
                Debug.LogWarning("Bound DiscradsNumberText. Consider renaming it to DiscardsNumberText in the scene.");
            }
        }

        goldText = BindTextInRoot(leftPanelRoot, "GoldNumberText", "GoldNumberText");
        anteNumberText = BindTextInRoot(leftPanelRoot, "AnteNumberText", "AnteNumberText");
        BindScoreCalculationUI(leftPanelRoot);
        runInfoButton = BindOptionalButtonInRoot(leftPanelRoot, "RunInfoButton", "RunInfoButton");

        if (runInfoButton != null)
        {
            runInfoButton.onClick.RemoveAllListeners();
            runInfoButton.onClick.AddListener(HandleRunInfoButtonClicked);
        }

        Debug.Log("Bound left status UI");
    }

    private void ResolveJokerSpriteDatabaseIfNeeded()
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

    private void BindScoreCalculationUI(Transform leftPanelRoot)
    {
        Transform scoreCalculationRoot = FindChildByTrimmedName(leftPanelRoot, "ScoreCalculationModule");

        if (scoreCalculationRoot == null)
        {
            Debug.LogError("Failed to bind ScoreCalculationModule");
            return;
        }

        scoreCalculationChipsText = BindTextInRoot(scoreCalculationRoot, "ChipsText", "ScoreCalculation ChipsText");
        scoreCalculationMultText = BindTextInRoot(scoreCalculationRoot, "MultText", "ScoreCalculation MultText");
        RefreshScoreCalculation(0, 0f);
        Debug.Log("Bound score calculation UI");
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
            slotView.SetTooltipController(cardTooltipController);
            jokerSlotViews[i] = slotView;
            Debug.Log($"Bound {slotName}");
        }

        BindJokerEffectsUI();
        BindJokerSaleButtonsUI();
    }

    private void BindJokerSaleButtonsUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        GameObject saleAreaObject = canvas != null
            ? FindGameObjectIncludingInactive(canvas.transform, "JokerSaleArea", jokerSaleArea)
            : GameObject.Find("Canvas/TopJokerBar/JokerSaleArea");

        if (saleAreaObject == null)
        {
            Debug.LogWarning("Failed to bind TopJokerBar/JokerSaleArea");
            return;
        }

        jokerSaleArea = saleAreaObject;
        jokerSaleButtons = new Button[5];

        for (int i = 0; i < jokerSaleButtons.Length; i++)
        {
            string buttonName = $"JokerSaleButton{i + 1}";
            GameObject buttonObject = FindGameObjectIncludingInactive(jokerSaleArea.transform, buttonName, null);

            if (buttonObject == null)
            {
                Debug.LogWarning($"Failed to bind {buttonName}");
                continue;
            }

            Image buttonImage = buttonObject.GetComponent<Image>();

            if (buttonImage == null)
            {
                buttonImage = buttonObject.AddComponent<Image>();
            }

            buttonImage.raycastTarget = true;
            Button button = buttonObject.GetComponent<Button>();

            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            int capturedIndex = i;
            button.targetGraphic = buttonImage;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HandleJokerSaleButtonClicked(capturedIndex));
            button.interactable = true;
            DisableTextRaycasts(buttonObject);
            buttonObject.SetActive(false);
            jokerSaleButtons[i] = button;
            Debug.Log($"Bound {buttonName}");
        }
    }

    private void ClearTopConsumableSlots()
    {
        GameObject containerObject = GameObject.Find("Canvas/TopJokerBar/ConsumableSlotsContainer");

        if (containerObject == null)
        {
            return;
        }

        for (int i = 1; i <= 2; i++)
        {
            Transform slotTransform = containerObject.transform.Find($"ConsumableSlot{i}");

            if (slotTransform == null)
            {
                continue;
            }

            ClearEmptyTopCardSlot(slotTransform.gameObject);
            Debug.Log($"Cleared empty ConsumableSlot{i}");
        }
    }

    private void ClearEmptyTopCardSlot(GameObject slotObject)
    {
        if (slotObject == null)
        {
            return;
        }

        Image[] images = slotObject.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < images.Length; i++)
        {
            images[i].sprite = null;
            Color color = images[i].color;
            color.a = 0f;
            images[i].color = color;
            images[i].raycastTarget = false;
        }

        TMP_Text[] texts = slotObject.GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = string.Empty;
            texts[i].raycastTarget = false;
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
        BindJokerEffectsUI();
        BindJokerSaleButtonsUI();
    }

    private void EnsureJokerSaleButtonsBound()
    {
        if (jokerSaleButtons != null && jokerSaleButtons.Length == 5)
        {
            bool hasAnyButton = false;

            for (int i = 0; i < jokerSaleButtons.Length; i++)
            {
                if (jokerSaleButtons[i] != null)
                {
                    hasAnyButton = true;
                    break;
                }
            }

            if (hasAnyButton)
            {
                return;
            }
        }

        BindJokerSaleButtonsUI();
    }

    private void EnsureJokerEffectsBound()
    {
        if (jokerEffectViews != null && jokerEffectViews.Length == 5)
        {
            bool hasAllEffects = true;

            for (int i = 0; i < jokerEffectViews.Length; i++)
            {
                if (jokerEffectViews[i] == null)
                {
                    hasAllEffects = false;
                    break;
                }
            }

            if (hasAllEffects)
            {
                return;
            }
        }

        BindJokerEffectsUI();
    }

    private void BindJokerEffectsUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        GameObject effectArea = canvas != null
            ? FindGameObjectIncludingInactive(canvas.transform, "JokerResolutionInfoArea", null)
            : GameObject.Find("Canvas/TopJokerBar/JokerResolutionInfoArea");

        if (effectArea == null)
        {
            Debug.LogWarning("Failed to find TopJokerBar/JokerResolutionInfoArea");
            return;
        }

        jokerEffectViews = new PlayedCardEffectView[5];

        for (int i = 0; i < jokerEffectViews.Length; i++)
        {
            string effectName = $"JokerEffect{i + 1}";
            GameObject effectObject = FindGameObjectIncludingInactive(effectArea.transform, effectName, null);

            if (effectObject == null)
            {
                Debug.LogWarning($"Failed to bind {effectName}");
                continue;
            }

            effectObject.SetActive(true);
            PlayedCardEffectView effectView = effectObject.GetComponent<PlayedCardEffectView>();

            if (effectView == null)
            {
                effectView = effectObject.AddComponent<PlayedCardEffectView>();
            }

            effectView.ClearImmediate();
            jokerEffectViews[i] = effectView;
            Debug.Log($"Bound {effectName}: {GetFullPath(effectObject.transform)} at {effectView.BaseAnchoredPosition}");
        }
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

    private TMP_Text BindTextInRoot(Transform root, string objectName, string label, bool logErrorIfMissing = true)
    {
        GameObject textObject = FindGameObjectIncludingInactive(root, objectName, null);

        if (textObject == null)
        {
            if (logErrorIfMissing)
            {
                Debug.LogError($"Failed to bind {label}");
            }

            return null;
        }

        TMP_Text boundText = textObject.GetComponent<TMP_Text>();

        if (boundText == null)
        {
            boundText = textObject.GetComponentInChildren<TMP_Text>(true);
        }

        if (boundText == null)
        {
            Debug.LogError($"Failed to bind {label}: TMP_Text missing");
            return null;
        }

        boundText.raycastTarget = false;
        Debug.Log($"Bound {label}");
        return boundText;
    }

    private Button BindOptionalButtonInRoot(Transform root, string objectName, string label)
    {
        GameObject buttonObject = FindGameObjectIncludingInactive(root, objectName, null);

        if (buttonObject == null)
        {
            Debug.LogWarning($"Optional {label} not found");
            return null;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        Image image = buttonObject.GetComponent<Image>();

        if (image != null)
        {
            image.raycastTarget = true;
            button.targetGraphic = image;
        }

        DisableTextRaycasts(buttonObject);
        Debug.Log($"Bound {label}");
        return button;
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
        playedCardFeedbacks = new CardVisualFeedback[5];
        playedCardSlotCards = new PlayingCard[5];

        for (int i = 0; i < playedCardImages.Length; i++)
        {
            string cardName = $"PlayedCard{i + 1}";
            Transform cardTransform = playedCardsArea.transform.Find(cardName);

            if (cardTransform == null)
            {
                Debug.LogError($"Failed to bind {cardName}");
                continue;
            }

            Image cardImage = GetOrCreatePlayedCardVisualImage(cardTransform);

            if (cardImage == null)
            {
                Debug.LogError($"Failed to bind {cardName}");
                continue;
            }

            cardImage.raycastTarget = false;
            playedCardImages[i] = cardImage;

            CardVisualFeedback visualFeedback = cardTransform.GetComponent<CardVisualFeedback>();

            if (visualFeedback == null)
            {
                visualFeedback = cardTransform.gameObject.AddComponent<CardVisualFeedback>();
            }

            visualFeedback.SetVisualRoot(cardImage.transform as RectTransform);
            visualFeedback.SetHasVisualContent(false);
            playedCardFeedbacks[i] = visualFeedback;
            Debug.Log($"Bound {cardName}: {GetFullPath(cardTransform)}");
        }

        Debug.Log("Bound played cards UI");
    }

    private void EnsurePlayedCardsBound()
    {
        if (playedCardImages != null && playedCardImages.Length == 5 && playedCardFeedbacks != null && playedCardFeedbacks.Length == 5)
        {
            bool hasAllCards = true;

            for (int i = 0; i < playedCardImages.Length; i++)
            {
                if (playedCardImages[i] == null || playedCardFeedbacks[i] == null)
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

        if (resolutionInfoText != null)
        {
            resolutionInfoText.raycastTarget = false;
            resolutionInfoText.text = string.Empty;
            Debug.Log("Bound ResolutionInfoText");
        }

        playedCardEffectViews = new PlayedCardEffectView[5];

        for (int i = 0; i < playedCardEffectViews.Length; i++)
        {
            string effectName = $"PlayedCardEffect{i + 1}";
            GameObject effectObject = FindGameObjectIncludingInactive(resolutionInfoArea.transform, effectName, null);

            if (effectObject == null)
            {
                Debug.LogError($"Failed to bind {effectName}");
                continue;
            }

            effectObject.SetActive(true);
            PlayedCardEffectView effectView = effectObject.GetComponent<PlayedCardEffectView>();

            if (effectView == null)
            {
                effectView = effectObject.AddComponent<PlayedCardEffectView>();
            }

            effectView.ClearImmediate();
            playedCardEffectViews[i] = effectView;
            Debug.Log($"Bound {effectName}: {GetFullPath(effectObject.transform)} at {effectView.BaseAnchoredPosition}");
        }

        Debug.Log("Bound resolution info UI");
    }

    private Image GetOrCreatePlayedCardVisualImage(Transform cardTransform)
    {
        if (cardTransform == null)
        {
            return null;
        }

        Image rootImage = cardTransform.GetComponent<Image>();

        if (rootImage != null)
        {
            rootImage.sprite = null;
            rootImage.color = new Color(1f, 1f, 1f, 0.01f);
            rootImage.raycastTarget = false;
        }

        Transform visualTransform = cardTransform.Find("CardVisual");
        RectTransform visualRect = visualTransform as RectTransform;

        if (visualRect == null)
        {
            GameObject visualObject = new GameObject("CardVisual", typeof(RectTransform));
            visualObject.transform.SetParent(cardTransform, false);
            visualRect = visualObject.GetComponent<RectTransform>();
            visualRect.anchorMin = Vector2.zero;
            visualRect.anchorMax = Vector2.one;
            visualRect.offsetMin = Vector2.zero;
            visualRect.offsetMax = Vector2.zero;
        }

        Image visualImage = visualRect.GetComponent<Image>();

        if (visualImage == null)
        {
            visualImage = visualRect.gameObject.AddComponent<Image>();
        }

        visualImage.raycastTarget = false;
        return visualImage;
    }

    private void EnsureResolutionInfoBound()
    {
        if (playedCardEffectViews != null && playedCardEffectViews.Length == 5)
        {
            bool hasAllEffects = true;

            for (int i = 0; i < playedCardEffectViews.Length; i++)
            {
                if (playedCardEffectViews[i] == null)
                {
                    hasAllEffects = false;
                    break;
                }
            }

            if (hasAllEffects)
            {
                return;
            }
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

            if (cardTransform.GetComponent<CardVisualFeedback>() == null)
            {
                cardTransform.gameObject.AddComponent<CardVisualFeedback>();
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
        shopUIController.SetInputGuard(() => CanUseShop, GetShopBlockedMessage);
        shopUIController.SetTooltipController(cardTooltipController);
        shopUIController.SetJokerTooltipContext(jokerEffectContext);
        shopUIController.JokerOfferClicked -= HandleShopJokerOfferClicked;
        shopUIController.JokerOfferClicked += HandleShopJokerOfferClicked;
        shopUIController.ConsumableOfferClicked -= HandleShopConsumableOfferClicked;
        shopUIController.ConsumableOfferClicked += HandleShopConsumableOfferClicked;
        shopUIController.RerollButtonClicked -= HandleShopRerollButtonClicked;
        shopUIController.RerollButtonClicked += HandleShopRerollButtonClicked;
        shopUIController.NextBlindButtonClicked -= HandleShopNextBlindButtonClicked;
        shopUIController.NextBlindButtonClicked += HandleShopNextBlindButtonClicked;
    }

    private void BindRunInfoController()
    {
        if (runInfoUIController == null)
        {
            runInfoUIController = GetComponent<RunInfoUIController>();
        }

        if (runInfoUIController == null)
        {
            runInfoUIController = gameObject.AddComponent<RunInfoUIController>();
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Failed to bind RunInfoUIController: Canvas not found");
            return;
        }

        runInfoUIController.Initialize(canvas.transform);
    }

    private void HandleShopJokerOfferClicked(int index)
    {
        if (!CanUseShop)
        {
            Debug.Log(GetShopBlockedMessage("purchase"));
            return;
        }

        ShopJokerOfferClicked?.Invoke(index);
    }

    private void HandleShopConsumableOfferClicked(int index)
    {
        if (!CanUseShop)
        {
            Debug.Log(GetShopBlockedMessage("purchase"));
            return;
        }

        ShopConsumableOfferClicked?.Invoke(index);
    }

    private void HandleShopRerollButtonClicked()
    {
        if (!CanUseShop)
        {
            Debug.Log(GetShopBlockedMessage("reroll"));
            return;
        }

        ShopRerollButtonClicked?.Invoke();
    }

    private void HandleShopNextBlindButtonClicked()
    {
        if (!CanUseShop)
        {
            Debug.Log(GetShopBlockedMessage("go to next blind"));
            return;
        }

        ShopNextBlindButtonClicked?.Invoke();
    }

    private void HandleJokerSlotClicked(int slotIndex)
    {
        JokerBase joker = GetEquippedJokerAt(slotIndex);

        if (joker == null)
        {
            HideJokerSaleButtons();
            return;
        }

        EnsureJokerSaleButtonsBound();

        if (selectedJokerSaleSlotIndex == slotIndex && IsJokerSaleButtonVisible(slotIndex))
        {
            HideJokerSaleButtons();
            Debug.Log($"Joker sale button hidden: slot {slotIndex + 1}");
            return;
        }

        selectedJokerSaleSlotIndex = slotIndex;
        UpdateVisibleJokerSaleButton();
    }

    private void HandleJokerSaleButtonClicked(int slotIndex)
    {
        JokerBase joker = GetEquippedJokerAt(slotIndex);

        if (joker == null)
        {
            HideJokerSaleButtons();
            Debug.Log($"Cannot sell Joker: slot {slotIndex + 1} is empty");
            return;
        }

        Debug.Log($"UI JokerSaleButton clicked: slot {slotIndex + 1}");
        JokerSaleButtonClicked?.Invoke(slotIndex);
    }

    private void UpdateVisibleJokerSaleButton()
    {
        EnsureJokerSaleButtonsBound();

        if (jokerSaleButtons == null)
        {
            return;
        }

        for (int i = 0; i < jokerSaleButtons.Length; i++)
        {
            Button saleButton = jokerSaleButtons[i];

            if (saleButton == null)
            {
                continue;
            }

            JokerBase joker = GetEquippedJokerAt(i);
            bool shouldShow = i == selectedJokerSaleSlotIndex && joker != null;
            saleButton.gameObject.SetActive(shouldShow);
            saleButton.interactable = shouldShow;

            if (shouldShow)
            {
                int sellPrice = GetJokerSellPrice(joker);
                TMP_Text buttonText = saleButton.GetComponentInChildren<TMP_Text>(true);
                SetText(buttonText, $"Sell ${sellPrice}");
                Debug.Log($"Joker sale button shown: slot {i + 1}, sell price ${sellPrice}");
            }
        }
    }

    private bool IsJokerSaleButtonVisible(int slotIndex)
    {
        return jokerSaleButtons != null
            && slotIndex >= 0
            && slotIndex < jokerSaleButtons.Length
            && jokerSaleButtons[slotIndex] != null
            && jokerSaleButtons[slotIndex].gameObject.activeSelf;
    }

    private JokerBase GetEquippedJokerAt(int slotIndex)
    {
        if (currentEquippedJokers == null || slotIndex < 0 || slotIndex >= currentEquippedJokers.Count)
        {
            return null;
        }

        return currentEquippedJokers[slotIndex];
    }

    private int GetJokerSellPrice(JokerBase joker)
    {
        return joker != null ? Mathf.Max(1, joker.Cost - 2) : 1;
    }

    private void HandleRunInfoButtonClicked()
    {
        Debug.Log("UI RunInfoButton clicked");
        RunInfoButtonClicked?.Invoke();

        if (runInfoUIController == null)
        {
            BindRunInfoController();
        }

        if (runInfoUIController == null)
        {
            Debug.LogError("Cannot show RunInfoPanel: RunInfoUIController is null");
            return;
        }

        runInfoUIController.ShowDefault();
    }

    private void BindCardTooltipController()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        Transform canvasRoot = canvas != null ? canvas.transform : null;
        GameObject tooltipPanelObject = FindGameObjectIncludingInactive(canvasRoot, "CardTooltipPanel", cardTooltipPanel);

        if (tooltipPanelObject == null)
        {
            Debug.LogError("Failed to bind CardTooltipPanel");
            return;
        }

        cardTooltipPanel = tooltipPanelObject;
        cardTooltipController = tooltipPanelObject.GetComponent<CardTooltipController>();

        if (cardTooltipController == null)
        {
            cardTooltipController = tooltipPanelObject.AddComponent<CardTooltipController>();
        }

        cardTooltipController.Initialize(canvasRoot);
    }

    private void HandleCashOutButtonClicked()
    {
        if (!CanUseCashOut)
        {
            Debug.Log(GetCashOutBlockedMessage());
            return;
        }

        CashOutButtonClicked?.Invoke();
    }

    private void HideTooltipForStateChange()
    {
        if (cardTooltipController != null)
        {
            cardTooltipController.HideTooltip();
            Debug.Log("Tooltip hidden due to state change.");
            return;
        }

        SetActiveIfAssigned(cardTooltipPanel, false);
    }

    private string GetGameplayBlockedMessage(string action)
    {
        if (IsDeckStatsOpen)
        {
            return $"Cannot {action}: deck view is open.";
        }

        if (isGameplayInputLocked)
        {
            return $"Cannot {action}: hand scoring is resolving.";
        }

        return $"Cannot {action}: current state is {CurrentState}.";
    }

    private string GetCashOutBlockedMessage()
    {
        if (IsDeckStatsOpen)
        {
            return "Cannot claim CashOut: deck view is open.";
        }

        return $"Cannot claim CashOut: current state is {CurrentState}.";
    }

    private string GetShopBlockedMessage(string action)
    {
        if (IsDeckStatsOpen)
        {
            return $"Cannot {action}: deck view is open.";
        }

        return $"Cannot {action}: current state is {CurrentState}.";
    }

    private Button BindPlayButton()
    {
        GameObject buttonObject = FindPlayButtonObject();

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
        GameObject buttonObject = FindPlayButtonObject();

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

    private GameObject FindPlayButtonObject()
    {
        GameObject buttonObject = GameObject.Find(PlayButtonPath);

        if (buttonObject != null)
        {
            return buttonObject;
        }

        GameObject actionContainer = GameObject.Find("Canvas/PlayStateRoot/BottomHandArea/ActionButtonsContainer");

        if (actionContainer == null)
        {
            return null;
        }

        Transform matchingChild = FindChildByTrimmedName(actionContainer.transform, "PlayButton");

        if (matchingChild == null)
        {
            return null;
        }

        Debug.LogWarning($"Found PlayButton by trimmed name. Current object name is '{matchingChild.name}'. Consider renaming it to 'PlayButton'.");
        return matchingChild.gameObject;
    }

    private Transform FindChildByTrimmedName(Transform parent, string trimmedName)
    {
        if (parent == null)
        {
            return null;
        }

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Trim() == trimmedName)
            {
                return child;
            }
        }

        return null;
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

    private List<PlayingCard> GetCardsSortedForPlayedArea(IReadOnlyList<PlayingCard> cards)
    {
        List<PlayingCard> sortedCards = cards != null ? new List<PlayingCard>(cards) : new List<PlayingCard>();
        sortedCards.Sort((firstCard, secondCard) =>
        {
            int rankComparison = secondCard.rank.CompareTo(firstCard.rank);

            if (rankComparison != 0)
            {
                return rankComparison;
            }

            return GetSuitSortValue(firstCard.suit).CompareTo(GetSuitSortValue(secondCard.suit));
        });

        return sortedCards;
    }

    private int GetSuitSortValue(Suit suit)
    {
        switch (suit)
        {
            case Suit.Hearts:
                return 0;
            case Suit.Spades:
                return 1;
            case Suit.Diamonds:
                return 2;
            default:
                return 3;
        }
    }

    private void SetPlayedCardVisualContent(int slotIndex, bool hasVisualContent)
    {
        if (playedCardFeedbacks == null || slotIndex < 0 || slotIndex >= playedCardFeedbacks.Length)
        {
            return;
        }

        if (playedCardFeedbacks[slotIndex] != null)
        {
            playedCardFeedbacks[slotIndex].SetHasVisualContent(hasVisualContent);
        }
    }

    private bool IsValidPlayedCardSlot(int slotIndex)
    {
        return playedCardSlotCards != null
            && playedCardFeedbacks != null
            && playedCardEffectViews != null
            && slotIndex >= 0
            && slotIndex < playedCardSlotCards.Length
            && slotIndex < playedCardFeedbacks.Length
            && slotIndex < playedCardEffectViews.Length;
    }

    private bool ContainsCard(IReadOnlyCollection<PlayingCard> cards, PlayingCard targetCard)
    {
        if (cards == null || targetCard == null)
        {
            return false;
        }

        foreach (PlayingCard card in cards)
        {
            if (card == targetCard)
            {
                return true;
            }
        }

        return false;
    }

    private string FormatMult(float mult)
    {
        return Math.Abs(mult % 1f) < 0.001f ? ((int)Math.Round(mult)).ToString() : mult.ToString("0.##");
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
