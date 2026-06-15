using System.Collections.Generic;

public class FirstEchoHeraldJoker : JokerBase
{
    public override string Name => "First Echo Herald";
    public override string Description => "The first scoring card retriggers 2 additional times.";
    public override int Cost => 4;

    public override CardRetriggerEffect GetCardRetriggerEffect(
        PlayingCard card,
        int scoringCardIndex,
        IReadOnlyList<PlayingCard> scoringCards,
        JokerRuleContext ruleContext,
        int jokerSlotIndex)
    {
        if (card == null || scoringCardIndex != 0)
        {
            return null;
        }

        return new CardRetriggerEffect(2, jokerSlotIndex, "+2", Name);
    }
}
