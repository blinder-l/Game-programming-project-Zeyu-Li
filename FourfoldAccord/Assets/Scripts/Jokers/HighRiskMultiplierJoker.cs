using UnityEngine;

public class HighRiskMultiplierJoker : JokerBase
{
    private const int BreakChanceDenominator = 20;

    public override string Name => "High Risk Multiplier Joker";
    public override string Description => "Mult x5 when scored, with a 1 in 20 chance to break after triggering.";
    public override int Cost => 8;

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null)
        {
            return;
        }

        scoreContext.mult *= 5.0f;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, "*5", Name, null, 0, 0f, 5f));
        scoreContext.triggeredJokerEffectLog.Add($"{Name}: mult x5");

        if (Random.Range(0, BreakChanceDenominator) == 0)
        {
            ShouldRemove = true;
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: broke after triggering");
        }
        else
        {
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: survived this trigger");
        }
    }
}
