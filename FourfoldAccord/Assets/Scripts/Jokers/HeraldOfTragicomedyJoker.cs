using System.Collections.Generic;

public class HeraldOfTragicomedyJoker : JokerBase
{
    public override string Name => "Herald of Tragicomedy";
    public override string Description => "Scoring face cards retrigger 1 additional time.";
    public override int Cost => 6;

    public override CardRetriggerEffect GetCardRetriggerEffect(
        PlayingCard card,
        int scoringCardIndex,
        IReadOnlyList<PlayingCard> scoringCards,
        JokerRuleContext ruleContext,
        int jokerSlotIndex)
    {
        if (card == null || !CardTraitUtility.IsFaceCardEffective(card, ruleContext))
        {
            return null;
        }

        return new CardRetriggerEffect(1, jokerSlotIndex, "+1", Name);
    }
}
