public class PriestOfTheSupernovaRemnantJoker : JokerBase
{
    private int lastAppliedMult;

    public override string Name => "Priest of the Supernova Remnant";
    public override string Description => "Adds Mult equal to how many times this hand type has been played this run.";
    public override int Cost => 5;
    public override string CurrentEffectText => $"+{lastAppliedMult} Mult";

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null)
        {
            return;
        }

        lastAppliedMult = scoreContext.currentHandTypePlayCount;

        if (lastAppliedMult <= 0)
        {
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: no hand type count available");
            return;
        }

        scoreContext.mult += lastAppliedMult;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"+{lastAppliedMult}", Name, null, 0, lastAppliedMult));
        scoreContext.triggeredJokerEffectLog.Add($"{Name}: {scoreContext.handType} played {lastAppliedMult} time(s), +{lastAppliedMult} Mult");
    }
}
