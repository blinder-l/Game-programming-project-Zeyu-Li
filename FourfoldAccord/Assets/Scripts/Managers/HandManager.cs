using System.Collections.Generic;
using System.Text;

public class HandManager
{
    private const int MaxSelectedCards = 5;

    private readonly List<PlayingCard> currentHand = new List<PlayingCard>();

    public IReadOnlyList<PlayingCard> CurrentHand => currentHand;
    public int HandSizeLimit { get; } = 8;
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

    public List<PlayingCard> DiscardSelectedCards(DeckManager deckManager)
    {
        return RemoveSelectedCards(deckManager, false);
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
        FillHand(deckManager);
        return removedCards;
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
