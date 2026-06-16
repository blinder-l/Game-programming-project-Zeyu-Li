public class LittleCrownPageJoker : JokerBase
{
    private int bonusChips;

    public override string Name => "Little Crown Page";
    public override string Description => "Each scoring 2 permanently adds +8 Chips to this Joker.";
    public override int Cost => 8;
    public override string CurrentEffectText => $"+{bonusChips} Chips";

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null)
        {
            return;
        }

        CardScoreEvent firstGrowthEvent = null;

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[i];
            PlayingCard card = scoreEvent != null ? scoreEvent.card : null;

            if (card == null || !card.HasRank || card.rank != Rank.Two)
            {
                continue;
            }

            int previousBonus = bonusChips;
            bonusChips += 8;

            if (firstGrowthEvent == null)
            {
                firstGrowthEvent = scoreEvent;
            }

            scoreContext.triggeredJokerEffectLog.Add($"{Name}: {card.GetDisplayName()} scored, grew from +{previousBonus} to +{bonusChips} Chips");
        }

        if (bonusChips <= 0)
        {
            return;
        }

        scoreContext.chips += bonusChips;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"+{bonusChips}", Name, firstGrowthEvent, bonusChips));
        scoreContext.triggeredJokerEffectLog.Add($"{Name} applied: +{bonusChips} Chips");
    }
}
