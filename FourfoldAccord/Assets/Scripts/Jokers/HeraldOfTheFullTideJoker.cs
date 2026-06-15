public class HeraldOfTheFullTideJoker : JokerBase
{
    public override string Name => "Herald of the Full Tide";
    public override string Description => "Every played card scores.";
    public override int Cost => 3;

    public override void ApplyRuleModifiers(JokerRuleContext ruleContext)
    {
        if (ruleContext != null)
        {
            ruleContext.hasSplash = true;
        }
    }
}
