using System;
using System.Collections.Generic;
using System.Text;

public class DeckManager
{
    private readonly List<PlayingCard> drawPile = new List<PlayingCard>();
    private readonly List<PlayingCard> discardPile = new List<PlayingCard>();
    private readonly Random random = new Random();

    public IReadOnlyList<PlayingCard> DrawPile => drawPile;
    public IReadOnlyList<PlayingCard> DiscardPile => discardPile;
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;

    public void CreateStandardDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

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
}
