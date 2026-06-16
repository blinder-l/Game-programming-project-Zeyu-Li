public class JesterOfTheTwinCourtsJoker : JokerBase
{
    public override string Name => "Jester of the Twin Courts";
    public override string Description => "Each scoring King or Queen gives X2 Mult.";
    public override int Cost => 10;

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

            if (card == null || !card.HasRank || (card.rank != Rank.King && card.rank != Rank.Queen))
            {
                continue;
            }

            scoreContext.mult *= 2f;
            scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, "*2", Name, scoreEvent, 0, 0f, 2f));
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: {card.GetDisplayName()} scored, X2 Mult");
        }
    }
}
