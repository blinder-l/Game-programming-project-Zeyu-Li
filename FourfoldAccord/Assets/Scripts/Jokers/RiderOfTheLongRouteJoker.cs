public class RiderOfTheLongRouteJoker : JokerBase
{
    private int currentBonusMult;

    public override string Name => "Rider of the Long Route";
    public override string Description => "Gains +1 Mult if no scoring face cards are played. Resets on scoring face cards.";
    public override int Cost => 6;
    public override string CurrentEffectText => $"+{currentBonusMult} Mult";

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || scoreContext.playedCards == null)
        {
            return;
        }

        bool hasFaceScoringCard = false;

        for (int i = 0; i < scoreContext.playedCards.Count; i++)
        {
            if (CardTraitUtility.IsFaceCardEffective(scoreContext.playedCards[i], scoreContext.ruleContext))
            {
                hasFaceScoringCard = true;
                break;
            }
        }

        if (hasFaceScoringCard)
        {
            currentBonusMult = 0;
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: scoring face card found, reset to +0 Mult");
            return;
        }

        currentBonusMult++;
        scoreContext.mult += currentBonusMult;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"+{currentBonusMult}", Name, null, 0, currentBonusMult));
        scoreContext.triggeredJokerEffectLog.Add($"{Name}: no scoring face cards, grew to +{currentBonusMult} Mult");
        scoreContext.triggeredJokerEffectLog.Add($"{Name} applied: +{currentBonusMult} Mult");
    }
}
