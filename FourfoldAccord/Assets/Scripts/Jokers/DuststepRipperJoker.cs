using System.Collections.Generic;

public class DuststepRipperJoker : JokerBase
{
    public override string Name => "Duststep Ripper";
    public override string Description => "Scoring 2, 3, 4, and 5 cards retrigger 1 additional time.";
    public override int Cost => 6;

    public override CardRetriggerEffect GetCardRetriggerEffect(
        PlayingCard card,
        int scoringCardIndex,
        IReadOnlyList<PlayingCard> scoringCards,
        JokerRuleContext ruleContext,
        int jokerSlotIndex)
    {
        if (card == null || !card.HasRank || !IsLowRank(card.rank))
        {
            return null;
        }

        return new CardRetriggerEffect(1, jokerSlotIndex, "+1", Name);
    }

    private bool IsLowRank(Rank rank)
    {
        return rank == Rank.Two || rank == Rank.Three || rank == Rank.Four || rank == Rank.Five;
    }
}
