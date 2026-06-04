using System.Collections.Generic;
using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private DeckManager deckManager;
    private HandManager handManager;
    private PokerHandEvaluator pokerHandEvaluator;
    private ScoreManager scoreManager;
    private RoundManager roundManager;

    private void Start()
    {
        deckManager = new DeckManager();
        deckManager.CreateStandardDeck();
        deckManager.Shuffle();

        handManager = new HandManager();
        handManager.FillHand(deckManager);

        pokerHandEvaluator = new PokerHandEvaluator();
        scoreManager = new ScoreManager();
        roundManager = new RoundManager();

        Debug.Log("Prototype started");
        Debug.Log("Controls: 1-8 select cards, P play selected cards, D discard selected cards");
        LogCurrentState();
    }

    private void Update()
    {
        if (IsRoundOver())
        {
            return;
        }

        HandleCardSelectionInput();

        if (Input.GetKeyDown(KeyCode.P))
        {
            TryPlaySelectedCards();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            TryDiscardSelectedCards();
        }
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

    private void TryPlaySelectedCards()
    {
        List<PlayingCard> selectedCards = handManager.GetSelectedCards();

        if (selectedCards.Count < 1 || selectedCards.Count > 5)
        {
            Debug.Log("Play requires selecting 1 to 5 cards.");
            return;
        }

        PokerHandResult pokerHandResult = pokerHandEvaluator.Evaluate(selectedCards);
        ScoreContext scoreContext = scoreManager.CalculateScore(pokerHandResult);

        List<PlayingCard> playedCards = handManager.PlaySelectedCards(deckManager);
        roundManager.ApplyPlayedHandScore(scoreContext.finalScore);

        Debug.Log($"Played {playedCards.Count} cards");
        Debug.Log($"Hand Type: {scoreContext.handType}");
        Debug.Log($"Chips: {scoreContext.chips} | Mult: {scoreContext.mult} | Final Score: {scoreContext.finalScore}");
        LogCurrentState();
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
        Debug.Log($"Round: {roundManager.GetDebugStatus()}");
        Debug.Log($"Deck count: {deckManager.DrawPileCount} | Discard pile count: {deckManager.DiscardPileCount}");
        Debug.Log($"Current hand:\n{handManager.GetHandDebugText()}");
    }

    private void LogRoundEndIfNeeded()
    {
        if (roundManager.HasPassedBlind)
        {
            Debug.Log("Blind passed.");
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
