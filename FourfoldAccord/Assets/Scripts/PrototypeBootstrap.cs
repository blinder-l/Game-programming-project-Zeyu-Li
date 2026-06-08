using System.Collections.Generic;
using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private DeckManager deckManager;
    private HandManager handManager;
    private PokerHandEvaluator pokerHandEvaluator;
    private ScoreManager scoreManager;
    private RoundManager roundManager;
    private RunManager runManager;
    private SuitMasteryManager suitMasteryManager;
    private JokerManager jokerManager;

    private void Start()
    {
        pokerHandEvaluator = new PokerHandEvaluator();
        scoreManager = new ScoreManager();
        runManager = new RunManager();
        suitMasteryManager = new SuitMasteryManager();
        jokerManager = new JokerManager();

        Debug.Log("Prototype started");
        Debug.Log("Controls: 1-8 select cards, P play selected cards, D discard selected cards, N start next Blind, F1-F4 equip suit retrigger Jokers, F5 equip high risk Joker, F6 equip stored discard Joker");
        StartCurrentBlind();
    }

    private void Update()
    {
        if (IsRoundOver())
        {
            HandleRoundOverInput();
            return;
        }

        HandleCardSelectionInput();
        HandleJokerDebugInput();

        if (Input.GetKeyDown(KeyCode.P))
        {
            TryPlaySelectedCards();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            TryDiscardSelectedCards();
        }
    }

    private void HandleRoundOverInput()
    {
        if (roundManager.HasPassedBlind && Input.GetKeyDown(KeyCode.N))
        {
            StartNextBlind();
        }
    }

    private void StartCurrentBlind()
    {
        deckManager = new DeckManager();
        deckManager.CreateStandardDeck();
        deckManager.Shuffle();

        handManager = new HandManager();
        handManager.FillHand(deckManager);

        roundManager = new RoundManager(runManager.GetCurrentTargetScore());

        Debug.Log($"Starting {runManager.GetDebugStatus()}");
        LogCurrentState();
    }

    private void StartNextBlind()
    {
        runManager.AdvanceToNextBlind();
        Debug.Log($"Advancing to Blind {runManager.CurrentBlindNumber}");
        StartCurrentBlind();
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
                Debug.Log($"Toggled card {i + 1}");
                LogCurrentState();
            }
        }
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
        List<Suit> gainedXpSuits = suitMasteryManager.AddXpForScoringSuits(scoreContext.suitCounts);

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

        Debug.Log($"Discarded {discardedCards.Count} cards");
        LogCurrentState();
    }

    private void LogCurrentState()
    {
        Debug.Log($"Run: {runManager.GetDebugStatus()}");
        Debug.Log($"Round: {roundManager.GetDebugStatus()}");
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
        Debug.Log($"Score Breakdown:\nBase Chips: {scoreContext.baseChips}\nRank Chips: {scoreContext.rankChips}\nTotal Chips: {scoreContext.chips}\nMult: {scoreContext.mult}\nFinal Score: {scoreContext.finalScore}\nGold Reward: {scoreContext.goldReward}");
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

    private void LogRoundEndIfNeeded()
    {
        if (roundManager.HasPassedBlind)
        {
            jokerManager.NotifyBlindPassed(roundManager);
            Debug.Log("Blind passed.");
            Debug.Log("Press N to start the next Blind.");
            Debug.Log($"Jokers after Blind passed:\n{jokerManager.GetJokerListDebugText()}");
        }
        else if (roundManager.HasFailedBlind)
        {
            Debug.Log("Blind failed.");
        }
    }

    private bool IsRoundOver()
    {
        return roundManager.HasPassedBlind || roundManager.HasFailedBlind;
    }
}
