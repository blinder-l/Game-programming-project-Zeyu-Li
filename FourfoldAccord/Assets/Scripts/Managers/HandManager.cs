using System.Collections.Generic;
using System.Text;

public class HandManager
{
    public const int DefaultHandSizeLimit = 8;
    private const int MaxSelectedCards = 5;

    private readonly List<PlayingCard> currentHand = new List<PlayingCard>();

    public IReadOnlyList<PlayingCard> CurrentHand => currentHand;
    public int HandSizeLimit { get; set; } = DefaultHandSizeLimit;
    public int CurrentHandCount => currentHand.Count;
    public int MaxSelectedCardCount => MaxSelectedCards;

    public void FillHand(DeckManager deckManager)
    {
        int cardsNeeded = HandSizeLimit - currentHand.Count;

        if (cardsNeeded <= 0)
        {
            return;
        }

        currentHand.AddRange(deckManager.DrawCards(cardsNeeded));
    }

    public bool TryAddCardToHand(PlayingCard card)
    {
        if (card == null || currentHand.Count >= HandSizeLimit)
        {
            return false;
        }

        card.isSelected = false;
        currentHand.Add(card);
        return true;
    }

    public void ToggleCardSelection(int handIndex)
    {
        if (handIndex < 0 || handIndex >= currentHand.Count)
        {
            return;
        }

        if (!currentHand[handIndex].isSelected && GetSelectedCardCount() >= MaxSelectedCards)
        {
            return;
        }

        currentHand[handIndex].isSelected = !currentHand[handIndex].isSelected;
    }

    public List<PlayingCard> GetSelectedCards()
    {
        List<PlayingCard> selectedCards = new List<PlayingCard>();

        for (int i = 0; i < currentHand.Count; i++)
        {
            if (currentHand[i].isSelected)
            {
                selectedCards.Add(currentHand[i]);
            }
        }

        return selectedCards;
    }

    public int GetSelectedCardCount()
    {
        int selectedCount = 0;

        for (int i = 0; i < currentHand.Count; i++)
        {
            if (currentHand[i].isSelected)
            {
                selectedCount++;
            }
        }

        return selectedCount;
    }

    public List<PlayingCard> PlaySelectedCards(DeckManager deckManager)
    {
        return RemoveSelectedCards(deckManager, true);
    }

    public List<PlayingCard> PlaySelectedCardsWithoutRefill(DeckManager deckManager)
    {
        return RemoveSelectedCards(deckManager, true, false);
    }

    public List<PlayingCard> DiscardSelectedCards(DeckManager deckManager)
    {
        return RemoveSelectedCards(deckManager, false);
    }

    public List<PlayingCard> RemoveSelectedCardsWithoutDiscard()
    {
        List<PlayingCard> removedCards = new List<PlayingCard>();

        for (int i = currentHand.Count - 1; i >= 0; i--)
        {
            PlayingCard card = currentHand[i];

            if (!card.isSelected)
            {
                continue;
            }

            card.isSelected = false;
            currentHand.RemoveAt(i);
            removedCards.Add(card);
        }

        removedCards.Reverse();
        return removedCards;
    }

    public List<PlayingCard> RemoveCardsWithoutDiscard(IReadOnlyCollection<PlayingCard> cardsToRemove)
    {
        return RemoveSpecificCards(cardsToRemove, null, false, false);
    }

    public List<PlayingCard> DiscardCards(IReadOnlyCollection<PlayingCard> cardsToDiscard, DeckManager deckManager)
    {
        return RemoveSpecificCards(cardsToDiscard, deckManager, false, true);
    }

    public void SortHandBySuit()
    {
        currentHand.Sort((firstCard, secondCard) =>
        {
            int suitComparison = GetSuitSortValue(firstCard.suit).CompareTo(GetSuitSortValue(secondCard.suit));

            if (suitComparison != 0)
            {
                return suitComparison;
            }

            return secondCard.rank.CompareTo(firstCard.rank);
        });
    }

    public void SortHandByRank()
    {
        currentHand.Sort((firstCard, secondCard) =>
        {
            int rankComparison = secondCard.rank.CompareTo(firstCard.rank);

            if (rankComparison != 0)
            {
                return rankComparison;
            }

            return GetSuitSortValue(firstCard.suit).CompareTo(GetSuitSortValue(secondCard.suit));
        });
    }

    private List<PlayingCard> RemoveSelectedCards(DeckManager deckManager, bool wasPlayed)
    {
        return RemoveSelectedCards(deckManager, wasPlayed, true);
    }

    private List<PlayingCard> RemoveSelectedCards(DeckManager deckManager, bool wasPlayed, bool refillHand)
    {
        List<PlayingCard> removedCards = new List<PlayingCard>();

        for (int i = currentHand.Count - 1; i >= 0; i--)
        {
            PlayingCard card = currentHand[i];

            if (!card.isSelected)
            {
                continue;
            }

            if (wasPlayed)
            {
                card.timesPlayed++;
            }
            else
            {
                card.timesDiscarded++;
            }

            card.isSelected = false;
            currentHand.RemoveAt(i);
            deckManager.AddToDiscardPile(card);
            removedCards.Add(card);
        }

        removedCards.Reverse();

        if (refillHand)
        {
            FillHand(deckManager);
        }

        return removedCards;
    }

    private List<PlayingCard> RemoveSpecificCards(
        IReadOnlyCollection<PlayingCard> cardsToRemove,
        DeckManager deckManager,
        bool wasPlayed,
        bool addToDiscardPile)
    {
        List<PlayingCard> removedCards = new List<PlayingCard>();

        if (cardsToRemove == null)
        {
            return removedCards;
        }

        for (int i = currentHand.Count - 1; i >= 0; i--)
        {
            PlayingCard card = currentHand[i];

            if (!ContainsCard(cardsToRemove, card))
            {
                continue;
            }

            if (wasPlayed)
            {
                card.timesPlayed++;
            }
            else if (addToDiscardPile)
            {
                card.timesDiscarded++;
            }

            card.isSelected = false;
            currentHand.RemoveAt(i);

            if (addToDiscardPile && deckManager != null)
            {
                deckManager.AddToDiscardPile(card);
            }

            removedCards.Add(card);
        }

        removedCards.Reverse();
        return removedCards;
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

    // Builds a readable list of cards currently in hand for console debugging.
    public string GetHandDebugText()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < currentHand.Count; i++)
        {
            string selectedMarker = currentHand[i].isSelected ? " [Selected]" : string.Empty;
            builder.AppendLine($"{i + 1}. {currentHand[i].GetDisplayName()}{selectedMarker}");
        }

        return builder.ToString();
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
}
