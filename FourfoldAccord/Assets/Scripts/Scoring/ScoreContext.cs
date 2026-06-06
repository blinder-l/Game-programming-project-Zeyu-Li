using System.Collections.Generic;
using System.Text;

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
    public Dictionary<Suit, int> suitCounts;
    public int goldReward;
    public List<string> triggeredSuitEffectLog;

    public ScoreContext(
        List<PlayingCard> playedCards,
        PokerHandType handType,
        int baseChips,
        int rankChips,
        int chips,
        float mult,
        int finalScore,
        Dictionary<PlayingCard, int> cardChipValues,
        Dictionary<Suit, int> suitCounts,
        int goldReward,
        List<string> triggeredSuitEffectLog)
    {
        this.playedCards = playedCards;
        this.handType = handType;
        this.baseChips = baseChips;
        this.rankChips = rankChips;
        this.chips = chips;
        this.mult = mult;
        this.finalScore = finalScore;
        this.cardChipValues = cardChipValues;
        this.suitCounts = suitCounts;
        this.goldReward = goldReward;
        this.triggeredSuitEffectLog = triggeredSuitEffectLog;
    }

    public string GetCardChipDebugText()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < playedCards.Count; i++)
        {
            PlayingCard card = playedCards[i];
            int chipValue = cardChipValues.ContainsKey(card) ? cardChipValues[card] : 0;
            builder.AppendLine($"{card.GetDisplayName()}: +{chipValue} chips");
        }

        return builder.ToString();
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
}
