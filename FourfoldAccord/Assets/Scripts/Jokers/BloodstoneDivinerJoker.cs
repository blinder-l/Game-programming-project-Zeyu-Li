public class BloodstoneDivinerJoker : JokerBase
{
    public override string Name => "Bloodstone Diviner";
    public override string Description => "Scoring Hearts have a 1 in 3 chance to give X1.5 Mult.";
    public override int Cost => 7;

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null)
        {
            return;
        }

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[i];
            PlayingCard card = scoreEvent != null ? scoreEvent.card : null;

            if (card == null || !CardTraitUtility.GetEffectiveSuits(card, scoreContext.ruleContext).Contains(Suit.Hearts))
            {
                continue;
            }

            if (ProbabilityUtility.RollChance(1, 3, scoreContext.ruleContext, Name, scoreContext.triggeredJokerEffectLog.Add))
            {
                scoreContext.mult *= 1.5f;
                scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, "*1.5", Name, scoreEvent, 0, 0f, 1.5f));
                scoreContext.triggeredJokerEffectLog.Add($"{Name}: {card.GetDisplayName()} triggered X1.5 Mult");
            }
            else
            {
                scoreContext.triggeredJokerEffectLog.Add($"{Name}: {card.GetDisplayName()} did not trigger");
            }
        }
    }
}
