public class FortuneFamiliarJoker : JokerBase
{
    private float xMult = 1f;

    public override string Name => "Fortune Familiar";
    public override string Description => "Gains X0.25 Mult whenever a Lucky Card successfully triggers.";
    public override int Cost => 6;
    public override string CurrentEffectText => $"X{FormatMultiplier(xMult)} Mult";

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null)
        {
            return;
        }

        if (scoreContext.luckySuccessfulTriggerCount > 0)
        {
            xMult += 0.25f * scoreContext.luckySuccessfulTriggerCount;
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: Lucky triggered {scoreContext.luckySuccessfulTriggerCount} time(s), increased to X{FormatMultiplier(xMult)}");
        }

        scoreContext.mult *= xMult;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"*{FormatMultiplier(xMult)}", Name, null, 0, 0f, xMult));
        scoreContext.triggeredJokerEffectLog.Add($"{Name} applied: X{FormatMultiplier(xMult)} Mult");
    }

    private string FormatMultiplier(float value)
    {
        return value.ToString("0.##");
    }
}
