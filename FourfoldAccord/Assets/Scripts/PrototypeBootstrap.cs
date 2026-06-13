using System.Collections.Generic;
using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private const int StartingGold = 10;

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
        }

        Debug.Log("Prototype started");
        Debug.Log("Controls: 1-8 select cards, S sort by suit, T sort by rank, P play selected cards, D discard selected cards, Shop: 1-3 buy, R reroll, N leave, F1-F4 equip suit retrigger Jokers, F5 equip high risk Joker, F6 equip stored discard Joker");
        StartCurrentBlind();
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

    private void ApplyBlindClearGoldRewards()
    {
        int interest = Mathf.Min(currentGold / 5, 5);
        int remainingDiscardsGold = roundManager.discardsRemaining;
        int blindClearGoldTotal = interest + remainingDiscardsGold;

        currentGold += blindClearGoldTotal;

        Debug.Log("=== Blind Clear Gold Rewards ===");
        Debug.Log($"Interest: +{interest}");
        Debug.Log($"Remaining Discards Gold: +{remainingDiscardsGold}");
        Debug.Log($"End of Blind Gold Total: +{blindClearGoldTotal}");
        Debug.Log($"Current Gold: {currentGold}");
    }

    private void EnterShop()
    {
        isInShop = true;
        shopManager.GenerateShopOptions();
        gameUIController?.SetState(GameUIState.Shop);

        Debug.Log("=== Shop ===");
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
            Debug.Log("Leaving shop.");
            StartNextBlind();
        }
    }

    private void LogShopState()
    {
        Debug.Log("Shop controls: 1-3 buy a Joker, R reroll shop (Cost: 2 Gold), N leave shop");
        Debug.Log($"Gold: {currentGold}");
        Debug.Log($"Joker Slots: {jokerManager.EquippedJokers.Count}/{jokerManager.MaxJokerSlots}");
        Debug.Log($"Shop Options:\n{shopManager.GetShopDebugText()}");
        Debug.Log($"Current Jokers:\n{jokerManager.GetJokerListDebugText()}");
    }

    private void TryBuyShopOption(int optionIndex)
    {
        JokerBase joker = shopManager.GetOption(optionIndex);

        if (joker == null)
        {
            Debug.Log("Invalid shop option.");
            LogShopState();
            return;
        }

        if (jokerManager.EquippedJokers.Count >= jokerManager.MaxJokerSlots)
        {
            Debug.Log($"Could not buy {joker.Name}: Joker slots are full.");
            LogShopState();
            return;
        }

        if (currentGold < joker.Cost)
        {
            Debug.Log($"Not enough Gold to buy {joker.Name}. Cost: {joker.Cost}, Gold: {currentGold}");
            LogShopState();
            return;
        }

        if (!jokerManager.TryEquipJoker(joker))
        {
            Debug.Log($"Could not buy Joker: {joker.Name}");
            LogShopState();
            return;
        }

        currentGold -= joker.Cost;
        shopManager.RemoveOption(optionIndex);
        Debug.Log($"Bought Joker: {joker.Name} for {joker.Cost} Gold. Gold remaining: {currentGold}");
        LogShopState();
    }

    private void TryRerollShop()
    {
        if (currentGold < ShopManager.RerollCost)
        {
            Debug.Log($"Not enough Gold to reroll shop. Cost: {ShopManager.RerollCost}, Gold: {currentGold}");
            LogShopState();
            return;
        }

        currentGold -= ShopManager.RerollCost;
        shopManager.GenerateShopOptions();

        Debug.Log($"Rerolled shop for {ShopManager.RerollCost} Gold. Gold remaining: {currentGold}");
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
        if (isInShop || IsRoundOver())
        {
            return;
        }

        TryPlaySelectedCards();
    }

    private void HandleDiscardButtonClicked()
    {
        if (isInShop || IsRoundOver())
        {
            return;
        }

        TryDiscardSelectedCards();
    }

    private void HandleCardSelectionInput()
    {
        for (int i = 0; i < handManager.CurrentHandCount; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);

            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                handManager.ToggleCardSelection(i);
                RefreshGameUI();
                Debug.Log($"Toggled card {i + 1}");
                LogCurrentState();
            }
        }
    }

    private void HandleHandCardClicked(int handIndex)
    {
        if (isInShop || IsRoundOver())
        {
            return;
        }

        handManager.ToggleCardSelection(handIndex);
        RefreshGameUI();
        Debug.Log($"Clicked hand card {handIndex + 1}");
        LogCurrentState();
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
        if (isInShop || IsRoundOver())
        {
            return;
        }

        SortHandBySuitAndRefresh();
    }

    private void HandleSortByRankButtonClicked()
    {
        if (isInShop || IsRoundOver())
        {
            return;
        }

        SortHandByRankAndRefresh();
    }

    private void SortHandBySuitAndRefresh()
    {
        handManager.SortHandBySuit();
        RefreshGameUI();
        Debug.Log("Sorted current hand by suit.");
        Debug.Log($"Current hand:\n{handManager.GetHandDebugText()}");
    }

    private void SortHandByRankAndRefresh()
    {
        handManager.SortHandByRank();
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
            TryEquipDebugJoker(new SuitRetriggerJoker(Suit.Diamonds));
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            TryEquipDebugJoker(new SuitRetriggerJoker(Suit.Clubs));
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            TryEquipDebugJoker(new SuitRetriggerJoker(Suit.Spades));
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
            Debug.Log($"Equipped Joker: {joker.Name}");
        }
        else
        {
            Debug.Log($"Could not equip Joker: {joker.Name}");
        }

        LogCurrentState();
    }

    private void TryPlaySelectedCards()
    {
        List<PlayingCard> selectedCards = handManager.GetSelectedCards();

        if (selectedCards.Count < 1 || selectedCards.Count > 5)
        {
            Debug.Log("Play requires selecting 1 to 5 cards.");
            return;
        }

        PokerHandResult pokerHandResult = pokerHandEvaluator.Evaluate(selectedCards);
        ScoreContext scoreContext = scoreManager.CalculateScore(pokerHandResult, suitMasteryManager, jokerManager);

        List<PlayingCard> playedCards = handManager.PlaySelectedCards(deckManager);
        roundManager.ApplyPlayedHandScore(scoreContext.finalScore);
        currentGold += scoreContext.goldReward;
        List<Suit> gainedXpSuits = suitMasteryManager.AddXpForScoringSuits(scoreContext.suitCounts);
        RefreshGameUI();

        LogPlayedHandResolution(playedCards, scoreContext, gainedXpSuits);
        LogRoundEndIfNeeded();
    }

    private void TryDiscardSelectedCards()
    {
        List<PlayingCard> selectedCards = handManager.GetSelectedCards();

        if (selectedCards.Count < 1)
        {
            Debug.Log("Discard requires selecting at least 1 card.");
            return;
        }

        if (roundManager.discardsRemaining <= 0)
        {
            Debug.Log("No discards remaining.");
            return;
        }

        roundManager.UseDiscard();
        List<PlayingCard> discardedCards = handManager.DiscardSelectedCards(deckManager);
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
        gameUIController?.RefreshHand(handManager?.CurrentHand);
    }

    private void LogRoundEndIfNeeded()
    {
        if (roundManager.HasPassedBlind)
        {
            Debug.Log("Blind passed.");
            ApplyBlindClearGoldRewards();
            jokerManager.NotifyBlindPassed(roundManager);
            Debug.Log($"Jokers after Blind passed:\n{jokerManager.GetJokerListDebugText()}");
            EnterShop();
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
