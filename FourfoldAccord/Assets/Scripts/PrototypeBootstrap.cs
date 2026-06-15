using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private const int StartingGold = 10;
    private const int MaxSelectedCards = 5;
    private const float CardScoreStepDelay = 0.7f;
    private const float FinalScoreHoldDelay = 0.45f;

    [SerializeField] private GameUIController gameUIController;

    private DeckManager deckManager;
    private HandManager handManager;
    private PokerHandEvaluator pokerHandEvaluator;
    private ScoreManager scoreManager;
    private RoundManager roundManager;
    private RunManager runManager;
    private ShopManager shopManager;
    private SuitMasteryManager suitMasteryManager;
    private HandTypeLevelManager handTypeLevelManager;
    private JokerManager jokerManager;
    private bool isInShop;
    private int currentGold;
    private int suitGoldThisBlind;
    private int bonusCardGoldThisBlind;
    private int lastCashOutTotal;
    private bool hasClaimedCashOut;
    private bool heldCardEffectsProcessedThisBlind;
    private bool isResolvingPlayedHand;
    private bool hasPreparedDeckForNextBlind;
    private string latestHandTypeText = "None";
    private string latestHandTypeRankText = "-";
    private List<PlayingCard> latestPlayedCards = new List<PlayingCard>();
    private ScoreContext latestScoreContext;
    private readonly List<PlayingCard> selectedCards = new List<PlayingCard>();
    private readonly Dictionary<PokerHandType, int> handTypePlayCounts = new Dictionary<PokerHandType, int>();

    private void Awake()
    {
        EnsureGameUIController();
        EnsureCardModifierDebugTester();
    }

    private void Start()
    {
        pokerHandEvaluator = new PokerHandEvaluator();
        scoreManager = new ScoreManager();
        runManager = new RunManager();
        shopManager = new ShopManager();
        suitMasteryManager = new SuitMasteryManager();
        handTypeLevelManager = new HandTypeLevelManager();
        InitializeHandTypePlayCounts();
        jokerManager = new JokerManager();
        currentGold = StartingGold;

        if (gameUIController != null)
        {
            gameUIController.HandCardClicked += HandleHandCardClicked;
            gameUIController.PlayButtonClicked += HandlePlayButtonClicked;
            gameUIController.DiscardButtonClicked += HandleDiscardButtonClicked;
            gameUIController.SortBySuitButtonClicked += HandleSortBySuitButtonClicked;
            gameUIController.SortByRankButtonClicked += HandleSortByRankButtonClicked;
            gameUIController.CashOutButtonClicked += HandleCashOutButtonClicked;
            gameUIController.ShopJokerOfferClicked += HandleShopJokerOfferClicked;
            gameUIController.ShopConsumableOfferClicked += HandleShopConsumableOfferClicked;
            gameUIController.ShopRerollButtonClicked += HandleShopRerollButtonClicked;
            gameUIController.ShopNextBlindButtonClicked += HandleShopNextBlindButtonClicked;
            gameUIController.RunInfoButtonClicked += HandleRunInfoButtonClicked;
        }

        Debug.Log("Prototype started");
        Debug.Log("Controls: 1-8 select cards, S sort by suit, T sort by rank, P play selected cards, D discard selected cards, Shop: 1-3 buy, R reroll, N leave, F1-F4 equip suit retrigger Jokers, F5 equip high risk Joker, F6 equip stored discard Joker");
        StartCurrentBlind();
    }

    private void EnsureGameUIController()
    {
        if (gameUIController == null)
        {
            gameUIController = FindFirstObjectByType<GameUIController>();
        }

        if (gameUIController == null)
        {
            gameUIController = gameObject.GetComponent<GameUIController>();
        }

        if (gameUIController == null)
        {
            gameUIController = gameObject.AddComponent<GameUIController>();
        }

        gameUIController.InitializeRuntimeBindings();
    }

    private void EnsureCardModifierDebugTester()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        CardModifierDebugTester debugTester = GetComponent<CardModifierDebugTester>();

        if (debugTester == null)
        {
            debugTester = gameObject.AddComponent<CardModifierDebugTester>();
        }

        debugTester.Initialize(this);
#endif
    }

    private void OnDestroy()
    {
        if (gameUIController != null)
        {
            gameUIController.HandCardClicked -= HandleHandCardClicked;
            gameUIController.PlayButtonClicked -= HandlePlayButtonClicked;
            gameUIController.DiscardButtonClicked -= HandleDiscardButtonClicked;
            gameUIController.SortBySuitButtonClicked -= HandleSortBySuitButtonClicked;
            gameUIController.SortByRankButtonClicked -= HandleSortByRankButtonClicked;
            gameUIController.CashOutButtonClicked -= HandleCashOutButtonClicked;
            gameUIController.ShopJokerOfferClicked -= HandleShopJokerOfferClicked;
            gameUIController.ShopConsumableOfferClicked -= HandleShopConsumableOfferClicked;
            gameUIController.ShopRerollButtonClicked -= HandleShopRerollButtonClicked;
            gameUIController.ShopNextBlindButtonClicked -= HandleShopNextBlindButtonClicked;
            gameUIController.RunInfoButtonClicked -= HandleRunInfoButtonClicked;
        }
    }

    private void Update()
    {
        GameUIState currentState = GetCurrentUIState();

        if (currentState == GameUIState.RunFailed)
        {
            HandleRunFailedBlockedInput();
            return;
        }

        if (currentState == GameUIState.Shop || isInShop)
        {
            if (CanUseShop())
            {
                HandleShopInput();
            }
            else
            {
                HandleBlockedShopInput();
            }

            return;
        }

        if (currentState == GameUIState.CashOut)
        {
            HandleBlockedCashOutInput();
            return;
        }

        if (IsRoundOver())
        {
            return;
        }

        if (!CanAcceptGameplayInput())
        {
            HandleBlockedGameplayInput();
            return;
        }

        HandleCardSelectionInput();
        HandleHandSortInput();
        HandleJokerDebugInput();
        HandleRoundInput();
    }

    private void StartCurrentBlind()
    {
        if (hasPreparedDeckForNextBlind)
        {
            hasPreparedDeckForNextBlind = false;
            Debug.Log("Using pre-refreshed deck and hand for this Blind.");
        }
        else
        {
            BuildFreshDeckAndHand();
        }

        roundManager = new RoundManager(runManager.GetCurrentTargetScore());
        isInShop = false;
        latestHandTypeText = "None";
        latestHandTypeRankText = "-";
        latestPlayedCards.Clear();
        latestScoreContext = null;
        ResetCashOutForNewBlind();
        ClearSelectedCards();
        gameUIController?.SetState(GameUIState.PlayingBlind);
        RefreshGameUI();

        Debug.Log($"Starting {runManager.GetDebugStatus()}");
        LogCurrentState();
    }

    private void BuildFreshDeckAndHand()
    {
        deckManager = new DeckManager();
        deckManager.CreateStandardDeck();
        deckManager.Shuffle();

        handManager = new HandManager();
        handManager.FillHand(deckManager);
    }

    private void PrepareDeckAndHandForNextBlindPreview()
    {
        BuildFreshDeckAndHand();
        hasPreparedDeckForNextBlind = true;
        ClearSelectedCards();
        gameUIController?.SetDeckStatsSources(deckManager, handManager);
        Debug.Log("Prepared fresh deck and hand for next Blind preview.");
        Debug.Log($"Preview deck count: {deckManager.DrawPileCount} | Preview hand count: {handManager.CurrentHandCount}");
    }

    private void StartNextBlind()
    {
        runManager.AdvanceToNextBlind();
        Debug.Log($"Advancing to Blind {runManager.CurrentBlindNumber}");
        StartCurrentBlind();
    }

    private void EnterShop()
    {
        isInShop = true;
        Debug.Log("Entering Shop state.");
        shopManager.GenerateOffers();
        gameUIController?.SetState(GameUIState.Shop);
        gameUIController?.ShowShop(shopManager.CurrentOffers, shopManager.CurrentConsumableOffers);

        Debug.Log("=== Shop ===");
        Debug.Log("Shop UI updated.");
        LogShopState();
    }

    private void HandleShopInput()
    {
        for (int i = 0; i < shopManager.ShopOptions.Count; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);

            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                TryBuyShopOption(i);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryRerollShop();
            return;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            LeaveShopAndStartNextBlind();
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Ignored key P: current state is Shop.");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Ignored key D: current state is Shop.");
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Cannot sort: current state is Shop.");
        }
    }

    private void LogShopState()
    {
        Debug.Log("Shop controls: 1-4 buy a Joker, R reroll shop (Cost: 2 Gold), N leave shop");
        Debug.Log($"Gold: {currentGold}");
        Debug.Log($"Joker Slots: {jokerManager.EquippedJokers.Count}/{jokerManager.MaxJokerSlots}");
        Debug.Log($"Shop Options:\n{shopManager.GetShopDebugText()}");
        Debug.Log($"Current Jokers:\n{jokerManager.GetJokerListDebugText()}");
    }

    private void TryBuyShopOption(int optionIndex)
    {
        if (!CanUseShop())
        {
            Debug.Log(GetShopBlockedMessage("purchase"));
            return;
        }

        if (shopManager.TryPurchaseOffer(optionIndex, currentGold, jokerManager, out string message, out int newGold))
        {
            currentGold = newGold;
            RefreshJokerBarUI();
            RefreshGameUI();
            gameUIController?.RefreshShopOffers(shopManager.CurrentOffers);
            gameUIController?.RefreshShopConsumableOffers(shopManager.CurrentConsumableOffers);
        }

        Debug.Log(message);
        Debug.Log("Shop UI updated.");
        LogShopState();
    }

    private void TryRerollShop()
    {
        if (!CanUseShop())
        {
            Debug.Log(GetShopBlockedMessage("reroll"));
            return;
        }

        if (shopManager.TryReroll(currentGold, out string message, out int newGold))
        {
            currentGold = newGold;
            RefreshGameUI();
            gameUIController?.RefreshShopOffers(shopManager.CurrentOffers);
            gameUIController?.RefreshShopConsumableOffers(shopManager.CurrentConsumableOffers);
        }

        Debug.Log(message);
        Debug.Log("Shop UI updated.");
        LogShopState();
    }

    private void HandleRoundInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TryPlaySelectedCards();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            TryDiscardSelectedCards();
        }
    }

    private void HandlePlayButtonClicked()
    {
        if (!CanAcceptGameplayInput())
        {
            Debug.Log(GetGameplayBlockedMessage("play"));
            return;
        }

        if (isInShop)
        {
            Debug.Log("Play failed: currently in shop.");
            return;
        }

        if (IsRoundOver())
        {
            Debug.Log("Play failed: round is already over.");
            return;
        }

        TryPlaySelectedCards();
    }

    private void HandleDiscardButtonClicked()
    {
        if (!CanAcceptGameplayInput())
        {
            Debug.Log(GetGameplayBlockedMessage("discard"));
            return;
        }

        if (isInShop)
        {
            Debug.Log("Discard failed: currently in shop.");
            return;
        }

        if (IsRoundOver())
        {
            Debug.Log("Discard failed: round is already over.");
            return;
        }

        TryDiscardSelectedCards();
    }

    private void HandleCashOutButtonClicked()
    {
        if (hasClaimedCashOut)
        {
            Debug.Log("CashOut already claimed; ignoring duplicate click.");
            return;
        }

        if (!CanUseCashOut())
        {
            Debug.Log(GetCashOutBlockedMessage());
            return;
        }

        currentGold += lastCashOutTotal;
        hasClaimedCashOut = true;
        gameUIController?.SetCashOutButtonInteractable(false);
        Debug.Log($"CashOut claimed: +{lastCashOutTotal} gold. Current gold: {currentGold}");
        EnterShop();
        RefreshGameUI();
        Debug.Log("Entered Shop state.");
    }

    private void HandleShopJokerOfferClicked(int offerIndex)
    {
        TryBuyShopOption(offerIndex);
    }

    private void HandleShopConsumableOfferClicked(int offerIndex)
    {
        TryBuyPlanetOffer(offerIndex);
    }

    private void HandleShopRerollButtonClicked()
    {
        TryRerollShop();
    }

    private void HandleShopNextBlindButtonClicked()
    {
        LeaveShopAndStartNextBlind();
    }

    private void HandleRunInfoButtonClicked()
    {
        RefreshRunInfoSources();
    }

    private void TryBuyPlanetOffer(int offerIndex)
    {
        if (!CanUseShop())
        {
            Debug.Log(GetShopBlockedMessage("purchase"));
            return;
        }

        if (shopManager.TryPurchasePlanetOffer(offerIndex, currentGold, handTypeLevelManager, out string message, out int newGold))
        {
            currentGold = newGold;
            Debug.Log(message);
            RefreshHandTypePreview();
            RefreshGameUI();
            gameUIController?.RefreshShopConsumableOffers(shopManager.CurrentConsumableOffers);
        }
        else
        {
            Debug.Log(message);
        }

        Debug.Log("Shop UI updated.");
        LogShopState();
    }

    private void LeaveShopAndStartNextBlind()
    {
        if (!CanUseShop())
        {
            Debug.Log(GetShopBlockedMessage("go to next blind"));
            return;
        }

        Debug.Log("Leaving Shop state.");
        StartNextBlind();
        Debug.Log($"Started next blind: {runManager.GetBlindDisplayName()}, Ante {runManager.GetAnteNumber()}");
    }

    private void HandleCardSelectionInput()
    {
        for (int i = 0; i < handManager.CurrentHandCount; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);

            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                ToggleSelectedCard(handManager.CurrentHand[i]);
                RefreshHandTypePreview();
                RefreshGameUI();
                Debug.Log($"Toggled card {i + 1}");
                LogCurrentState();
            }
        }
    }

    private void HandleHandCardClicked(PlayingCard card)
    {
        if (!CanAcceptGameplayInput())
        {
            Debug.Log(GetGameplayBlockedMessage("select card"));
            return;
        }

        if (isInShop || IsRoundOver())
        {
            return;
        }

        ToggleSelectedCard(card);
        RefreshHandTypePreview();
        RefreshGameUI();
        LogCurrentState();
    }

    private void ToggleSelectedCard(PlayingCard card)
    {
        if (card == null || !IsCardInCurrentHand(card))
        {
            Debug.Log("Selection failed: card is not in the current hand.");
            return;
        }

        PruneSelectedCards();

        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            card.isSelected = false;
            Debug.Log($"Deselected card: {card.GetDisplayName()}");
            return;
        }

        if (selectedCards.Count >= MaxSelectedCards)
        {
            Debug.Log($"Selection failed: cannot select more than {MaxSelectedCards} cards.");
            return;
        }

        selectedCards.Add(card);
        card.isSelected = true;
        Debug.Log($"Selected card: {card.GetDisplayName()}");
    }

    private void HandleHandSortInput()
    {
        if (!IsShiftHeld() && Input.GetKeyDown(KeyCode.S))
        {
            SortHandBySuitAndRefresh();
        }

        if (!IsShiftHeld() && Input.GetKeyDown(KeyCode.T))
        {
            SortHandByRankAndRefresh();
        }
    }

    private bool IsShiftHeld()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void HandleSortBySuitButtonClicked()
    {
        if (!CanAcceptGameplayInput())
        {
            Debug.Log(GetGameplayBlockedMessage("sort"));
            return;
        }

        if (isInShop || IsRoundOver())
        {
            return;
        }

        SortHandBySuitAndRefresh();
    }

    private void HandleSortByRankButtonClicked()
    {
        if (!CanAcceptGameplayInput())
        {
            Debug.Log(GetGameplayBlockedMessage("sort"));
            return;
        }

        if (isInShop || IsRoundOver())
        {
            return;
        }

        SortHandByRankAndRefresh();
    }

    private void SortHandBySuitAndRefresh()
    {
        handManager.SortHandBySuit();
        RefreshHandTypePreview();
        RefreshGameUI();
        Debug.Log("Sorted current hand by suit.");
        Debug.Log($"Current hand:\n{handManager.GetHandDebugText()}");
    }

    private void SortHandByRankAndRefresh()
    {
        handManager.SortHandByRank();
        RefreshHandTypePreview();
        RefreshGameUI();
        Debug.Log("Sorted current hand by rank.");
        Debug.Log($"Current hand:\n{handManager.GetHandDebugText()}");
    }

    private void HandleJokerDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TryEquipDebugJoker(new SuitRetriggerJoker(Suit.Hearts));
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            TryEquipDebugJoker(new SuitRetriggerJoker(Suit.Spades));
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            TryEquipDebugJoker(new SuitRetriggerJoker(Suit.Diamonds));
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            TryEquipDebugJoker(new SuitRetriggerJoker(Suit.Clubs));
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            TryEquipDebugJoker(new HighRiskMultiplierJoker());
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            TryEquipDebugJoker(new StoredDiscardMultiplierJoker());
        }
    }

    private void TryEquipDebugJoker(JokerBase joker)
    {
        if (jokerManager.TryEquipJoker(joker))
        {
            Debug.Log($"Debug equipped Joker: {joker.Name}");
            RefreshJokerBarUI();
        }
        else
        {
            Debug.Log($"Could not equip Joker: {joker.Name}");
            RefreshJokerBarUI();
        }

        LogCurrentState();
    }

    private void TryPlaySelectedCards()
    {
        List<PlayingCard> cardsToPlay = GetSelectedCardsForAction();
        Debug.Log($"Selected count: {cardsToPlay.Count}");
        Debug.Log($"Selected cards:\n{GetCardListDebugText(cardsToPlay)}");
        Debug.Log($"Play selected cards:\n{GetCardListDebugText(cardsToPlay)}");

        if (cardsToPlay.Count < 1 || cardsToPlay.Count > MaxSelectedCards)
        {
            Debug.Log(cardsToPlay.Count < 1 ? "Cannot play: no selected cards." : "Play failed: select 1 to 5 cards.");
            return;
        }

        if (!TryGetCurrentHandIndices(cardsToPlay, out List<int> handIndices))
        {
            return;
        }

        TryPlayCardsByIndices(handIndices);
    }

    private bool TryPlayCardsByIndices(List<int> handIndices)
    {
        if (handIndices == null || handIndices.Count < 1)
        {
            Debug.Log("Cannot play: no selected cards.");
            return false;
        }

        if (roundManager == null)
        {
            Debug.Log("Cannot play: RoundManager is not initialized.");
            return false;
        }

        if (roundManager.HasPassedBlind)
        {
            Debug.Log("Cannot play: current Blind is already passed.");
            return false;
        }

        if (roundManager.HasFailedBlind)
        {
            Debug.Log("Cannot play: current Blind is already failed.");
            return false;
        }

        if (roundManager.handsRemaining <= 0)
        {
            Debug.Log("Cannot play: no hands remaining.");
            return false;
        }

        if (isResolvingPlayedHand)
        {
            Debug.Log("Cannot play: hand scoring is already resolving.");
            return false;
        }

        if (!TryGetCardsAtCurrentHandIndices(handIndices, out List<PlayingCard> cardsToPlay))
        {
            return false;
        }

        SetCurrentHandSelectionByIndices(handIndices);

        PokerHandResult pokerHandResult = pokerHandEvaluator.Evaluate(cardsToPlay);
        ScoreContext scoreContext = scoreManager.CalculateScore(pokerHandResult, suitMasteryManager, jokerManager, handTypeLevelManager);
        latestHandTypeText = scoreContext.handType.ToString();
        latestHandTypeRankText = GetHandTypeRankText(scoreContext.handType);
        StartCoroutine(PlayScoringSequence(cardsToPlay, scoreContext));
        return true;
    }

    private IEnumerator PlayScoringSequence(List<PlayingCard> cardsToPlay, ScoreContext scoreContext)
    {
        isResolvingPlayedHand = true;
        gameUIController?.SetGameplayInputLocked(true);
        gameUIController?.RefreshHandWithTemporarilyHiddenCards(handManager?.CurrentHand, cardsToPlay);

        List<PlayingCard> displayedCards = gameUIController != null
            ? gameUIController.ShowPendingPlayedCards(cardsToPlay)
            : new List<PlayingCard>(cardsToPlay);

        latestPlayedCards = displayedCards;
        latestScoreContext = null;
        gameUIController?.RefreshScoreCalculation(scoreContext.baseChips, scoreContext.mult);

        int displayedChips = scoreContext.baseChips;

        for (int i = 0; i < displayedCards.Count; i++)
        {
            PlayingCard card = displayedCards[i];
            List<CardScoreEvent> scoreEvents = GetScoreEventsForDisplayedCard(scoreContext, card);

            if (scoreEvents.Count == 0)
            {
                Debug.Log($"PlayedCard{i + 1} skipped non-scoring card: {card.GetDisplayName()}; PlayedCardEffect{i + 1} stays hidden.");
                continue;
            }

            for (int eventIndex = 0; eventIndex < scoreEvents.Count; eventIndex++)
            {
                CardScoreEvent scoreEvent = scoreEvents[eventIndex];
                displayedChips += scoreEvent.chipValue;
                gameUIController?.PlayCardChipEffect(i, card, scoreEvent.chipValue);
                gameUIController?.RefreshScoreCalculation(displayedChips, scoreContext.mult);

                if (scoreEvent.isRetrigger)
                {
                    Debug.Log($"PlayedCard{i + 1} Red Seal retrigger animation: {card.GetDisplayName()} +{scoreEvent.chipValue}");
                }

                yield return new WaitForSeconds(CardScoreStepDelay);
            }
        }

        gameUIController?.RefreshScoreCalculation(scoreContext.chips, scoreContext.mult);
        yield return new WaitForSeconds(FinalScoreHoldDelay);

        List<PlayingCard> playedCards = handManager.PlaySelectedCardsWithoutRefill(deckManager);

        if (playedCards.Count != cardsToPlay.Count)
        {
            Debug.LogError($"Cannot finish play: expected to play {cardsToPlay.Count} cards, but HandManager played {playedCards.Count}.");
            gameUIController?.SetGameplayInputLocked(false);
            isResolvingPlayedHand = false;
            RefreshGameUI();
            yield break;
        }

        handManager.FillHand(deckManager);
        roundManager.ApplyPlayedHandScore(scoreContext.finalScore);
        suitGoldThisBlind += scoreContext.goldReward;
        bonusCardGoldThisBlind += scoreContext.bonusCardGoldReward;

        if (scoreContext.goldReward > 0)
        {
            Debug.Log($"Suit gold earned this hand: {scoreContext.goldReward}");
            Debug.Log($"Suit gold this blind total: {suitGoldThisBlind}");
            Debug.Log($"Current gold unchanged until CashOut: {currentGold}");
        }

        if (scoreContext.bonusCardGoldReward > 0)
        {
            Debug.Log($"Bonus card gold earned this hand: {scoreContext.bonusCardGoldReward}");
            Debug.Log($"Bonus card gold this blind total: {bonusCardGoldThisBlind}");
            Debug.Log($"Current gold unchanged until CashOut: {currentGold}");
        }

        List<Suit> gainedXpSuits = suitMasteryManager.AddXpForScoringSuits(scoreContext.suitCounts);
        IncrementHandTypePlayCount(scoreContext.handType);
        latestPlayedCards = new List<PlayingCard>();
        latestScoreContext = scoreContext;
        ClearSelectedCards();
        latestHandTypeText = "None";
        latestHandTypeRankText = "-";
        gameUIController?.RefreshScoreCalculation(0, 0f);
        gameUIController?.ClearPlayedCards();
        gameUIController?.ClearPlayedCardEffects();
        gameUIController?.SetGameplayInputLocked(false);
        isResolvingPlayedHand = false;
        RefreshGameUI();

        LogPlayedHandResolution(playedCards, scoreContext, gainedXpSuits);
        LogRoundEndIfNeeded();
    }

    private List<CardScoreEvent> GetScoreEventsForDisplayedCard(ScoreContext scoreContext, PlayingCard card)
    {
        List<CardScoreEvent> matchingEvents = new List<CardScoreEvent>();

        if (scoreContext == null || scoreContext.cardScoreEvents == null || card == null)
        {
            return matchingEvents;
        }

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[i];

            if (scoreEvent != null && scoreEvent.card == card)
            {
                matchingEvents.Add(scoreEvent);
            }
        }

        return matchingEvents;
    }

    private void TryDiscardSelectedCards()
    {
        List<PlayingCard> cardsToDiscard = GetSelectedCardsForAction();
        Debug.Log($"Discard selected cards:\n{GetCardListDebugText(cardsToDiscard)}");

        if (cardsToDiscard.Count < 1)
        {
            Debug.Log("Discard failed: select at least 1 card.");
            return;
        }

        if (roundManager.discardsRemaining <= 0)
        {
            Debug.Log("Discard failed: no discards remaining.");
            return;
        }

        roundManager.UseDiscard();
        List<PlayingCard> discardedCards = handManager.DiscardSelectedCards(deckManager);
        HandleDiscardedCardSealEffects(discardedCards);
        ClearSelectedCards();
        latestHandTypeText = "None";
        latestHandTypeRankText = "-";
        RefreshGameUI();

        Debug.Log($"Discarded {discardedCards.Count} cards");
        LogCurrentState();
    }

    private void LogCurrentState()
    {
        Debug.Log($"Run: {runManager.GetDebugStatus()}");
        Debug.Log($"Round: {roundManager.GetDebugStatus()}");
        Debug.Log($"Gold: {currentGold}");
        Debug.Log($"Jokers:\n{jokerManager.GetJokerListDebugText()}");
        Debug.Log($"Suit Mastery:\n{suitMasteryManager.GetMasteryDebugText()}");
        Debug.Log($"Deck count: {deckManager.DrawPileCount} | Discard pile count: {deckManager.DiscardPileCount}");
        Debug.Log($"Current hand:\n{handManager.GetHandDebugText()}");
    }

    private void LogPlayedHandResolution(List<PlayingCard> playedCards, ScoreContext scoreContext, List<Suit> gainedXpSuits)
    {
        Debug.Log("=== Played Hand Resolution ===");
        Debug.Log($"Selected Cards:\n{GetCardListDebugText(playedCards)}");
        Debug.Log($"Hand Type: {scoreContext.handType}");
        Debug.Log($"Score Breakdown:\nBase Chips: {scoreContext.baseChips}\nRank Chips: {scoreContext.rankChips}\nTotal Chips: {scoreContext.chips}\nMult: {scoreContext.mult}\nFinal Score: {scoreContext.finalScore}\nGold Reward: {scoreContext.goldReward}\nBonus Card Gold Reward: {scoreContext.bonusCardGoldReward}\nCurrent Gold: {currentGold}");
        Debug.Log($"Card Chips:\n{scoreContext.GetCardChipDebugText()}");
        Debug.Log($"Card Effects:\n{scoreContext.GetCardEffectDebugText()}");
        Debug.Log($"Scoring Suit Presence:\n{scoreContext.GetSuitPresenceDebugText()}");
        Debug.Log($"Suit Effects:\n{scoreContext.GetSuitEffectDebugText()}");
        Debug.Log($"Joker Effects:\n{scoreContext.GetJokerEffectDebugText()}");
        Debug.Log($"Suit Mastery XP Gained:\n{suitMasteryManager.GetXpGainDebugText(gainedXpSuits)}");
        Debug.Log($"Suit Mastery Status:\n{suitMasteryManager.GetMasteryDebugText()}");
        Debug.Log($"Round Status: {roundManager.GetDebugStatus()}");
        Debug.Log($"Next Hand:\n{handManager.GetHandDebugText()}");
    }

    private void HandleDiscardedCardSealEffects(List<PlayingCard> discardedCards)
    {
        if (discardedCards == null)
        {
            return;
        }

        for (int i = 0; i < discardedCards.Count; i++)
        {
            PlayingCard card = discardedCards[i];

            if (card == null || card.seal != CardSeal.Purple)
            {
                continue;
            }

            Debug.Log($"{card.GetDisplayName()}: Purple Seal would create a tarot card on discard");
        }
    }

    private void ProcessHeldCardEffectsBeforeCashOut()
    {
        if (heldCardEffectsProcessedThisBlind)
        {
            Debug.Log("Held card end-of-blind effects already processed for this Blind.");
            return;
        }

        heldCardEffectsProcessedThisBlind = true;

        if (handManager == null)
        {
            Debug.Log("Cannot process held card effects: HandManager is null.");
            return;
        }

        string lastPlayedHandType = GetLastPlayedHandTypeText();

        for (int i = 0; i < handManager.CurrentHandCount; i++)
        {
            PlayingCard card = handManager.CurrentHand[i];

            if (card == null)
            {
                continue;
            }

            if (card.enhancement == CardEnhancement.Gold)
            {
                bonusCardGoldThisBlind += 3;
                Debug.Log($"{card.GetDisplayName()}: Gold Card held at end of Blind +$3");
                Debug.Log($"Bonus card gold this blind total: {bonusCardGoldThisBlind}");
            }

            if (card.seal == CardSeal.Blue)
            {
                Debug.Log($"{card.GetDisplayName()}: Blue Seal would create planet card for last played hand type: {lastPlayedHandType}");
            }
        }
    }

    private string GetLastPlayedHandTypeText()
    {
        if (latestScoreContext == null)
        {
            return "unknown";
        }

        return latestScoreContext.handType.ToString();
    }

    private string GetCardListDebugText(List<PlayingCard> cards)
    {
        if (cards.Count == 0)
        {
            return "No cards";
        }

        List<string> cardNames = new List<string>();

        for (int i = 0; i < cards.Count; i++)
        {
            cardNames.Add(cards[i].GetDisplayName());
        }

        return string.Join("\n", cardNames);
    }

    private void RefreshGameUI()
    {
        if (gameUIController == null)
        {
            return;
        }

        PruneSelectedCards();
        SyncSelectedCardFlags();
        Debug.Log($"RefreshHandUI: hand count = {GetCurrentHandCountForLog()}, selected count = {selectedCards.Count}");
        if (!isResolvingPlayedHand && selectedCards.Count == 0)
        {
            gameUIController.RefreshScoreCalculation(0, 0f);
        }

        gameUIController.SetDeckStatsSources(deckManager, handManager);
        RefreshRunInfoSources();
        RefreshJokerBarUI();
        gameUIController.RefreshHand(handManager?.CurrentHand);
        gameUIController.RefreshPlayedCards(latestPlayedCards);
        gameUIController.RefreshResolutionInfo(latestScoreContext);

        if (runManager == null || roundManager == null)
        {
            return;
        }

        gameUIController.RefreshBlindStatus(
            runManager.GetBlindDisplayName(),
            roundManager.targetScore,
            roundManager.currentScore,
            latestHandTypeText,
            latestHandTypeRankText,
            roundManager.handsRemaining,
            roundManager.discardsRemaining,
            currentGold,
            runManager.GetAnteNumber());
    }

    private List<PlayingCard> GetSelectedCardsForAction()
    {
        PruneSelectedCards();
        SyncSelectedCardFlags();
        return new List<PlayingCard>(selectedCards);
    }

    private bool TryGetCurrentHandIndices(List<PlayingCard> cards, out List<int> handIndices)
    {
        handIndices = new List<int>();

        for (int i = 0; i < cards.Count; i++)
        {
            PlayingCard selectedCard = cards[i];
            int handIndex = GetCurrentHandIndex(selectedCard);

            if (handIndex < 0)
            {
                Debug.LogError($"Cannot play: selected card was not found in current hand: {selectedCard}");
                return false;
            }

            handIndices.Add(handIndex);
        }

        Debug.Log($"Selected hand indices: {string.Join(", ", handIndices)}");
        return true;
    }

    private bool TryGetCardsAtCurrentHandIndices(List<int> handIndices, out List<PlayingCard> cards)
    {
        cards = new List<PlayingCard>();

        if (handManager == null)
        {
            Debug.Log("Cannot play: HandManager is not initialized.");
            return false;
        }

        for (int i = 0; i < handIndices.Count; i++)
        {
            int handIndex = handIndices[i];

            if (handIndex < 0 || handIndex >= handManager.CurrentHandCount)
            {
                Debug.LogError($"Cannot play: hand index {handIndex} is outside current hand range.");
                return false;
            }

            cards.Add(handManager.CurrentHand[handIndex]);
        }

        return true;
    }

    private void SetCurrentHandSelectionByIndices(List<int> handIndices)
    {
        if (handManager == null)
        {
            return;
        }

        for (int i = 0; i < handManager.CurrentHandCount; i++)
        {
            handManager.CurrentHand[i].isSelected = handIndices.Contains(i);
        }
    }

    private void ClearSelectedCards()
    {
        for (int i = 0; i < selectedCards.Count; i++)
        {
            if (selectedCards[i] != null)
            {
                selectedCards[i].isSelected = false;
            }
        }

        selectedCards.Clear();
        SyncSelectedCardFlags();
    }

    private void PruneSelectedCards()
    {
        for (int i = selectedCards.Count - 1; i >= 0; i--)
        {
            if (selectedCards[i] == null || !IsCardInCurrentHand(selectedCards[i]))
            {
                selectedCards.RemoveAt(i);
            }
        }
    }

    private void SyncSelectedCardFlags()
    {
        if (handManager == null)
        {
            return;
        }

        for (int i = 0; i < handManager.CurrentHandCount; i++)
        {
            PlayingCard card = handManager.CurrentHand[i];
            card.isSelected = selectedCards.Contains(card);
        }
    }

    private bool IsCardInCurrentHand(PlayingCard card)
    {
        if (handManager == null || card == null)
        {
            return false;
        }

        for (int i = 0; i < handManager.CurrentHandCount; i++)
        {
            if (handManager.CurrentHand[i] == card)
            {
                return true;
            }
        }

        return false;
    }

    private int GetCurrentHandIndex(PlayingCard card)
    {
        if (handManager == null || card == null)
        {
            return -1;
        }

        for (int i = 0; i < handManager.CurrentHandCount; i++)
        {
            if (handManager.CurrentHand[i] == card)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetCurrentHandCountForLog()
    {
        return handManager != null ? handManager.CurrentHandCount : 0;
    }

    private bool CanAcceptGameplayInput()
    {
        return gameUIController == null || gameUIController.CanAcceptGameplayInput;
    }

    private bool CanUseCashOut()
    {
        return gameUIController != null && gameUIController.CanUseCashOut;
    }

    private bool CanUseShop()
    {
        return gameUIController != null && gameUIController.CanUseShop && isInShop;
    }

    private GameUIState GetCurrentUIState()
    {
        return gameUIController != null ? gameUIController.CurrentState : GameUIState.PlayingBlind;
    }

    private void HandleBlockedGameplayInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(GetGameplayBlockedMessage("play"));
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log(GetGameplayBlockedMessage("discard"));
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(GetGameplayBlockedMessage("sort"));
        }

        for (int i = 0; i < 8; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);

            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                Debug.Log(GetGameplayBlockedMessage("select card"));
                return;
            }
        }
    }

    private void HandleBlockedCashOutInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Ignored key P: current state is CashOut.");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Ignored key D: current state is CashOut.");
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Cannot sort: current state is CashOut.");
        }

        for (int i = 0; i < 8; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);

            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                Debug.Log("Cannot select card: current state is CashOut.");
                return;
            }
        }
    }

    private void HandleBlockedShopInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log(GetShopBlockedMessage("reroll"));
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log(GetShopBlockedMessage("go to next blind"));
        }

        for (int i = 0; i < shopManager.ShopOptions.Count; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);

            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                Debug.Log(GetShopBlockedMessage("purchase"));
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Ignored key P: current state is Shop.");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Ignored key D: current state is Shop.");
        }
    }

    private void HandleRunFailedBlockedInput()
    {
        if (Input.anyKeyDown)
        {
            Debug.Log("Run failed. Input ignored.");
        }
    }

    private string GetGameplayBlockedMessage(string action)
    {
        if (gameUIController != null && gameUIController.IsDeckStatsOpen)
        {
            return $"Cannot {action}: deck view is open.";
        }

        if (isResolvingPlayedHand)
        {
            return $"Cannot {action}: hand scoring is resolving.";
        }

        return $"Cannot {action}: current state is {GetCurrentUIState()}.";
    }

    private string GetCashOutBlockedMessage()
    {
        if (gameUIController != null && gameUIController.IsDeckStatsOpen)
        {
            return "Cannot claim CashOut: deck view is open.";
        }

        return $"Cannot claim CashOut: current state is {GetCurrentUIState()}.";
    }

    private string GetShopBlockedMessage(string action)
    {
        if (gameUIController != null && gameUIController.IsDeckStatsOpen)
        {
            return $"Cannot {action}: deck view is open.";
        }

        return $"Cannot {action}: current state is {GetCurrentUIState()}.";
    }

    private void RefreshJokerBarUI()
    {
        if (gameUIController == null)
        {
            return;
        }

        if (jokerManager == null)
        {
            Debug.LogError("Failed to bind JokerManager");
            Debug.LogError("Cannot refresh Joker bar: JokerManager is null");
            return;
        }

        gameUIController.RefreshJokerBar(jokerManager.EquippedJokers);
    }

    private void ResetCashOutForNewBlind()
    {
        suitGoldThisBlind = 0;
        bonusCardGoldThisBlind = 0;
        lastCashOutTotal = 0;
        hasClaimedCashOut = false;
        heldCardEffectsProcessedThisBlind = false;
        Debug.Log("CashOut reset for new blind.");
        Debug.Log($"suitGoldThisBlind = {suitGoldThisBlind}");
        Debug.Log($"bonusCardGoldThisBlind = {bonusCardGoldThisBlind}");
        Debug.Log($"hasClaimedCashOut = {hasClaimedCashOut}");
        Debug.Log($"heldCardEffectsProcessedThisBlind = {heldCardEffectsProcessedThisBlind}");
    }

    private void OpenCashOutPanel()
    {
        int fixedBlindReward = runManager.GetFixedBlindReward();
        int interest = Mathf.Min(currentGold / 5, 5);
        int discardBonus = roundManager.discardsRemaining;
        lastCashOutTotal = fixedBlindReward + suitGoldThisBlind + bonusCardGoldThisBlind + interest + discardBonus;

        Debug.Log("Blind passed. Opening CashOutPanel.");
        Debug.Log($"Fixed blind reward: {fixedBlindReward}");
        Debug.Log($"Suit gold this blind: {suitGoldThisBlind}");
        Debug.Log($"Bonus card gold this blind: {bonusCardGoldThisBlind}");
        Debug.Log($"Interest: {interest}");
        Debug.Log($"Discard bonus: {discardBonus}");
        Debug.Log($"CashOut total: {lastCashOutTotal}");

        gameUIController?.SetState(GameUIState.CashOut);
        gameUIController?.ShowCashOut(
            roundManager.targetScore,
            roundManager.currentScore,
            fixedBlindReward,
            suitGoldThisBlind,
            bonusCardGoldThisBlind,
            interest,
            discardBonus,
            lastCashOutTotal);
    }

    private void RefreshHandTypePreview()
    {
        List<PlayingCard> cardsToPreview = GetSelectedCardsForAction();

        if (cardsToPreview.Count == 0)
        {
            latestHandTypeText = "None";
            latestHandTypeRankText = "-";
            gameUIController?.RefreshScoreCalculation(0, 0f);
        }
        else if (pokerHandEvaluator == null)
        {
            latestHandTypeText = "None";
            latestHandTypeRankText = "-";
            gameUIController?.RefreshScoreCalculation(0, 0f);
        }
        else
        {
            PokerHandResult previewResult = pokerHandEvaluator.Evaluate(cardsToPreview);
            latestHandTypeText = previewResult.handType.ToString();
            latestHandTypeRankText = GetHandTypeRankText(previewResult.handType);
            gameUIController?.RefreshScoreCalculation(
                handTypeLevelManager.GetCurrentBaseChips(previewResult.handType),
                handTypeLevelManager.GetCurrentBaseMult(previewResult.handType));
        }

        gameUIController?.RefreshHandTypeText(latestHandTypeText, latestHandTypeRankText);
        Debug.Log($"Hand type preview updated: {latestHandTypeText} {latestHandTypeRankText}");
    }

    private void RefreshRunInfoSources()
    {
        if (gameUIController == null)
        {
            return;
        }

        gameUIController.SetRunInfoSources(handTypeLevelManager, suitMasteryManager, handTypePlayCounts);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public HandManager GetDebugHandManager()
    {
        return handManager;
    }

    public void RefreshDebugCardModifierDisplay()
    {
        if (gameUIController == null)
        {
            return;
        }

        gameUIController.RefreshHand(handManager?.CurrentHand);
        gameUIController.SetDeckStatsSources(deckManager, handManager);
        Debug.Log("Card modifier debug display refreshed.");
    }
#endif

    private string GetHandTypeRankText(PokerHandType handType)
    {
        if (handTypeLevelManager == null)
        {
            return "-";
        }

        return $"Lv {handTypeLevelManager.GetLevel(handType)}";
    }

    private void InitializeHandTypePlayCounts()
    {
        handTypePlayCounts.Clear();

        foreach (PokerHandType handType in Enum.GetValues(typeof(PokerHandType)))
        {
            handTypePlayCounts[handType] = 0;
        }
    }

    private void IncrementHandTypePlayCount(PokerHandType handType)
    {
        if (!handTypePlayCounts.ContainsKey(handType))
        {
            handTypePlayCounts[handType] = 0;
        }

        handTypePlayCounts[handType]++;
        Debug.Log($"Hand type play count updated: {handType} = {handTypePlayCounts[handType]}");
    }

    private void LogRoundEndIfNeeded()
    {
        if (roundManager.HasPassedBlind)
        {
            Debug.Log("Blind passed.");
            ProcessHeldCardEffectsBeforeCashOut();
            jokerManager.NotifyBlindPassed(roundManager);
            Debug.Log($"Jokers after Blind passed:\n{jokerManager.GetJokerListDebugText()}");
            PrepareDeckAndHandForNextBlindPreview();
            OpenCashOutPanel();
        }
        else if (roundManager.HasFailedBlind)
        {
            gameUIController?.SetState(GameUIState.RunFailed);
            Debug.Log("Blind failed.");
        }
    }

    private bool IsRoundOver()
    {
        return roundManager.HasPassedBlind || roundManager.HasFailedBlind;
    }
}
