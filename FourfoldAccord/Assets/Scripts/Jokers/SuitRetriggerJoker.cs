using System.Collections.Generic;

public class SuitRetriggerJoker : JokerBase
{
    private readonly Suit targetSuit;

    public override string Name => $"{targetSuit} Retrigger Joker";
    public override string Description => $"Retriggers all scoring {targetSuit} cards for their rank chips.";
    public override int Cost => 5;

    public SuitRetriggerJoker(Suit targetSuit)
    {
        this.targetSuit = targetSuit;
    }

    public override void ApplyScoreEffect(ScoreContext scoreContext)
    {
        if (scoreContext == null || scoreContext.cardChipValues == null)
        {
            return;
        }

        int retriggeredCardCount = 0;
        int bonusChips = 0;

        foreach (KeyValuePair<PlayingCard, int> cardChipValue in scoreContext.cardChipValues)
        {
            if (!cardChipValue.Key.HasSuit || cardChipValue.Key.suit != targetSuit)
            {
                continue;
            }

            retriggeredCardCount++;
            bonusChips += cardChipValue.Value;
        }

        if (retriggeredCardCount == 0)
        {
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: no matching scoring cards");
            return;
        }

        scoreContext.chips += bonusChips;
        scoreContext.triggeredJokerEffectLog.Add($"{Name}: retriggered {retriggeredCardCount} card(s) for +{bonusChips} chips");
    }
}
