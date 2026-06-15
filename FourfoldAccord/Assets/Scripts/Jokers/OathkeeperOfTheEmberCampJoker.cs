public class OathkeeperOfTheEmberCampJoker : JokerBase
{
    private float xMult = 1f;

    public override string Name => "Oathkeeper of the Ember Camp";
    public override string Description => "Gains X0.25 Mult when a Joker is sold. Resets after a Boss Blind.";
    public override int Cost => 9;
    public override string CurrentEffectText => $"X{FormatMultiplier(xMult)} Mult";

    public override void OnJokerSold(JokerBase soldJoker, int jokerSlotIndex)
    {
        if (soldJoker == null)
        {
            return;
        }

        float previous = xMult;
        xMult += 0.25f;
        UnityEngine.Debug.Log($"{Name}: {soldJoker.Name} sold, grew from X{FormatMultiplier(previous)} to X{FormatMultiplier(xMult)}");
    }

    public override void OnBlindPassed(JokerRuntimeContext runtimeContext)
    {
        if (runtimeContext != null && runtimeContext.isBossBlind)
        {
            xMult = 1f;
            UnityEngine.Debug.Log($"{Name}: reset after Boss Blind");
        }
    }

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || xMult <= 1f)
        {
            return;
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
