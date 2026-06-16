public class PhantasmalImprinterJoker : JokerBase
{
    private float xMult = 1f;

    public override string Name => "Phantasmal Imprinter";
    public override string Description => "Gains X0.25 Mult whenever a playing card is added to your deck.";
    public override int Cost => 7;
    public override string CurrentEffectText => $"X{FormatMultiplier(xMult)} Mult";

    public override void OnPlayingCardAddedToDeck(PlayingCard card, string source, int jokerSlotIndex)
    {
        float previous = xMult;
        xMult += 0.25f;
        UnityEngine.Debug.Log($"{Name}: card added by {source}, grew from X{FormatMultiplier(previous)} to X{FormatMultiplier(xMult)}");
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
