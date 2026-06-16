public class PathfinderOfTheHiddenRouteJoker : JokerBase
{
    public override string Name => "Pathfinder of the Hidden Route";
    public override string Description => "Straights may skip one rank between cards.";
    public override int Cost => 7;

    public override void ApplyRuleModifiers(JokerRuleContext ruleContext)
    {
        if (ruleContext != null)
        {
            ruleContext.hasShortcut = true;
        }
    }
}
