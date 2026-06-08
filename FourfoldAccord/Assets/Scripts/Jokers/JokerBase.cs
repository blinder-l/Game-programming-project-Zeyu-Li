public abstract class JokerBase
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract int Cost { get; }

    public bool ShouldRemove { get; protected set; }

    // Jokers should modify intermediate score fields, not finalScore.
    public virtual void ApplyScoreEffect(ScoreContext scoreContext)
    {
    }

    public virtual void OnBlindPassed(RoundManager roundManager)
    {
    }
}
