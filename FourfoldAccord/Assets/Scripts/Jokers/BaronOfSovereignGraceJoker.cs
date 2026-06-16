public class BaronOfSovereignGraceJoker : JokerBase
{
    public override string Name => "Baron of Sovereign Grace";
    public override string Description => "Each King held in hand gives X1.5 Mult.";
    public override int Cost => 8;

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || scoreContext.heldCards == null)
        {
            return;
        }

        int heldKingCount = 0;

        for (int i = 0; i < scoreContext.heldCards.Count; i++)
        {
            PlayingCard card = scoreContext.heldCards[i];

            if (card != null && card.HasRank && card.rank == Rank.King)
            {
                heldKingCount++;
                scoreContext.triggeredJokerEffectLog.Add($"{Name}: held {card.GetDisplayName()} applied X1.5 Mult");
            }
        }

        if (heldKingCount == 0)
        {
            return;
        }

        float multiplier = 1f;

        for (int i = 0; i < heldKingCount; i++)
        {
            multiplier *= 1.5f;
        }

        scoreContext.mult *= multiplier;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"*{FormatMultiplier(multiplier)}", Name, null, 0, 0f, multiplier));
        scoreContext.triggeredJokerEffectLog.Add($"{Name}: {heldKingCount} held King(s), X{FormatMultiplier(multiplier)} Mult");
    }

    private string FormatMultiplier(float value)
    {
        return value.ToString("0.##");
    }
}
