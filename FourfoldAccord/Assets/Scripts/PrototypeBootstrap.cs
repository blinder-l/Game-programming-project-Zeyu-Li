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
        RunRankBasedHandTests(pokerHandEvaluator);

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

    private void RunRankBasedHandTests(PokerHandEvaluator pokerHandEvaluator)
    {
        List<PlayingCard> pairTestCards = new List<PlayingCard>
        {
            new PlayingCard(100, Suit.Hearts, Rank.Ace),
            new PlayingCard(101, Suit.Spades, Rank.Ace)
        };
        Debug.Log($"Test Pair: {pokerHandEvaluator.Evaluate(pairTestCards).handType}");

        List<PlayingCard> twoPairTestCards = new List<PlayingCard>
        {
            new PlayingCard(102, Suit.Hearts, Rank.King),
            new PlayingCard(103, Suit.Spades, Rank.King),
            new PlayingCard(104, Suit.Clubs, Rank.Three),
            new PlayingCard(105, Suit.Diamonds, Rank.Three)
        };
        Debug.Log($"Test Two Pair: {pokerHandEvaluator.Evaluate(twoPairTestCards).handType}");

        List<PlayingCard> threeOfAKindTestCards = new List<PlayingCard>
        {
            new PlayingCard(106, Suit.Hearts, Rank.Queen),
            new PlayingCard(107, Suit.Spades, Rank.Queen),
            new PlayingCard(108, Suit.Clubs, Rank.Queen)
        };
        Debug.Log($"Test Three of a Kind: {pokerHandEvaluator.Evaluate(threeOfAKindTestCards).handType}");

        List<PlayingCard> fullHouseTestCards = new List<PlayingCard>
        {
            new PlayingCard(109, Suit.Hearts, Rank.Ten),
            new PlayingCard(110, Suit.Spades, Rank.Ten),
            new PlayingCard(111, Suit.Clubs, Rank.Ten),
            new PlayingCard(112, Suit.Hearts, Rank.Four),
            new PlayingCard(113, Suit.Spades, Rank.Four)
        };
        Debug.Log($"Test Full House: {pokerHandEvaluator.Evaluate(fullHouseTestCards).handType}");

        List<PlayingCard> fourOfAKindTestCards = new List<PlayingCard>
        {
            new PlayingCard(114, Suit.Hearts, Rank.Nine),
            new PlayingCard(115, Suit.Spades, Rank.Nine),
            new PlayingCard(116, Suit.Clubs, Rank.Nine),
            new PlayingCard(117, Suit.Diamonds, Rank.Nine)
        };
        Debug.Log($"Test Four of a Kind: {pokerHandEvaluator.Evaluate(fourOfAKindTestCards).handType}");
    }
}
