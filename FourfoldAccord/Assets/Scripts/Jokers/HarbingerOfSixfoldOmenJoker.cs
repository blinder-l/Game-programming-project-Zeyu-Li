public class HarbingerOfSixfoldOmenJoker : JokerBase
{
    public override string Name => "Harbinger of Sixfold Omen";
    public override string Description => "Doubles the numerator of listed chance effects.";
    public override int Cost => 4;

    public override void ApplyRuleModifiers(JokerRuleContext ruleContext)
    {
        if (ruleContext != null)
        {
            ruleContext.hasOopsAll6s = true;
        }
    }
}
