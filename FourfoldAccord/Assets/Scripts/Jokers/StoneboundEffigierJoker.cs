public class StoneboundEffigierJoker : JokerBase
{
    public override string Name => "Stonebound Effigier";
    public override string Description => "Gives +25 Chips for each Stone Card in your deck.";
    public override int Cost => 6;

    public override string GetCurrentEffectText(JokerEffectContext context)
    {
        int stoneCount = context != null ? context.ownedStoneCardCount : 0;
        return $"+{stoneCount * 25} Chips";
    }

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null)
        {
            return;
        }

        int stoneCount = scoreContext.ownedStoneCardCount;
        int bonusChips = stoneCount * 25;

        if (bonusChips <= 0)
        {
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: no Stone Cards in deck");
            return;
        }

        scoreContext.chips += bonusChips;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"+{bonusChips}", Name, null, bonusChips));
        scoreContext.triggeredJokerEffectLog.Add($"{Name}: {stoneCount} Stone Cards in deck, +{bonusChips} Chips");
    }
}
