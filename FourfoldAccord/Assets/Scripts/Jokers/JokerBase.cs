public abstract class JokerBase
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract int Cost { get; }
    public virtual string SpriteKey => Name;
    public virtual string CurrentEffectText => string.Empty;
    public virtual string GetCurrentEffectText(JokerEffectContext context)
    {
        return CurrentEffectText;
    }

    public bool ShouldRemove { get; protected set; }

    public virtual void ApplyRuleModifiers(JokerRuleContext ruleContext)
    {
    }

    public virtual CardRetriggerEffect GetCardRetriggerEffect(
        PlayingCard card,
        int scoringCardIndex,
        System.Collections.Generic.IReadOnlyList<PlayingCard> scoringCards,
        JokerRuleContext ruleContext,
        int jokerSlotIndex)
    {
        return null;
    }

    // Jokers should modify intermediate score fields, not finalScore.
    public virtual void ApplyScoreEffect(ScoreContext scoreContext)
    {
    }

    public virtual void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        ApplyScoreEffect(scoreContext);
    }

    public virtual void OnBlindPassed(RoundManager roundManager)
    {
    }

    public virtual void OnBlindStarted(System.Collections.Generic.IReadOnlyList<PlayingCard> ownedCards)
    {
    }

    public virtual void OnAfterHandScored(ScoreContext scoreContext)
    {
    }
}

public class JokerEffectContext
{
    public int ownedStoneCardCount;
}
