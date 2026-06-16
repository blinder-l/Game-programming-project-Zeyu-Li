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

    public virtual void OnBlindPassed(JokerRuntimeContext runtimeContext)
    {
    }

    public virtual void OnBlindStarted(System.Collections.Generic.IReadOnlyList<PlayingCard> ownedCards)
    {
    }

    public virtual void OnBlindStarted(JokerRuntimeContext runtimeContext)
    {
        OnBlindStarted(runtimeContext?.OwnedCards);
    }

    public virtual void OnAfterHandScored(ScoreContext scoreContext)
    {
    }

    public virtual void OnBeforeScore(JokerRuntimeContext runtimeContext, int jokerSlotIndex)
    {
    }

    public virtual void OnAfterCardsPlayedBeforeRefill(ScoreContext scoreContext, JokerRuntimeContext runtimeContext, int jokerSlotIndex)
    {
    }

    public virtual void OnPlayingCardAddedToDeck(PlayingCard card, string source, int jokerSlotIndex)
    {
    }

    public virtual void OnPlanetCardUsed(PlanetCard planetCard, PokerHandType targetHandType, int jokerSlotIndex)
    {
    }

    public virtual void OnJokerSold(JokerBase soldJoker, int jokerSlotIndex)
    {
    }

    public virtual void OnDiscardAction(JokerDiscardContext discardContext, int jokerSlotIndex)
    {
    }
}

public class JokerEffectContext
{
    public int ownedStoneCardCount;
}

public class JokerRuntimeContext
{
    public DeckManager deckManager;
    public HandManager handManager;
    public PokerHandEvaluator pokerHandEvaluator;
    public HandTypeLevelManager handTypeLevelManager;
    public JokerRuleContext ruleContext;
    public System.Collections.Generic.IReadOnlyList<PlayingCard> playedCardsSubmitted;
    public System.Collections.Generic.List<PlayingCard> cardsToAddToHandAfterPlayedCardsRemoved = new System.Collections.Generic.List<PlayingCard>();
    public System.Action<PlayingCard, string> notifyPlayingCardAdded;
    public System.Action<int> addGold;
    public bool isFirstPlayedHandThisBlind;
    public bool isBossBlind;
    public System.Collections.Generic.IReadOnlyList<PlayingCard> OwnedCards =>
        deckManager != null ? deckManager.GetAllOwnedCardsSnapshot(handManager?.CurrentHand) : null;

    public void NotifyPlayingCardAdded(PlayingCard card, string source)
    {
        notifyPlayingCardAdded?.Invoke(card, source);
    }

    public void AddGold(int amount)
    {
        addGold?.Invoke(amount);
    }
}

public class JokerDiscardContext
{
    public System.Collections.Generic.List<PlayingCard> selectedCards = new System.Collections.Generic.List<PlayingCard>();
    public System.Collections.Generic.List<PlayingCard> cardsToDiscard = new System.Collections.Generic.List<PlayingCard>();
    public System.Collections.Generic.List<PlayingCard> destroyedCards = new System.Collections.Generic.List<PlayingCard>();
    public bool isFirstDiscardThisBlind;
    public JokerRuleContext ruleContext;
    public PokerHandEvaluator pokerHandEvaluator;
    public HandTypeLevelManager handTypeLevelManager;
    public System.Action<int> addGold;

    public void AddGold(int amount)
    {
        addGold?.Invoke(amount);
    }
}
