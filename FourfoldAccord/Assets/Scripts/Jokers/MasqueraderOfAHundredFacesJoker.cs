public class MasqueraderOfAHundredFacesJoker : JokerBase
{
    public override string Name => "Masquerader of a Hundred Faces";
    public override string Description => "All non-Stone cards count as face cards.";
    public override int Cost => 5;

    public override void ApplyRuleModifiers(JokerRuleContext ruleContext)
    {
        if (ruleContext != null)
        {
            ruleContext.hasPareidolia = true;
        }
    }
}
