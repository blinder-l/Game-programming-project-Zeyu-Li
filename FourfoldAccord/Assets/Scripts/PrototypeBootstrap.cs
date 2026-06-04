using System.Collections.Generic;
using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private void Start()
    {
        DeckManager deckManager = new DeckManager();
        deckManager.CreateStandardDeck();

        Debug.Log("Prototype started");
        Debug.Log($"Deck count before shuffle: {deckManager.DrawPileCount}");
        Debug.Log($"Top card before shuffle: {deckManager.DrawPile[0].GetDisplayName()}");

        deckManager.Shuffle();
        Debug.Log("Deck shuffled");

        HandManager handManager = new HandManager();
        handManager.FillHand(deckManager);

        Debug.Log($"Initial hand count: {handManager.CurrentHandCount}");
        Debug.Log($"Deck count after filling hand: {deckManager.DrawPileCount}");
        Debug.Log($"Initial hand:\n{handManager.GetHandDebugText()}");

        handManager.ToggleCardSelection(0);
        handManager.ToggleCardSelection(1);
        Debug.Log($"Selected for play:\n{handManager.GetHandDebugText()}");

        PokerHandEvaluator pokerHandEvaluator = new PokerHandEvaluator();
        PokerHandResult pokerHandResult = pokerHandEvaluator.Evaluate(handManager.GetSelectedCards());
        Debug.Log($"Detected hand: {pokerHandResult.handType}");

        List<PlayingCard> playedCards = handManager.PlaySelectedCards(deckManager);
        Debug.Log($"Played {playedCards.Count} cards");
        Debug.Log($"Hand after play refill:\n{handManager.GetHandDebugText()}");
        Debug.Log($"Deck count after play refill: {deckManager.DrawPileCount}");
        Debug.Log($"Discard pile count after play: {deckManager.DiscardPileCount}");

        handManager.ToggleCardSelection(0);
        handManager.ToggleCardSelection(1);
        handManager.ToggleCardSelection(2);
        Debug.Log($"Selected for discard:\n{handManager.GetHandDebugText()}");

        List<PlayingCard> discardedCards = handManager.DiscardSelectedCards(deckManager);
        Debug.Log($"Discarded {discardedCards.Count} cards");
        Debug.Log($"Hand after discard refill:\n{handManager.GetHandDebugText()}");
        Debug.Log($"Deck count after discard refill: {deckManager.DrawPileCount}");
        Debug.Log($"Discard pile count after discard: {deckManager.DiscardPileCount}");
    }
}
