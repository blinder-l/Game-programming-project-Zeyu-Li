using System;
using System.Collections.Generic;

public class ScoreManager
{
    private readonly SuitEffectManager suitEffectManager = new SuitEffectManager();

    public ScoreContext CalculateScore(PokerHandResult pokerHandResult)
    {
        return CalculateScore(pokerHandResult, null);
    }

    public ScoreContext CalculateScore(PokerHandResult pokerHandResult, SuitMasteryManager suitMasteryManager)
    {
        if (pokerHandResult == null)
        {
            pokerHandResult = new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>());
        }

        List<PlayingCard> scoringCards = pokerHandResult.scoringCards ?? new List<PlayingCard>();
        Dictionary<PlayingCard, int> cardChipValues = GetCardChipValues(scoringCards);
        Dictionary<Suit, int> suitCounts = GetSuitCounts(scoringCards);
        int baseChips = GetBaseChips(pokerHandResult.handType);
        int rankChips = GetRankChips(cardChipValues);
        int chips = baseChips + rankChips;
        float mult = GetBaseMult(pokerHandResult.handType);
        int finalScore = (int)Math.Round(chips * mult);

        ScoreContext scoreContext = new ScoreContext(
            new List<PlayingCard>(scoringCards),
            pokerHandResult.handType,
            baseChips,
            rankChips,
            chips,
            mult,
            finalScore,
            cardChipValues,
            suitCounts,
            0,
            new List<string>());

        suitEffectManager.ApplySuitEffects(scoreContext, suitMasteryManager);
        return scoreContext;
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

    private Dictionary<PlayingCard, int> GetCardChipValues(List<PlayingCard> cards)
    {
        Dictionary<PlayingCard, int> cardChipValues = new Dictionary<PlayingCard, int>();

        for (int i = 0; i < cards.Count; i++)
        {
            cardChipValues[cards[i]] = GetRankChipValue(cards[i].rank);
        }

        return cardChipValues;
    }

    private int GetRankChips(Dictionary<PlayingCard, int> cardChipValues)
    {
        int rankChips = 0;

        foreach (KeyValuePair<PlayingCard, int> cardChipValue in cardChipValues)
        {
            rankChips += cardChipValue.Value;
        }

        return rankChips;
    }

    private int GetRankChipValue(Rank rank)
    {
        switch (rank)
        {
            case Rank.Two:
                return 2;
            case Rank.Three:
                return 3;
            case Rank.Four:
                return 4;
            case Rank.Five:
                return 5;
            case Rank.Six:
                return 6;
            case Rank.Seven:
                return 7;
            case Rank.Eight:
                return 8;
            case Rank.Nine:
                return 9;
            case Rank.Ace:
                return 11;
            default:
                return 10;
        }
    }

    private Dictionary<Suit, int> GetSuitCounts(List<PlayingCard> cards)
    {
        Dictionary<Suit, int> suitCounts = new Dictionary<Suit, int>();

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            suitCounts[suit] = 0;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            suitCounts[cards[i].suit]++;
        }

        return suitCounts;
    }
}
