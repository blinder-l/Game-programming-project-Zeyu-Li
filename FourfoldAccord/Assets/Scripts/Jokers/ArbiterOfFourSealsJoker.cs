public class ArbiterOfFourSealsJoker : JokerBase
{
    public override string Name => "Arbiter of Four Seals";
    public override string Description => "Straights and Flushes can be made with 4 cards.";
    public override int Cost => 7;

    public override void ApplyRuleModifiers(JokerRuleContext ruleContext)
    {
        if (ruleContext != null)
        {
            ruleContext.hasFourFingers = true;
        }
    }
}
