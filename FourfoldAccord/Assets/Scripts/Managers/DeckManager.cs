using System;
using System.Collections.Generic;
using System.Text;

public class DeckManager
{
    private readonly List<PlayingCard> drawPile = new List<PlayingCard>();
    private readonly List<PlayingCard> discardPile = new List<PlayingCard>();
    private readonly List<PlayingCard> destroyedCards = new List<PlayingCard>();
    private readonly Random random = new Random();

    public IReadOnlyList<PlayingCard> DrawPile => drawPile;
    public IReadOnlyList<PlayingCard> DiscardPile => discardPile;
    public IReadOnlyList<PlayingCard> DestroyedCards => destroyedCards;
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;

    public List<PlayingCard> GetDrawPileSnapshot()
    {
        return new List<PlayingCard>(drawPile);
    }

    public List<PlayingCard> GetAllOwnedCardsSnapshot(IReadOnlyList<PlayingCard> currentHand)
    {
        List<PlayingCard> ownedCards = new List<PlayingCard>();
        HashSet<string> includedInstanceIds = new HashSet<string>();

        AddUniqueCardsToSnapshot(ownedCards, includedInstanceIds, drawPile);
        AddUniqueCardsToSnapshot(ownedCards, includedInstanceIds, discardPile);
        AddUniqueCardsToSnapshot(ownedCards, includedInstanceIds, currentHand);

        return ownedCards;
    }

    public List<PlayingCard> GetStandardDeckSnapshot()
    {
        List<PlayingCard> standardDeck = new List<PlayingCard>();
        int nextUniqueId = 0;

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                standardDeck.Add(new PlayingCard(nextUniqueId, suit, rank));
                nextUniqueId++;
            }
        }

        return standardDeck;
    }

    public void CreateStandardDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        destroyedCards.Clear();

        int nextUniqueId = 0;

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                drawPile.Add(new PlayingCard(nextUniqueId, suit, rank));
                nextUniqueId++;
            }
        }
    }

    public void LoadOwnedCardsAsDrawPile(IReadOnlyList<PlayingCard> ownedCards)
    {
        drawPile.Clear();
        discardPile.Clear();
        destroyedCards.Clear();

        if (ownedCards == null)
        {
            return;
        }

        HashSet<string> includedInstanceIds = new HashSet<string>();

        for (int i = 0; i < ownedCards.Count; i++)
        {
            PlayingCard card = ownedCards[i];

            if (card == null)
            {
                continue;
            }

            string instanceKey = string.IsNullOrEmpty(card.instanceId) ? card.uniqueId.ToString() : card.instanceId;

            if (includedInstanceIds.Contains(instanceKey))
            {
                continue;
            }

            card.isSelected = false;
            includedInstanceIds.Add(instanceKey);
            drawPile.Add(card);
        }
    }

    public void Shuffle()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);
            PlayingCard temporaryCard = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temporaryCard;
        }
    }

    public PlayingCard DrawCard()
    {
        if (drawPile.Count == 0)
        {
            return null;
        }

        PlayingCard drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        return drawnCard;
    }

    public List<PlayingCard> DrawCards(int count)
    {
        List<PlayingCard> drawnCards = new List<PlayingCard>();

        for (int i = 0; i < count && drawPile.Count > 0; i++)
        {
            drawnCards.Add(DrawCard());
        }

        return drawnCards;
    }

    public void AddToDiscardPile(PlayingCard card)
    {
        if (card == null)
        {
            return;
        }

        discardPile.Add(card);
    }

    public void AddNewOwnedCardToDrawPile(PlayingCard card)
    {
        if (card == null)
        {
            return;
        }

        card.isSelected = false;
        drawPile.Add(card);
    }

    public bool RemoveFromDrawPile(PlayingCard card)
    {
        if (card == null)
        {
            return false;
        }

        return drawPile.Remove(card);
    }

    public bool RemoveFromDiscardPile(PlayingCard card)
    {
        if (card == null)
        {
            return false;
        }

        return discardPile.Remove(card);
    }

    public void MarkDestroyed(PlayingCard card)
    {
        if (card == null)
        {
            return;
        }

        RemoveFromDrawPile(card);
        RemoveFromDiscardPile(card);

        if (!destroyedCards.Contains(card))
        {
            destroyedCards.Add(card);
        }
    }

    public int GetNextUniqueId(IReadOnlyList<PlayingCard> currentHand)
    {
        int maxUniqueId = -1;
        UpdateMaxUniqueId(drawPile, ref maxUniqueId);
        UpdateMaxUniqueId(discardPile, ref maxUniqueId);
        UpdateMaxUniqueId(destroyedCards, ref maxUniqueId);
        UpdateMaxUniqueId(currentHand, ref maxUniqueId);
        return maxUniqueId + 1;
    }

    // Builds a readable list of all cards currently in the draw pile for console debugging.
    public string GetDrawPileDebugText()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < drawPile.Count; i++)
        {
            builder.AppendLine($"{i + 1}. {drawPile[i].GetDisplayName()} (ID: {drawPile[i].uniqueId})");
        }

        return builder.ToString();
    }

    private void AddUniqueCardsToSnapshot(
        List<PlayingCard> target,
        HashSet<string> includedInstanceIds,
        IReadOnlyList<PlayingCard> source)
    {
        if (target == null || includedInstanceIds == null || source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            PlayingCard card = source[i];

            if (card == null)
            {
                continue;
            }

            string instanceKey = string.IsNullOrEmpty(card.instanceId) ? card.uniqueId.ToString() : card.instanceId;

            if (includedInstanceIds.Contains(instanceKey))
            {
                continue;
            }

            includedInstanceIds.Add(instanceKey);
            target.Add(card);
        }
    }

    private void UpdateMaxUniqueId(IReadOnlyList<PlayingCard> cards, ref int maxUniqueId)
    {
        if (cards == null)
        {
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null && cards[i].uniqueId > maxUniqueId)
            {
                maxUniqueId = cards[i].uniqueId;
            }
        }
    }
}
