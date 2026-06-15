public class AugurOfConstellationsJoker : JokerBase
{
    private float xMult = 1f;

    public override string Name => "Augur of Constellations";
    public override string Description => "Gains X0.1 Mult whenever a Planet card is used.";
    public override int Cost => 6;
    public override string CurrentEffectText => $"X{FormatMultiplier(xMult)} Mult";

    public override void OnPlanetCardUsed(PlanetCard planetCard, PokerHandType targetHandType, int jokerSlotIndex)
    {
        float previous = xMult;
        xMult += 0.1f;
        UnityEngine.Debug.Log($"{Name}: planet used for {targetHandType}, grew from X{FormatMultiplier(previous)} to X{FormatMultiplier(xMult)}");
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
