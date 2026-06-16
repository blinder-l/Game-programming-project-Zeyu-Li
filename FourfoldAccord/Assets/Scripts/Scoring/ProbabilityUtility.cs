using System;

public static class ProbabilityUtility
{
    private static readonly Random random = new Random();

    public static bool RollChance(int numerator, int denominator, JokerRuleContext ruleContext, string source, Action<string> log = null)
    {
        if (numerator <= 0 || denominator <= 0)
        {
            return false;
        }

        int adjustedNumerator = numerator;

        if (ruleContext != null && ruleContext.hasOopsAll6s)
        {
            adjustedNumerator = Math.Min(denominator, numerator * 2);
            log?.Invoke($"Harbinger of Sixfold Omen: chance doubled from {numerator}/{denominator} to {adjustedNumerator}/{denominator} for {source}");
        }

        if (adjustedNumerator >= denominator)
        {
            return true;
        }

        return random.Next(denominator) < adjustedNumerator;
    }
}
