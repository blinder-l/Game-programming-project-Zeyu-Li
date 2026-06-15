using System;
using System.Collections.Generic;
using System.Text;

public class CardScoreEvent
{
    public PlayingCard card;
    public int chipValue;
    public bool isRetrigger;

    public CardScoreEvent(PlayingCard card, int chipValue, bool isRetrigger)
    {
        this.card = card;
        this.chipValue = chipValue;
        this.isRetrigger = isRetrigger;
    }
}

public class ScoreContext
{
    public List<PlayingCard> playedCards;
    public PokerHandType handType;
    public int baseChips;
    public int rankChips;
    public int chips;
    public float mult;
    public int finalScore;
    public Dictionary<PlayingCard, int> cardChipValues;
    public List<CardScoreEvent> cardScoreEvents;
    public Dictionary<Suit, int> suitCounts;
    public int goldReward;
    public int bonusCardGoldReward;
    public List<string> triggeredCardEffectLog;
    public List<string> triggeredSuitEffectLog;
    public List<string> triggeredJokerEffectLog;

    public ScoreContext(
        List<PlayingCard> playedCards,
        PokerHandType handType,
        int baseChips,
        int rankChips,
        int chips,
        float mult,
        int finalScore,
        Dictionary<PlayingCard, int> cardChipValues,
        List<CardScoreEvent> cardScoreEvents,
        Dictionary<Suit, int> suitCounts,
        int goldReward,
        int bonusCardGoldReward,
        List<string> triggeredCardEffectLog,
        List<string> triggeredSuitEffectLog,
        List<string> triggeredJokerEffectLog)
    {
        this.playedCards = playedCards;
        this.handType = handType;
        this.baseChips = baseChips;
        this.rankChips = rankChips;
        this.chips = chips;
        this.mult = mult;
        this.finalScore = finalScore;
        this.cardChipValues = cardChipValues;
        this.cardScoreEvents = cardScoreEvents;
        this.suitCounts = suitCounts;
        this.goldReward = goldReward;
        this.bonusCardGoldReward = bonusCardGoldReward;
        this.triggeredCardEffectLog = triggeredCardEffectLog;
        this.triggeredSuitEffectLog = triggeredSuitEffectLog;
        this.triggeredJokerEffectLog = triggeredJokerEffectLog;
    }

    public string GetCardChipDebugText()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < playedCards.Count; i++)
        {
            PlayingCard card = playedCards[i];

            if (card == null)
            {
                builder.AppendLine("Null card: +0 chips");
                continue;
            }

            int chipValue = cardChipValues.ContainsKey(card) ? cardChipValues[card] : 0;

            int singleScoreChipValue = GetSingleScoreChipValue(card);

            if (chipValue != singleScoreChipValue)
            {
                builder.AppendLine($"{card.GetDisplayName()}: +{chipValue} chips total (includes retriggered scoring)");
            }
            else if (card.IsStone)
            {
                int stoneChipValue = chipValue - card.permanentBonusChips;
                if (card.permanentBonusChips != 0)
                {
                    builder.AppendLine($"{card.GetDisplayName()}: stone {stoneChipValue} + permanent {FormatSignedNumber(card.permanentBonusChips)} = +{chipValue} chips");
                }
                else
                {
                    builder.AppendLine($"{card.GetDisplayName()}: stone chips +{chipValue}");
                }
            }
            else if (card.permanentBonusChips != 0)
            {
                int rankChipValue = chipValue - card.permanentBonusChips;
                builder.AppendLine($"{card.GetDisplayName()}: rank {rankChipValue} + permanent {FormatSignedNumber(card.permanentBonusChips)} = +{chipValue} chips");
            }
            else
            {
                builder.AppendLine($"{card.GetDisplayName()}: +{chipValue} chips");
            }
        }

        return builder.ToString();
    }

    private string FormatSignedNumber(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private int GetSingleScoreChipValue(PlayingCard card)
    {
        if (card.IsStone)
        {
            return 50 + card.permanentBonusChips;
        }

        return GetRankChipValue(card.rank) + card.permanentBonusChips;
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

    public string GetSuitPresenceDebugText()
    {
        StringBuilder builder = new StringBuilder();

        foreach (KeyValuePair<Suit, int> suitCount in suitCounts)
        {
            if (suitCount.Value > 0)
            {
                builder.AppendLine($"{suitCount.Key}: {suitCount.Value}");
            }
        }

        if (builder.Length == 0)
        {
            builder.AppendLine("No scoring suits");
        }

        return builder.ToString();
    }

    public string GetSuitEffectDebugText()
    {
        if (triggeredSuitEffectLog.Count == 0)
        {
            return "No suit effects applied";
        }

        return string.Join("\n", triggeredSuitEffectLog);
    }

    public string GetCardEffectDebugText()
    {
        if (triggeredCardEffectLog.Count == 0)
        {
            return "No card enhancement or seal effects triggered";
        }

        return string.Join("\n", triggeredCardEffectLog);
    }

    public string GetJokerEffectDebugText()
    {
        if (triggeredJokerEffectLog.Count == 0)
        {
            return "No Joker effects triggered";
        }

        return string.Join("\n", triggeredJokerEffectLog);
    }

    public void RecalculateFinalScore()
    {
        finalScore = (int)Math.Round(chips * mult);
    }
}
