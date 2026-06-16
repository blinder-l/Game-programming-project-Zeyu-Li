public class SigilSmearerJoker : JokerBase
{
    public override string Name => "Sigil Smearer";
    public override string Description => "Hearts and Diamonds share a suit. Spades and Clubs share a suit.";
    public override int Cost => 7;

    public override void ApplyRuleModifiers(JokerRuleContext ruleContext)
    {
        if (ruleContext != null)
        {
            ruleContext.hasSmearedJoker = true;
        }
    }
}
