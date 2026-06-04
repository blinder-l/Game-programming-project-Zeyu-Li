using System;
using System.Collections.Generic;

public class ScoreManager
{
    public ScoreContext CalculateScore(PokerHandResult pokerHandResult)
    {
        if (pokerHandResult == null)
        {
            pokerHandResult = new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>());
        }

        int chips = GetBaseChips(pokerHandResult.handType);
        float mult = GetBaseMult(pokerHandResult.handType);
        int finalScore = (int)Math.Round(chips * mult);

        return new ScoreContext(
            new List<PlayingCard>(pokerHandResult.scoringCards),
            pokerHandResult.handType,
            chips,
            mult,
            finalScore);
    }

    private int GetBaseChips(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.Pair:
                return 20;
            case PokerHandType.TwoPair:
                return 40;
            case PokerHandType.ThreeOfAKind:
                return 60;
            case PokerHandType.Straight:
                return 100;
            case PokerHandType.Flush:
                return 120;
            case PokerHandType.FullHouse:
                return 160;
            case PokerHandType.FourOfAKind:
                return 220;
            case PokerHandType.StraightFlush:
                return 300;
            default:
                return 10;
        }
    }

    private float GetBaseMult(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.Pair:
                return 1.2f;
            case PokerHandType.TwoPair:
                return 1.5f;
            case PokerHandType.ThreeOfAKind:
                return 2.0f;
            case PokerHandType.Straight:
                return 2.0f;
            case PokerHandType.Flush:
                return 2.5f;
            case PokerHandType.FullHouse:
                return 3.0f;
            case PokerHandType.FourOfAKind:
                return 4.0f;
            case PokerHandType.StraightFlush:
                return 5.0f;
            default:
                return 1.0f;
        }
    }
}
