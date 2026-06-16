using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private const int StartingGold = 10;
    private const int MaxSelectedCards = 5;
    private const int VictoryAnte = 6;
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
    private BossBlindManager bossBlindManager;
    private EdictManager edictManager;
    private GameAudioController gameAudioController;
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
    private bool hasProcessedFirstDiscardThisBlind;
    private bool hasShownGameResult;
    private string latestHandTypeText = "None";
    private string latestHandTypeRankText = "-";
    private List<PlayingCard> latestPlayedCards = new List<PlayingCard>();
    private ScoreContext latestScoreContext;
    private readonly List<PlayingCard> selectedCards = new List<PlayingCard>();
    private readonly SpellCard[] heldSpellCards = new SpellCard[2];
    private readonly Dictionary<PokerHandType, int> handTypePlayCounts = new Dictionary<PokerHandType, int>();
    private readonly GameResultStats runResultStats = new GameResultStats();

    private void Awake()
    {
        EnsureGameUIController();
        EnsureGameAudioController();
        EnsureCardModifierDebugTester();
    }

    private void Start()
    {
        pokerHandEvaluator = new PokerHandEvaluator();
        scoreManager = new ScoreManager();
        runManager = new RunManager();
        shopManager = new ShopManager();
        bossBlindManager = new BossBlindManager();
        edictManager = new EdictManager();
        suitMasteryManager = new SuitMasteryManager();
        handTypeLevelManager = new HandTypeLevelManager();
        ApplyEdictRuntimeEffects();
        InitializeHandTypePlayCounts();
        jokerManager = new JokerManager();
        currentGold = StartingGold;
        runResultStats.Reset();
        hasShownGameResult = false;

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
            gameUIController.ShopEdictOfferClicked += HandleShopEdictOfferClicked;
            gameUIController.ShopRerollButtonClicked += HandleShopRerollButtonClicked;
            gameUIController.ShopNextBlindButtonClicked += HandleShopNextBlindButtonClicked;
            gameUIController.RunInfoButtonClicked += HandleRunInfoButtonClicked;
            gameUIController.ExitButtonClicked += HandleExitButtonClicked;
            gameUIController.JokerSaleButtonClicked += HandleJokerSaleButtonClicked;
            gameUIController.ConsumableUseButtonClicked += HandleConsumableUseButtonClicked;
        }

        Debug.Log("Prototype started");
        Debug.Log("Controls: 1-8 select cards, S sort by suit, T sort by rank, P play selected cards, D discard selected cards, Shop: 1-4 buy, R reroll, N leave");
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

    private void EnsureGameAudioController()
    {
        if (gameAudioController == null)
        {
            gameAudioController = FindFirstObjectByType<GameAudioController>();
        }

        if (gameAudioController == null)
        {
            gameAudioController = gameObject.GetComponent<GameAudioController>();
        }

        if (gameAudioController == null)
        {
            gameAudioController = gameObject.AddComponent<GameAudioController>();
        }

        gameAudioController.Initialize();
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
            gameUIController.ShopEdictOfferClicked -= HandleShopEdictOfferClicked;
            gameUIController.ShopRerollButtonClicked -= HandleShopRerollButtonClicked;
            gameUIController.ShopNextBlindButtonClicked -= HandleShopNextBlindButtonClicked;
            gameUIController.RunInfoButtonClicked -= HandleRunInfoButtonClicked;
            gameUIController.ExitButtonClicked -= HandleExitButtonClicked;
            gameUIController.JokerSaleButtonClicked -= HandleJokerSaleButtonClicked;
            gameUIController.ConsumableUseButtonClicked -= HandleConsumableUseButtonClicked;
        }
    }

    private void Update()
    {
        if (hasShownGameResult)
        {
            return;
        }

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
        bossBlindManager.StartPreparedBlind(runManager.CurrentBlindNumber, runManager.IsBossBlind());
        int handSizeLimit = bossBlindManager.GetHandSizeLimit(HandManager.DefaultHandSizeLimit);

        if (hasPreparedDeckForNextBlind)
        {
            hasPreparedDeckForNextBlind = false;
            if (handManager != null)
            {
                handManager.HandSizeLimit = handSizeLimit;
            }
            Debug.Log("Using pre-refreshed deck and hand for this Blind.");
        }
        else
        {
            BuildFreshDeckAndHand(handSizeLimit);
        }

        int baseTargetScore = runManager.GetCurrentTargetScore();
        int targetScore = bossBlindManager.GetTargetScore(baseTargetScore);
        int startingHands = bossBlindManager.GetStartingHands(4 + GetAdditionalHandsPerBlind());
        int startingDiscards = bossBlindManager.GetStartingDiscards(3 + GetAdditionalDiscardsPerBlind());
        roundManager = new RoundManager(targetScore, startingHands, startingDiscards);
        isInShop = false;
        latestHandTypeText = "None";
        latestHandTypeRankText = "-";
        latestPlayedCards.Clear();
        latestScoreContext = null;
        ResetCashOutForNewBlind();
        ClearSelectedCards();
        NotifyJokersBlindStarted();
        gameUIController?.SetState(GameUIState.PlayingBlind);
        RefreshGameUI();

        Debug.Log($"Starting {runManager.GetDebugStatus()}");
        LogCurrentState();
    }

    private void BuildFreshDeckAndHand(int handSizeLimit = HandManager.DefaultHandSizeLimit)
    {
        deckManager = new DeckManager();
        deckManager.CreateStandardDeck();
        deckManager.Shuffle();

        handManager = new HandManager();
        handManager.HandSizeLimit = handSizeLimit;
        handManager.FillHand(deckManager);
    }

    private void PrepareDeckAndHandForNextBlindPreview()
    {
        int nextBlindNumber = runManager.CurrentBlindNumber + 1;
        bossBlindManager.PrepareForBlind(nextBlindNumber, runManager.IsBossBlind(nextBlindNumber));
        int handSizeLimit = bossBlindManager.GetHandSizeLimit(HandManager.DefaultHandSizeLimit);

        List<PlayingCard> ownedCards = deckManager != null
            ? deckManager.GetAllOwnedCardsSnapshot(handManager?.CurrentHand)
            : new List<PlayingCard>();

        if (ownedCards.Count == 0)
        {
            BuildFreshDeckAndHand(handSizeLimit);
        }
        else
        {
            deckManager = new DeckManager();
            deckManager.LoadOwnedCardsAsDrawPile(ownedCards);
            deckManager.Shuffle();

            handManager = new HandManager();
            handManager.HandSizeLimit = handSizeLimit;
            handManager.FillHand(deckManager);
        }

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
        ApplyEdictRuntimeEffects();
        shopManager.GenerateOffers(jokerManager, edictManager);
        gameUIController?.SetShopPriceDiscount(GetShopPriceDiscount());
        gameUIController?.SetState(GameUIState.Shop);
        gameUIController?.ShowShop(shopManager.CurrentOffers, shopManager.CurrentConsumableOffers, shopManager.CurrentEdictOffer);

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
            runResultStats.RecordPurchasedCard();
            RefreshJokerBarUI();
            RefreshGameUI();
            gameUIController?.RefreshShopOffers(shopManager.CurrentOffers);
            gameUIController?.RefreshShopConsumableOffers(shopManager.CurrentConsumableOffers);
            gameUIController?.RefreshShopEdictOffer(shopManager.CurrentEdictOffer);
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

        if (shopManager.TryReroll(currentGold, jokerManager, edictManager, out string message, out int newGold))
        {
            currentGold = newGold;
            runResultStats.RecordReroll();
            RefreshGameUI();
            gameUIController?.RefreshShopOffers(shopManager.CurrentOffers);
            gameUIController?.RefreshShopConsumableOffers(shopManager.CurrentConsumableOffers);
            gameUIController?.RefreshShopEdictOffer(shopManager.CurrentEdictOffer);
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
        TryBuyConsumableOffer(offerIndex);
    }

    private void HandleShopEdictOfferClicked()
    {
        TryBuyEdictOffer();
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

    private void HandleExitButtonClicked()
    {
        Debug.Log("Exit requested by player.");
        ShowGameResult(GameResultType.Retreat);
    }

    private void HandleJokerSaleButtonClicked(int slotIndex)
    {
        if (jokerManager == null)
        {
            Debug.LogError("Cannot sell Joker: JokerManager is null");
            return;
        }

        if (!jokerManager.TrySellJokerAt(slotIndex, out JokerBase soldJoker))
        {
            Debug.Log($"Cannot sell Joker: slot {slotIndex + 1} is empty or invalid");
            gameUIController?.HideJokerSaleButtons();
            return;
        }

        int sellPrice = Mathf.Max(1, soldJoker.Cost - 2);
        currentGold += sellPrice;
        Debug.Log($"Sold Joker: {soldJoker.Name} for ${sellPrice}. Current gold: {currentGold}");
        jokerManager.NotifyJokerSold(soldJoker);

        gameUIController?.HideJokerSaleButtons();
        RefreshJokerBarUI();
        RefreshGameUI();

        if (GetCurrentUIState() == GameUIState.Shop)
        {
            gameUIController?.RefreshShopOffers(shopManager.CurrentOffers);
            gameUIController?.RefreshShopConsumableOffers(shopManager.CurrentConsumableOffers);
            Debug.Log("Shop UI updated after Joker sale.");
        }
    }

    private void HandleConsumableUseButtonClicked(int slotIndex)
    {
        if (!CanUseSpellConsumable())
        {
            Debug.Log(GetGameplayBlockedMessage("use consumable"));
            gameUIController?.HideConsumableUseButtons();
            return;
        }

        if (slotIndex < 0 || slotIndex >= heldSpellCards.Length)
        {
            Debug.Log($"Cannot use Spell: invalid slot {slotIndex + 1}");
            return;
        }

        SpellCard spellCard = heldSpellCards[slotIndex];

        if (spellCard == null)
        {
            Debug.Log($"Cannot use Spell: slot {slotIndex + 1} is empty");
            gameUIController?.HideConsumableUseButtons();
            return;
        }

        if (!TryUseSpellCard(spellCard, out string message))
        {
            Debug.Log(message);
            RefreshGameUI();
            return;
        }

        heldSpellCards[slotIndex] = null;
        Debug.Log(message);
        Debug.Log($"Consumed Spell: {spellCard.Name}");
        gameUIController?.HideConsumableUseButtons();
        RefreshHandTypePreview();
        RefreshGameUI();
    }

    private void TryBuyConsumableOffer(int offerIndex)
    {
        if (!CanUseShop())
        {
            Debug.Log(GetShopBlockedMessage("purchase"));
            return;
        }

        ConsumableShopOffer offer = offerIndex >= 0 && offerIndex < shopManager.CurrentConsumableOffers.Count
            ? shopManager.CurrentConsumableOffers[offerIndex]
            : null;

        if (offer == null)
        {
            Debug.Log("Cannot purchase consumable: invalid offer index");
            return;
        }

        if (offer.IsPlanet)
        {
            TryBuyPlanetOffer(offerIndex);
            return;
        }

        if (offer.IsSpell)
        {
            TryBuySpellOffer(offerIndex);
            return;
        }

        Debug.Log("Cannot purchase consumable: offer has no card.");
    }

    private void TryBuyEdictOffer()
    {
        if (!CanUseShop())
        {
            Debug.Log(GetShopBlockedMessage("purchase"));
            return;
        }

        if (shopManager.TryPurchaseEdictOffer(
            currentGold,
            edictManager,
            out EdictCard purchasedEdict,
            out string message,
            out int newGold))
        {
            currentGold = newGold;
            runResultStats.RecordPurchasedCard();
            ApplyEdictRuntimeEffects();
            RefreshHandTypePreview();
            RefreshGameUI();
            gameUIController?.SetShopPriceDiscount(GetShopPriceDiscount());
            gameUIController?.RefreshShopOffers(shopManager.CurrentOffers);
            gameUIController?.RefreshShopConsumableOffers(shopManager.CurrentConsumableOffers);
            gameUIController?.RefreshShopEdictOffer(shopManager.CurrentEdictOffer);
            Debug.Log($"Edict effect active: {purchasedEdict.Name}");
        }

        Debug.Log(message);
        Debug.Log("Shop UI updated.");
        LogShopState();
    }

    private void TryBuyPlanetOffer(int offerIndex)
    {
        if (!CanUseShop())
        {
            Debug.Log(GetShopBlockedMessage("purchase"));
            return;
        }

        PlanetCard purchasedPlanetCard = offerIndex >= 0 && offerIndex < shopManager.CurrentConsumableOffers.Count
            ? shopManager.CurrentConsumableOffers[offerIndex].PlanetCard
            : null;

        if (shopManager.TryPurchasePlanetOffer(offerIndex, currentGold, handTypeLevelManager, out string message, out int newGold))
        {
            currentGold = newGold;
            runResultStats.RecordPurchasedCard();
            Debug.Log(message);
            if (purchasedPlanetCard != null)
            {
                jokerManager?.NotifyPlanetCardUsed(purchasedPlanetCard, purchasedPlanetCard.targetHandType);
            }
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

    private void TryBuySpellOffer(int offerIndex)
    {
        if (!CanUseShop())
        {
            Debug.Log(GetShopBlockedMessage("purchase"));
            return;
        }

        if (shopManager.TryPurchaseSpellOffer(
            offerIndex,
            currentGold,
            HasFreeSpellSlot(),
            out SpellCard purchasedSpellCard,
            out string message,
            out int newGold))
        {
            currentGold = newGold;
            runResultStats.RecordPurchasedCard();

            if (!TryAddHeldSpellCard(purchasedSpellCard))
            {
                Debug.LogError($"Purchased Spell but failed to add to held slots: {purchasedSpellCard?.Name}");
            }

            RefreshGameUI();
            gameUIController?.RefreshShopConsumableOffers(shopManager.CurrentConsumableOffers);
        }

        Debug.Log(message);
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

        ApplyEdictRuntimeEffects();
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                TryEquipDebugJoker(new WaymarkPilgrimJoker());
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                TryEquipDebugJoker(new GildedMaskbearerJoker());
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                TryEquipDebugJoker(new StoneboundEffigierJoker());
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                TryEquipDebugJoker(new FortuneFamiliarJoker());
            }
        }
#endif
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

        JokerRuleContext ruleContext = jokerManager != null ? jokerManager.BuildRuleContext() : null;
        PokerHandResult pokerHandResult = pokerHandEvaluator.Evaluate(cardsToPlay, ruleContext);

        if (!CanPlaySelectedCardsAgainstBoss(cardsToPlay, pokerHandResult))
        {
            return false;
        }

        JokerRuntimeContext playRuntimeContext = BuildJokerRuntimeContext(cardsToPlay, roundManager.handsRemaining == 4);
        jokerManager?.NotifyBeforeScore(playRuntimeContext);
        List<PlayingCard> ownedCardsSnapshot = deckManager != null
            ? deckManager.GetAllOwnedCardsSnapshot(handManager?.CurrentHand)
            : null;
        List<PlayingCard> heldCardsSnapshot = GetHeldCardsSnapshot(cardsToPlay);
        int currentHandTypePlayCount = GetHandTypePlayCount(pokerHandResult.handType) + 1;
        Dictionary<PokerHandType, int> handTypePlayCountsBeforeHand = new Dictionary<PokerHandType, int>(handTypePlayCounts);
        ScoreContext scoreContext = scoreManager.CalculateScore(
            pokerHandResult,
            suitMasteryManager,
            jokerManager,
            handTypeLevelManager,
            ownedCardsSnapshot,
            heldCardsSnapshot,
            ruleContext,
            currentHandTypePlayCount,
            handTypePlayCountsBeforeHand,
            bossBlindManager != null ? bossBlindManager.BuildContext() : null);
        latestHandTypeText = scoreContext.handType.ToString();
        latestHandTypeRankText = GetHandTypeRankText(scoreContext.handType);
        gameAudioController?.PlayCardsSfx();
        StartCoroutine(PlayScoringSequence(cardsToPlay, scoreContext, playRuntimeContext));
        return true;
    }

    private bool CanPlaySelectedCardsAgainstBoss(List<PlayingCard> cardsToPlay, PokerHandResult pokerHandResult)
    {
        if (bossBlindManager == null || !bossBlindManager.IsActive)
        {
            return true;
        }

        if (bossBlindManager.CurrentType == BossBlindType.Psychic && (cardsToPlay == null || cardsToPlay.Count != 5))
        {
            Debug.Log("Boss Blocked Play: The Psychic requires exactly 5 selected cards.");
            return false;
        }

        if (bossBlindManager.CurrentType == BossBlindType.Eye)
        {
            PokerHandType handType = pokerHandResult != null ? pokerHandResult.handType : PokerHandType.HighCard;

            if (!bossBlindManager.TryRecordEyeHandType(handType, out string message))
            {
                Debug.Log(message);
                return false;
            }

            Debug.Log(message);
        }

        return true;
    }

    private IEnumerator PlayScoringSequence(List<PlayingCard> cardsToPlay, ScoreContext scoreContext, JokerRuntimeContext playRuntimeContext)
    {
        isResolvingPlayedHand = true;
        gameUIController?.SetGameplayInputLocked(true);
        gameUIController?.RefreshHandWithTemporarilyHiddenCards(handManager?.CurrentHand, cardsToPlay);

        List<PlayingCard> displayedCards = gameUIController != null
            ? gameUIController.ShowPendingPlayedCards(cardsToPlay)
            : new List<PlayingCard>(cardsToPlay);

        latestPlayedCards = displayedCards;
        AssignCardScoreEventSlots(scoreContext, displayedCards);
        latestScoreContext = null;
        float displayedMult = scoreContext.multBeforeJokers > 0f ? scoreContext.multBeforeJokers : scoreContext.mult;
        gameUIController?.RefreshScoreCalculation(scoreContext.baseChips, displayedMult);

        int displayedChips = scoreContext.baseChips;
        List<CardScoreEvent> orderedScoreEvents = GetOrderedScoreEventsForDisplayedCards(scoreContext, displayedCards);
        HashSet<JokerScoreEvent> playedJokerScoreEvents = new HashSet<JokerScoreEvent>();

        for (int i = 0; i < displayedCards.Count; i++)
        {
            PlayingCard card = displayedCards[i];
            if (!HasScoreEventForSlot(orderedScoreEvents, i))
            {
                Debug.Log($"PlayedCard{i + 1} skipped non-scoring card: {card.GetDisplayName()}; PlayedCardEffect{i + 1} stays hidden.");
            }
        }

        for (int i = 0; i < orderedScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = orderedScoreEvents[i];

            if (scoreEvent == null || scoreEvent.card == null)
            {
                continue;
            }

            displayedChips += scoreEvent.chipsAdded;
            gameUIController?.PlayCardScoreEvent(scoreEvent);
            gameUIController?.RefreshScoreCalculation(displayedChips, displayedMult);

            if (scoreEvent.isRetrigger)
            {
                Debug.Log($"PlayedCard{scoreEvent.playedCardSlotIndex + 1} {scoreEvent.effectSource} retrigger animation: {scoreEvent.card.GetDisplayName()} +{scoreEvent.chipsAdded}");
            }

            yield return new WaitForSeconds(CardScoreStepDelay);

            yield return PlayJokerScoreEventsForCardEvent(
                scoreContext,
                scoreEvent,
                playedJokerScoreEvents,
                jokerScoreEvent => ApplyJokerScoreEventToDisplay(jokerScoreEvent, ref displayedChips, ref displayedMult));
        }

        yield return PlayUnanchoredJokerScoreEvents(
            scoreContext,
            playedJokerScoreEvents,
            jokerScoreEvent => ApplyJokerScoreEventToDisplay(jokerScoreEvent, ref displayedChips, ref displayedMult));

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

        runResultStats.RecordPlayedHand(scoreContext.handType, scoreContext.finalScore, playedCards.Count);
        ApplyPendingRuntimeCardsToHand(playRuntimeContext);
        jokerManager?.NotifyCardsPlayedBeforeRefill(scoreContext, playRuntimeContext);
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
        jokerManager.NotifyHandScored(scoreContext);
        RefreshJokerBarUI();
        LogRoundEndIfNeeded();
    }

    private void AssignCardScoreEventSlots(ScoreContext scoreContext, List<PlayingCard> displayedCards)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null || displayedCards == null)
        {
            return;
        }

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[i];

            if (scoreEvent == null || scoreEvent.card == null)
            {
                continue;
            }

            scoreEvent.playedCardSlotIndex = GetDisplayedCardSlotIndex(displayedCards, scoreEvent.card);
            scoreEvent.chipsAdded = scoreEvent.chipValue;

            if (string.IsNullOrEmpty(scoreEvent.effectText))
            {
                scoreEvent.effectText = $"+{scoreEvent.chipsAdded}";
            }

            if (string.IsNullOrEmpty(scoreEvent.effectSource))
            {
                scoreEvent.effectSource = scoreEvent.isRetrigger ? "Red Seal" : "Card";
            }
        }
    }

    private int GetDisplayedCardSlotIndex(List<PlayingCard> displayedCards, PlayingCard card)
    {
        if (displayedCards == null || card == null)
        {
            return -1;
        }

        for (int i = 0; i < displayedCards.Count; i++)
        {
            if (displayedCards[i] == card)
            {
                return i;
            }
        }

        return -1;
    }

    private List<CardScoreEvent> GetOrderedScoreEventsForDisplayedCards(ScoreContext scoreContext, List<PlayingCard> displayedCards)
    {
        List<CardScoreEvent> orderedEvents = new List<CardScoreEvent>();

        if (scoreContext == null || scoreContext.cardScoreEvents == null || displayedCards == null)
        {
            return orderedEvents;
        }

        for (int slotIndex = 0; slotIndex < displayedCards.Count; slotIndex++)
        {
            for (int eventIndex = 0; eventIndex < scoreContext.cardScoreEvents.Count; eventIndex++)
            {
                CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[eventIndex];

                if (scoreEvent != null && scoreEvent.playedCardSlotIndex == slotIndex)
                {
                    orderedEvents.Add(scoreEvent);
                }
            }
        }

        return orderedEvents;
    }

    private bool HasScoreEventForSlot(List<CardScoreEvent> scoreEvents, int slotIndex)
    {
        if (scoreEvents == null)
        {
            return false;
        }

        for (int i = 0; i < scoreEvents.Count; i++)
        {
            if (scoreEvents[i] != null && scoreEvents[i].playedCardSlotIndex == slotIndex)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator PlayJokerScoreEventsForCardEvent(
        ScoreContext scoreContext,
        CardScoreEvent triggerEvent,
        HashSet<JokerScoreEvent> playedJokerScoreEvents,
        Action<JokerScoreEvent> applyDisplayChange)
    {
        if (scoreContext == null || scoreContext.jokerScoreEvents == null || triggerEvent == null)
        {
            yield break;
        }

        for (int i = 0; i < scoreContext.jokerScoreEvents.Count; i++)
        {
            JokerScoreEvent jokerScoreEvent = scoreContext.jokerScoreEvents[i];

            if (jokerScoreEvent == null ||
                jokerScoreEvent.triggerAfterCardScoreEvent != triggerEvent ||
                playedJokerScoreEvents.Contains(jokerScoreEvent))
            {
                continue;
            }

            playedJokerScoreEvents.Add(jokerScoreEvent);
            gameUIController?.PlayJokerScoreEvent(jokerScoreEvent);
            applyDisplayChange?.Invoke(jokerScoreEvent);
            yield return new WaitForSeconds(CardScoreStepDelay);
        }
    }

    private IEnumerator PlayUnanchoredJokerScoreEvents(
        ScoreContext scoreContext,
        HashSet<JokerScoreEvent> playedJokerScoreEvents,
        Action<JokerScoreEvent> applyDisplayChange)
    {
        if (scoreContext == null || scoreContext.jokerScoreEvents == null)
        {
            yield break;
        }

        for (int i = 0; i < scoreContext.jokerScoreEvents.Count; i++)
        {
            JokerScoreEvent jokerScoreEvent = scoreContext.jokerScoreEvents[i];

            if (jokerScoreEvent == null ||
                jokerScoreEvent.triggerAfterCardScoreEvent != null ||
                playedJokerScoreEvents.Contains(jokerScoreEvent))
            {
                continue;
            }

            playedJokerScoreEvents.Add(jokerScoreEvent);
            gameUIController?.PlayJokerScoreEvent(jokerScoreEvent);
            applyDisplayChange?.Invoke(jokerScoreEvent);
            yield return new WaitForSeconds(CardScoreStepDelay);
        }
    }

    private void ApplyJokerScoreEventToDisplay(JokerScoreEvent jokerScoreEvent, ref int displayedChips, ref float displayedMult)
    {
        if (jokerScoreEvent == null)
        {
            return;
        }

        if (jokerScoreEvent.chipsDelta != 0)
        {
            displayedChips += jokerScoreEvent.chipsDelta;
        }

        if (Math.Abs(jokerScoreEvent.multAdd) > 0.0001f)
        {
            displayedMult += jokerScoreEvent.multAdd;
        }

        if (Math.Abs(jokerScoreEvent.multMultiplier - 1f) > 0.0001f)
        {
            displayedMult *= jokerScoreEvent.multMultiplier;
        }

        gameUIController?.RefreshScoreCalculation(displayedChips, displayedMult);
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

        JokerDiscardContext discardContext = new JokerDiscardContext
        {
            selectedCards = new List<PlayingCard>(cardsToDiscard),
            cardsToDiscard = new List<PlayingCard>(cardsToDiscard),
            destroyedCards = new List<PlayingCard>(),
            isFirstDiscardThisBlind = !hasProcessedFirstDiscardThisBlind,
            ruleContext = jokerManager != null ? jokerManager.BuildRuleContext() : null,
            pokerHandEvaluator = pokerHandEvaluator,
            handTypeLevelManager = handTypeLevelManager,
            addGold = AddGoldFromJoker
        };

        jokerManager?.NotifyDiscardAction(discardContext);
        hasProcessedFirstDiscardThisBlind = true;

        roundManager.UseDiscard();
        List<PlayingCard> destroyedCards = handManager.RemoveCardsWithoutDiscard(discardContext.destroyedCards);

        for (int i = 0; i < destroyedCards.Count; i++)
        {
            deckManager.MarkDestroyed(destroyedCards[i]);
        }

        List<PlayingCard> discardedCards = handManager.DiscardCards(discardContext.cardsToDiscard, deckManager);
        handManager.FillHand(deckManager);
        runResultStats.RecordDiscardedCards(discardedCards.Count);
        HandleDiscardedCardSealEffects(discardedCards);
        ClearSelectedCards();
        latestHandTypeText = "None";
        latestHandTypeRankText = "-";
        RefreshGameUI();

        Debug.Log($"Discarded {discardedCards.Count} cards");
        Debug.Log($"Destroyed {destroyedCards.Count} cards");
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
        gameUIController.SetJokerTooltipContext(GetOwnedStoneCardCount());
        RefreshRunInfoSources();
        RefreshJokerBarUI();
        gameUIController.RefreshConsumableSlots(heldSpellCards);
        SyncBossDebuffFlagsForCurrentHand();
        gameUIController.RefreshHand(handManager?.CurrentHand);
        gameUIController.RefreshPlayedCards(latestPlayedCards);
        gameUIController.RefreshResolutionInfo(latestScoreContext);

        if (runManager == null || roundManager == null)
        {
            return;
        }

        gameUIController.RefreshBossBlindInfo(bossBlindManager != null ? bossBlindManager.CurrentRuleText : string.Empty);
        gameUIController.RefreshBlindStatus(
            GetCurrentBlindDisplayName(),
            roundManager.targetScore,
            roundManager.currentScore,
            latestHandTypeText,
            latestHandTypeRankText,
            roundManager.handsRemaining,
            roundManager.discardsRemaining,
            currentGold,
            runManager.GetAnteNumber());
    }

    private string GetCurrentBlindDisplayName()
    {
        if (bossBlindManager != null && bossBlindManager.IsActive)
        {
            return bossBlindManager.CurrentDisplayName;
        }

        return runManager != null ? runManager.GetBlindDisplayName() : string.Empty;
    }

    private void SyncBossDebuffFlagsForCurrentHand()
    {
        if (handManager == null || handManager.CurrentHand == null)
        {
            return;
        }

        for (int i = 0; i < handManager.CurrentHand.Count; i++)
        {
            PlayingCard card = handManager.CurrentHand[i];

            if (card == null)
            {
                continue;
            }

            card.isDebuffed = bossBlindManager != null && bossBlindManager.IsCardDebuffedByBoss(card);
        }
    }

    private List<PlayingCard> GetSelectedCardsForAction()
    {
        PruneSelectedCards();
        SyncSelectedCardFlags();
        return new List<PlayingCard>(selectedCards);
    }

    private bool HasFreeSpellSlot()
    {
        for (int i = 0; i < heldSpellCards.Length; i++)
        {
            if (heldSpellCards[i] == null)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryAddHeldSpellCard(SpellCard spellCard)
    {
        if (spellCard == null)
        {
            return false;
        }

        for (int i = 0; i < heldSpellCards.Length; i++)
        {
            if (heldSpellCards[i] != null)
            {
                continue;
            }

            heldSpellCards[i] = spellCard;
            Debug.Log($"Stored Spell in ConsumableSlot{i + 1}: {spellCard.Name}");
            return true;
        }

        Debug.Log("Cannot buy spell card: consumable slots are full.");
        return false;
    }

    private bool TryUseSpellCard(SpellCard spellCard, out string message)
    {
        message = "Cannot use Spell: invalid Spell card.";

        if (spellCard == null)
        {
            return false;
        }

        PlayingCard targetCard = null;

        if (spellCard.RequiresSingleSelectedHandCard && !TryGetSingleSelectedHandCard(out targetCard, out message))
        {
            return false;
        }

        switch (spellCard.spellType)
        {
            case SpellCardType.AuricCovenant:
                targetCard.enhancement = CardEnhancement.Gold;
                message = $"Auric Covenant: {targetCard.GetDisplayName()} became Gold.";
                return true;
            case SpellCardType.StoneboundOath:
                targetCard.enhancement = CardEnhancement.Stone;
                message = $"Stonebound Oath: {targetCard.GetDisplayName()} became Stone.";
                return true;
            case SpellCardType.FortuneInscription:
                targetCard.enhancement = CardEnhancement.Lucky;
                message = $"Fortune Inscription: {targetCard.GetDisplayName()} became Lucky.";
                return true;
            case SpellCardType.CrimsonSealRite:
                targetCard.seal = CardSeal.Red;
                message = $"Crimson Seal Rite: {targetCard.GetDisplayName()} gained Red Seal.";
                return true;
            case SpellCardType.GildedSealRite:
                targetCard.seal = CardSeal.Gold;
                message = $"Gilded Seal Rite: {targetCard.GetDisplayName()} gained Gold Seal.";
                return true;
            case SpellCardType.HermitsVault:
                int gainedGold = Mathf.Min(currentGold, 20);
                currentGold += gainedGold;
                message = $"Hermit’s Vault: gained ${gainedGold}. Current gold: {currentGold}";
                return true;
            case SpellCardType.GallowsOffering:
                return TryUseGallowsOffering(targetCard, out message);
            case SpellCardType.AscendantBlessing:
                return TryUseAscendantBlessing(out message);
            default:
                message = $"Cannot use Spell: {spellCard.Name} is not implemented.";
                return false;
        }
    }

    private bool TryGetSingleSelectedHandCard(out PlayingCard targetCard, out string message)
    {
        targetCard = null;
        List<PlayingCard> selected = GetSelectedCardsForAction();

        if (selected.Count != 1)
        {
            message = "Spell requires exactly 1 selected hand card.";
            return false;
        }

        targetCard = selected[0];

        if (!IsCardInCurrentHand(targetCard))
        {
            message = "Spell target failed: selected card is not in current hand.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private bool TryUseGallowsOffering(PlayingCard targetCard, out string message)
    {
        if (targetCard == null || !IsCardInCurrentHand(targetCard))
        {
            message = "Gallows Offering failed: target card is not in current hand.";
            return false;
        }

        List<PlayingCard> removedCards = handManager.RemoveCardsWithoutDiscard(new[] { targetCard });

        if (removedCards.Count == 0)
        {
            message = "Gallows Offering failed: target card could not be removed.";
            return false;
        }

        for (int i = 0; i < removedCards.Count; i++)
        {
            deckManager?.MarkDestroyed(removedCards[i]);
        }

        selectedCards.Remove(targetCard);
        targetCard.isSelected = false;
        currentGold += 5;
        ClearSelectedCards();
        message = $"Gallows Offering: destroyed {targetCard.GetDisplayName()} and gained $5. Current gold: {currentGold}";
        return true;
    }

    private bool TryUseAscendantBlessing(out string message)
    {
        if (latestScoreContext == null)
        {
            message = "Ascendant Blessing failed: no valid last played hand type.";
            return false;
        }

        PokerHandType handType = latestScoreContext.handType;
        handTypeLevelManager.Upgrade(handType);
        message = $"Ascendant Blessing: upgraded {handType} to Lv {handTypeLevelManager.GetLevel(handType)}.";
        return true;
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

    private bool CanUseSpellConsumable()
    {
        return CanAcceptGameplayInput() && !isInShop && !IsRoundOver() && !isResolvingPlayedHand;
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

        gameUIController.SetJokerTooltipContext(GetOwnedStoneCardCount());
        gameUIController.RefreshJokerBar(jokerManager.EquippedJokers);
    }

    private int GetOwnedStoneCardCount()
    {
        if (deckManager == null)
        {
            return 0;
        }

        List<PlayingCard> ownedCards = deckManager.GetAllOwnedCardsSnapshot(handManager?.CurrentHand);
        int stoneCount = 0;

        for (int i = 0; i < ownedCards.Count; i++)
        {
            if (ownedCards[i] != null && ownedCards[i].enhancement == CardEnhancement.Stone)
            {
                stoneCount++;
            }
        }

        return stoneCount;
    }

    private void ResetCashOutForNewBlind()
    {
        suitGoldThisBlind = 0;
        bonusCardGoldThisBlind = 0;
        lastCashOutTotal = 0;
        hasClaimedCashOut = false;
        heldCardEffectsProcessedThisBlind = false;
        hasProcessedFirstDiscardThisBlind = false;
        Debug.Log("CashOut reset for new blind.");
        Debug.Log($"suitGoldThisBlind = {suitGoldThisBlind}");
        Debug.Log($"bonusCardGoldThisBlind = {bonusCardGoldThisBlind}");
        Debug.Log($"hasClaimedCashOut = {hasClaimedCashOut}");
        Debug.Log($"heldCardEffectsProcessedThisBlind = {heldCardEffectsProcessedThisBlind}");
    }

    private void OpenCashOutPanel()
    {
        int fixedBlindReward = runManager.GetFixedBlindReward() + GetFixedBlindRewardBonus();
        int interest = Mathf.Min(currentGold / 5, 5 + GetInterestCapBonus());
        int discardBonus = roundManager.discardsRemaining;
        lastCashOutTotal = fixedBlindReward + suitGoldThisBlind + bonusCardGoldThisBlind + interest + discardBonus;

        Debug.Log("Blind passed. Opening CashOutPanel.");
        Debug.Log($"Fixed blind reward: {fixedBlindReward}");
        Debug.Log($"Suit gold this blind: {suitGoldThisBlind}");
        Debug.Log($"Bonus card gold this blind: {bonusCardGoldThisBlind}");
        Debug.Log($"Interest: {interest}");
        Debug.Log($"Discard bonus: {discardBonus}");
        Debug.Log($"CashOut total: {lastCashOutTotal}");

        gameAudioController?.PlayCashOutSfx();
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
            JokerRuleContext ruleContext = jokerManager != null ? jokerManager.BuildRuleContext() : null;
            PokerHandResult previewResult = pokerHandEvaluator.Evaluate(cardsToPreview, ruleContext);
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

        gameUIController.SetRunInfoSources(handTypeLevelManager, suitMasteryManager, handTypePlayCounts, edictManager?.PurchasedEdicts);
    }

    private void ApplyEdictRuntimeEffects()
    {
        int shopDiscount = GetShopPriceDiscount();

        if (shopManager != null)
        {
            shopManager.SetShopPriceDiscount(shopDiscount);
        }

        if (handTypeLevelManager != null)
        {
            handTypeLevelManager.GlobalBaseChipsBonus = GetHandTypeBaseChipsBonus();
        }

        gameUIController?.SetShopPriceDiscount(shopDiscount);
    }

    private int GetAdditionalHandsPerBlind()
    {
        return edictManager != null ? edictManager.AdditionalHandsPerBlind : 0;
    }

    private int GetAdditionalDiscardsPerBlind()
    {
        return edictManager != null ? edictManager.AdditionalDiscardsPerBlind : 0;
    }

    private int GetShopPriceDiscount()
    {
        return edictManager != null ? edictManager.ShopPriceDiscount : 0;
    }

    private int GetInterestCapBonus()
    {
        return edictManager != null ? edictManager.InterestCapBonus : 0;
    }

    private int GetFixedBlindRewardBonus()
    {
        return edictManager != null ? edictManager.FixedBlindRewardBonus : 0;
    }

    private int GetHandTypeBaseChipsBonus()
    {
        return edictManager != null ? edictManager.HandTypeBaseChipsBonus : 0;
    }

    private List<PlayingCard> GetHeldCardsSnapshot(IReadOnlyCollection<PlayingCard> cardsToPlay)
    {
        List<PlayingCard> heldCards = new List<PlayingCard>();

        if (handManager == null || handManager.CurrentHand == null)
        {
            return heldCards;
        }

        for (int i = 0; i < handManager.CurrentHand.Count; i++)
        {
            PlayingCard card = handManager.CurrentHand[i];

            if (card != null && !ContainsCard(cardsToPlay, card))
            {
                heldCards.Add(card);
            }
        }

        return heldCards;
    }

    private void NotifyJokersBlindStarted()
    {
        if (jokerManager == null || deckManager == null)
        {
            return;
        }

        jokerManager.NotifyBlindStarted(BuildJokerRuntimeContext(null, false));
    }

    private JokerRuntimeContext BuildJokerRuntimeContext(IReadOnlyList<PlayingCard> playedCardsSubmitted, bool isFirstPlayedHandThisBlind)
    {
        return new JokerRuntimeContext
        {
            deckManager = deckManager,
            handManager = handManager,
            pokerHandEvaluator = pokerHandEvaluator,
            handTypeLevelManager = handTypeLevelManager,
            ruleContext = jokerManager != null ? jokerManager.BuildRuleContext() : null,
            playedCardsSubmitted = playedCardsSubmitted,
            notifyPlayingCardAdded = NotifyPlayingCardAddedToDeck,
            addGold = AddGoldFromJoker,
            isFirstPlayedHandThisBlind = isFirstPlayedHandThisBlind,
            isBossBlind = runManager != null && runManager.IsBossBlind()
        };
    }

    private void NotifyPlayingCardAddedToDeck(PlayingCard card, string source)
    {
        jokerManager?.NotifyPlayingCardAdded(card, source);
        RefreshJokerBarUI();
    }

    private void ApplyPendingRuntimeCardsToHand(JokerRuntimeContext runtimeContext)
    {
        if (runtimeContext == null || runtimeContext.cardsToAddToHandAfterPlayedCardsRemoved == null)
        {
            return;
        }

        for (int i = 0; i < runtimeContext.cardsToAddToHandAfterPlayedCardsRemoved.Count; i++)
        {
            PlayingCard card = runtimeContext.cardsToAddToHandAfterPlayedCardsRemoved[i];

            if (card == null)
            {
                continue;
            }

            deckManager?.RemoveFromDrawPile(card);

            if (handManager != null && handManager.TryAddCardToHand(card))
            {
                Debug.Log($"Added pending Joker-created card to hand: {card.GetDisplayName()}");
            }
            else
            {
                deckManager?.AddNewOwnedCardToDrawPile(card);
                Debug.Log($"Could not add pending Joker-created card to hand; returned to draw pile: {card.GetDisplayName()}");
            }
        }

        runtimeContext.cardsToAddToHandAfterPlayedCardsRemoved.Clear();
    }

    private void AddGoldFromJoker(int amount)
    {
        currentGold += amount;
        Debug.Log($"Joker gold changed by {amount}. Current gold: {currentGold}");
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
        gameUIController.SetJokerTooltipContext(GetOwnedStoneCardCount());
        RefreshJokerBarUI();
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

    private int GetHandTypePlayCount(PokerHandType handType)
    {
        if (!handTypePlayCounts.ContainsKey(handType))
        {
            return 0;
        }

        return handTypePlayCounts[handType];
    }

    private void LogRoundEndIfNeeded()
    {
        if (hasShownGameResult)
        {
            return;
        }

        if (roundManager.HasPassedBlind)
        {
            Debug.Log("Blind passed.");
            ProcessHeldCardEffectsBeforeCashOut();
            jokerManager.NotifyBlindPassed(roundManager);
            jokerManager.NotifyBlindPassed(BuildJokerRuntimeContext(null, false));
            Debug.Log($"Jokers after Blind passed:\n{jokerManager.GetJokerListDebugText()}");

            if (HasWonRun())
            {
                ShowGameResult(GameResultType.Victory);
                return;
            }

            PrepareDeckAndHandForNextBlindPreview();
            OpenCashOutPanel();
        }
        else if (roundManager.HasFailedBlind)
        {
            Debug.Log("Blind failed.");
            gameUIController?.SetState(GameUIState.RunFailed);
            ShowGameResult(GameResultType.Defeat);
        }
    }

    private bool HasWonRun()
    {
        return runManager != null
            && roundManager != null
            && roundManager.HasPassedBlind
            && runManager.IsBossBlind()
            && runManager.GetAnteNumber() >= VictoryAnte;
    }

    private void ShowGameResult(GameResultType resultType)
    {
        if (hasShownGameResult)
        {
            return;
        }

        hasShownGameResult = true;
        isResolvingPlayedHand = false;
        CaptureGameResultSnapshot();
        gameUIController?.ShowGameResult(resultType, runResultStats);
        Debug.Log($"Game result shown: {resultType}");
    }

    private void CaptureGameResultSnapshot()
    {
        if (runManager != null)
        {
            runResultStats.anteNumber = runManager.GetAnteNumber();
            runResultStats.lastBlindName = GetCurrentBlindResultName();
        }
        else
        {
            runResultStats.anteNumber = 1;
            runResultStats.lastBlindName = "Unknown";
        }
    }

    private string GetCurrentBlindResultName()
    {
        if (bossBlindManager != null && bossBlindManager.IsActive && !string.IsNullOrWhiteSpace(bossBlindManager.CurrentDisplayName))
        {
            return bossBlindManager.CurrentDisplayName;
        }

        return runManager != null ? runManager.GetBlindDisplayName() : "Unknown";
    }

    private bool IsRoundOver()
    {
        return roundManager.HasPassedBlind || roundManager.HasFailedBlind;
    }
}
