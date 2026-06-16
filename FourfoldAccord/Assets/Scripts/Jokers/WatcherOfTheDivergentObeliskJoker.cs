using System;
using System.Collections.Generic;

public class WatcherOfTheDivergentObeliskJoker : JokerBase
{
    private float xMult = 1f;

    public override string Name => "Watcher of the Divergent Obelisk";
    public override string Description => "Gains X0.2 Mult when the played hand type is not currently most played. Resets if it is.";
    public override int Cost => 8;
    public override string CurrentEffectText => $"X{FormatMultiplier(xMult)} Mult";

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null)
        {
            return;
        }

        int highestCount = GetHighestCount(scoreContext.handTypePlayCountsBeforeHand);
        int currentCount = GetCount(scoreContext.handTypePlayCountsBeforeHand, scoreContext.handType);

        if (highestCount > 0 && currentCount >= highestCount)
        {
            xMult = 1f;
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: {scoreContext.handType} was most played, reset to X1.00");
            return;
        }

        xMult += 0.2f;
        scoreContext.triggeredJokerEffectLog.Add($"{Name}: {scoreContext.handType} was not most played, grew to X{FormatMultiplier(xMult)}");

        if (xMult <= 1f)
        {
            return;
        }

        scoreContext.mult *= xMult;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"*{FormatMultiplier(xMult)}", Name, null, 0, 0f, xMult));
        scoreContext.triggeredJokerEffectLog.Add($"{Name} applied: X{FormatMultiplier(xMult)} Mult");
    }

    private int GetHighestCount(IReadOnlyDictionary<PokerHandType, int> playCounts)
    {
        int highestCount = 0;

        if (playCounts == null)
        {
            return highestCount;
        }

        foreach (KeyValuePair<PokerHandType, int> playCount in playCounts)
        {
            highestCount = Math.Max(highestCount, playCount.Value);
        }

        return highestCount;
    }

    private int GetCount(IReadOnlyDictionary<PokerHandType, int> playCounts, PokerHandType handType)
    {
        if (playCounts == null || !playCounts.ContainsKey(handType))
        {
            return 0;
        }

        return playCounts[handType];
    }

    private string FormatMultiplier(float value)
    {
        return value.ToString("0.##");
    }
}
