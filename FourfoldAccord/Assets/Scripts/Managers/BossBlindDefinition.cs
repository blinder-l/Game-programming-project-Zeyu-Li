public class BossBlindDefinition
{
    public BossBlindType Type { get; }
    public string DisplayName { get; }
    public string RuleText { get; }
    public int TargetScoreMultiplier { get; }
    public int? StartingHandsOverride { get; }
    public int? StartingDiscardsOverride { get; }
    public int HandSizeAdjustment { get; }
    public bool HasDebuffedSuit { get; }
    public Suit DebuffedSuit { get; }

    public BossBlindDefinition(
        BossBlindType type,
        string displayName,
        string ruleText,
        int targetScoreMultiplier = 1,
        int? startingHandsOverride = null,
        int? startingDiscardsOverride = null,
        int handSizeAdjustment = 0,
        bool hasDebuffedSuit = false,
        Suit debuffedSuit = Suit.Hearts)
    {
        Type = type;
        DisplayName = displayName;
        RuleText = ruleText;
        TargetScoreMultiplier = targetScoreMultiplier;
        StartingHandsOverride = startingHandsOverride;
        StartingDiscardsOverride = startingDiscardsOverride;
        HandSizeAdjustment = handSizeAdjustment;
        HasDebuffedSuit = hasDebuffedSuit;
        DebuffedSuit = debuffedSuit;
    }
}
