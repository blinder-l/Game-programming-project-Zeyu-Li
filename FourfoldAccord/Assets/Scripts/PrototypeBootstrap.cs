using System.Collections.Generic;
using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private const int StartingGold = 10;
    private const int MaxSelectedCards = 5;

    [SerializeField] private GameUIController gameUIController;

    private DeckManager deckManager;
    private HandManager handManager;
    private PokerHandEvaluator pokerHandEvaluator;
    private ScoreManager scoreManager;
    private RoundManager roundManager;
    private RunManager runManager;
    private ShopManager shopManager;
    private SuitMasteryManager suitMasteryManager;
    private JokerManager jokerManager;
    private bool isInShop;
    private int currentGold;
    private int suitGoldThisBlind;
    private int lastCashOutTotal;
    private bool hasClaimedCashOut;
    private string latestHandTypeText = "None";
    private List<PlayingCard> latestPlayedCards = new List<PlayingCard>();
    private ScoreContext latestScoreContext;
    private readonly List<PlayingCard> selectedCards = new List<PlayingCard>();

    private void Awake()
    {
        EnsureGameUIController();
    }

    private void Start()
    {
        pokerHandEvaluator = new PokerHandEvaluator();
        scoreManager = new ScoreManager();
        runManager = new RunManager();
        shopManager = new ShopManager();
        suitMasteryManager = new SuitMasteryManager();
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
            gameUIController.ShopRerollButtonClicked += HandleShopRerollButtonClicked;
            gameUIController.ShopNextBlindButtonClicked += HandleShopNextBlindButtonClicked;
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
            gameUIController.ShopRerollButtonClicked -= HandleShopRerollButtonClicked;
            gameUIController.ShopNextBlindButtonClicked -= HandleShopNextBlindButtonClicked;
        }
    }

    private void Update()
    {
        if (isInShop)
        {
            HandleShopInput();
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
        deckManager = new DeckManager();
        deckManager.CreateStandardDeck();
        deckManager.Shuffle();

        handManager = new HandManager();
        handManager.FillHand(deckManager);

        roundManager = new RoundManager(runManager.GetCurrentTargetScore());
        isInShop = false;
        latestHandTypeText = "None";
        latestPlayedCards.Clear();
        latestScoreContext = null;
        ResetCashOutForNewBlind();
        ClearSelectedCards();
        gameUIController?.SetState(GameUIState.PlayingBlind);
        RefreshGameUI();

        Debug.Log($"Starting {runManager.GetDebugStatus()}");
        LogCurrentState();
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
        gameUIController?.ShowShop(shopManager.CurrentOffers);

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
        if (!isInShop)
        {
            Debug.Log($"Cannot purchase: current state is {gameUIController.CurrentState}");
            return;
        }

        if (shopManager.TryPurchaseOffer(optionIndex, currentGold, jokerManager, out string message, out int newGold))
        {
            currentGold = newGold;
            RefreshJokerBarUI();
            RefreshGameUI();
            gameUIController?.RefreshShopOffers(shopManager.CurrentOffers);
        }

        Debug.Log(message);
        Debug.Log("Shop UI updated.");
        LogShopState();
    }

    private void TryRerollShop()
    {
        if (!isInShop)
        {
            return;
        }

        if (shopManager.TryReroll(currentGold, out string message, out int newGold))
        {
            currentGold = newGold;
            RefreshGameUI();
            gameUIController?.RefreshShopOffers(shopManager.CurrentOffers);
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
            Debug.Log("Cannot play: gameplay input is blocked while deck view is open.");
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
            Debug.Log("Cannot discard: gameplay input is blocked while deck view is open.");
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

    private void HandleShopRerollButtonClicked()
    {
        TryRerollShop();
    }

    private void HandleShopNextBlindButtonClicked()
    {
        LeaveShopAndStartNextBlind();
    }

    private void LeaveShopAndStartNextBlind()
    {
        if (!isInShop)
        {
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
            Debug.Log("Cannot select card: gameplay input is blocked while deck view is open.");
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
        if (Input.GetKeyDown(KeyCode.S))
        {
            SortHandBySuitAndRefresh();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SortHandByRankAndRefresh();
        }
    }

    private void HandleSortBySuitButtonClicked()
    {
        if (!CanAcceptGameplayInput())
        {
            Debug.Log("Cannot sort: gameplay input is blocked while deck view is open.");
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
            Debug.Log("Cannot sort: gameplay input is blocked while deck view is open.");
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

        if (!TryGetCardsAtCurrentHandIndices(handIndices, out List<PlayingCard> cardsToPlay))
        {
            return false;
        }

        SetCurrentHandSelectionByIndices(handIndices);

        PokerHandResult pokerHandResult = pokerHandEvaluator.Evaluate(cardsToPlay);
        ScoreContext scoreContext = scoreManager.CalculateScore(pokerHandResult, suitMasteryManager, jokerManager);
        latestHandTypeText = scoreContext.handType.ToString();

        List<PlayingCard> playedCards = handManager.PlaySelectedCards(deckManager);

        if (playedCards.Count != cardsToPlay.Count)
        {
            Debug.LogError($"Cannot play: expected to play {cardsToPlay.Count} cards, but HandManager played {playedCards.Count}.");
            return false;
        }

        roundManager.ApplyPlayedHandScore(scoreContext.finalScore);
        suitGoldThisBlind += scoreContext.goldReward;

        if (scoreContext.goldReward > 0)
        {
            Debug.Log($"Suit gold earned this hand: {scoreContext.goldReward}");
            Debug.Log($"Suit gold this blind total: {suitGoldThisBlind}");
            Debug.Log($"Current gold unchanged until CashOut: {currentGold}");
        }

        List<Suit> gainedXpSuits = suitMasteryManager.AddXpForScoringSuits(scoreContext.suitCounts);
        latestPlayedCards = playedCards;
        latestScoreContext = scoreContext;
        ClearSelectedCards();
        RefreshGameUI();

        LogPlayedHandResolution(playedCards, scoreContext, gainedXpSuits);
        LogRoundEndIfNeeded();
        return true;
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
        ClearSelectedCards();
        latestHandTypeText = "None";
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
        Debug.Log($"Score Breakdown:\nBase Chips: {scoreContext.baseChips}\nRank Chips: {scoreContext.rankChips}\nTotal Chips: {scoreContext.chips}\nMult: {scoreContext.mult}\nFinal Score: {scoreContext.finalScore}\nGold Reward: {scoreContext.goldReward}\nCurrent Gold: {currentGold}");
        Debug.Log($"Card Chips:\n{scoreContext.GetCardChipDebugText()}");
        Debug.Log($"Scoring Suit Presence:\n{scoreContext.GetSuitPresenceDebugText()}");
        Debug.Log($"Suit Effects:\n{scoreContext.GetSuitEffectDebugText()}");
        Debug.Log($"Joker Effects:\n{scoreContext.GetJokerEffectDebugText()}");
        Debug.Log($"Suit Mastery XP Gained:\n{suitMasteryManager.GetXpGainDebugText(gainedXpSuits)}");
        Debug.Log($"Suit Mastery Status:\n{suitMasteryManager.GetMasteryDebugText()}");
        Debug.Log($"Round Status: {roundManager.GetDebugStatus()}");
        Debug.Log($"Next Hand:\n{handManager.GetHandDebugText()}");
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
        gameUIController.SetDeckStatsSources(deckManager, handManager);
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

    private void HandleBlockedGameplayInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Cannot play: gameplay input is blocked while deck view is open.");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Cannot discard: gameplay input is blocked while deck view is open.");
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Cannot sort: gameplay input is blocked while deck view is open.");
        }

        for (int i = 0; i < 8; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);

            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                Debug.Log("Cannot select card: gameplay input is blocked while deck view is open.");
                return;
            }
        }
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
        lastCashOutTotal = 0;
        hasClaimedCashOut = false;
        Debug.Log("CashOut reset for new blind.");
        Debug.Log($"suitGoldThisBlind = {suitGoldThisBlind}");
        Debug.Log($"hasClaimedCashOut = {hasClaimedCashOut}");
    }

    private void OpenCashOutPanel()
    {
        int fixedBlindReward = runManager.GetFixedBlindReward();
        int interest = Mathf.Min(currentGold / 5, 5);
        int discardBonus = roundManager.discardsRemaining;
        lastCashOutTotal = fixedBlindReward + suitGoldThisBlind + interest + discardBonus;

        Debug.Log("Blind passed. Opening CashOutPanel.");
        Debug.Log($"Fixed blind reward: {fixedBlindReward}");
        Debug.Log($"Suit gold this blind: {suitGoldThisBlind}");
        Debug.Log($"Interest: {interest}");
        Debug.Log($"Discard bonus: {discardBonus}");
        Debug.Log($"CashOut total: {lastCashOutTotal}");

        gameUIController?.SetState(GameUIState.CashOut);
        gameUIController?.ShowCashOut(
            roundManager.targetScore,
            roundManager.currentScore,
            fixedBlindReward,
            suitGoldThisBlind,
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
        }
        else if (pokerHandEvaluator == null)
        {
            latestHandTypeText = "None";
        }
        else
        {
            PokerHandResult previewResult = pokerHandEvaluator.Evaluate(cardsToPreview);
            latestHandTypeText = previewResult.handType.ToString();
        }

        gameUIController?.RefreshHandTypeText(latestHandTypeText);
        Debug.Log($"Hand type preview updated: {latestHandTypeText}");
    }

    private void LogRoundEndIfNeeded()
    {
        if (roundManager.HasPassedBlind)
        {
            Debug.Log("Blind passed.");
            jokerManager.NotifyBlindPassed(roundManager);
            Debug.Log($"Jokers after Blind passed:\n{jokerManager.GetJokerListDebugText()}");
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
