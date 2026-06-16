using System;
using System.Collections.Generic;

public enum GameResultType
{
    Victory,
    Defeat,
    Retreat
}

public class GameResultStats
{
    public int bestScore;
    public int cardsPlayed;
    public int cardsDiscarded;
    public int cardsPurchased;
    public int totalRerollsUsed;
    public int anteNumber;
    public string lastBlindName = string.Empty;

    private readonly Dictionary<PokerHandType, int> playedHandTypeCounts = new Dictionary<PokerHandType, int>();

    public IReadOnlyDictionary<PokerHandType, int> PlayedHandTypeCounts => playedHandTypeCounts;

    public void Reset()
    {
        bestScore = 0;
        cardsPlayed = 0;
        cardsDiscarded = 0;
        cardsPurchased = 0;
        totalRerollsUsed = 0;
        anteNumber = 1;
        lastBlindName = string.Empty;
        playedHandTypeCounts.Clear();

        foreach (PokerHandType handType in Enum.GetValues(typeof(PokerHandType)))
        {
            playedHandTypeCounts[handType] = 0;
        }
    }

    public void RecordPlayedHand(PokerHandType handType, int finalScore, int playedCardCount)
    {
        bestScore = Math.Max(bestScore, finalScore);
        cardsPlayed += Math.Max(0, playedCardCount);

        if (!playedHandTypeCounts.ContainsKey(handType))
        {
            playedHandTypeCounts[handType] = 0;
        }

        playedHandTypeCounts[handType]++;
    }

    public void RecordDiscardedCards(int discardedCardCount)
    {
        cardsDiscarded += Math.Max(0, discardedCardCount);
    }

    public void RecordPurchasedCard()
    {
        cardsPurchased++;
    }

    public void RecordReroll()
    {
        totalRerollsUsed++;
    }

    public PokerHandType GetMostUsedHandType()
    {
        PokerHandType bestHandType = PokerHandType.HighCard;
        int bestCount = -1;

        foreach (PokerHandType handType in Enum.GetValues(typeof(PokerHandType)))
        {
            int count = playedHandTypeCounts.ContainsKey(handType) ? playedHandTypeCounts[handType] : 0;

            if (count > bestCount || (count == bestCount && handType > bestHandType))
            {
                bestCount = count;
                bestHandType = handType;
            }
        }

        return bestHandType;
    }

    public string GetMostUsedHandTypeText()
    {
        int totalHands = 0;

        foreach (int count in playedHandTypeCounts.Values)
        {
            totalHands += count;
        }

        return totalHands > 0 ? FormatHandType(GetMostUsedHandType()) : "None";
    }

    private string FormatHandType(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.HighCard:
                return "High Card";
            case PokerHandType.Pair:
                return "Pair";
            case PokerHandType.TwoPair:
                return "Two Pair";
            case PokerHandType.ThreeOfAKind:
                return "Three of a Kind";
            case PokerHandType.Straight:
                return "Straight";
            case PokerHandType.Flush:
                return "Flush";
            case PokerHandType.FullHouse:
                return "Full House";
            case PokerHandType.FourOfAKind:
                return "Four of a Kind";
            case PokerHandType.StraightFlush:
                return "Straight Flush";
            default:
                return handType.ToString();
        }
    }
}
