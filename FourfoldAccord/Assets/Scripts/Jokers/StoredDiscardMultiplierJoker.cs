using System;

public class StoredDiscardMultiplierJoker : JokerBase
{
    private int storedMultiplier = 1;

    public override string Name => "Stored Discard Multiplier Joker";
    public override string Description => $"Mult x{storedMultiplier}; gains remaining discards when a Battle is passed.";
    public override int Cost => 7;

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null)
        {
            return;
        }

        scoreContext.mult *= storedMultiplier;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"*{storedMultiplier}", Name, null, 0, 0f, storedMultiplier));
        scoreContext.triggeredJokerEffectLog.Add($"{Name}: mult x{storedMultiplier}");
    }

    public override void OnBlindPassed(RoundManager roundManager)
    {
        if (roundManager == null)
        {
            return;
        }

        storedMultiplier += Math.Max(0, roundManager.discardsRemaining);
    }
}
