public class BloodlineScribeJoker : JokerBase
{
    private bool hasTriggeredThisBlind;

    public override string Name => "Bloodline Scribe";
    public override string Description => "On the first hand of each Blind, if you play exactly 1 card, copy it into your deck and hand.";
    public override int Cost => 8;

    public override void OnBlindStarted(JokerRuntimeContext runtimeContext)
    {
        hasTriggeredThisBlind = false;
    }

    public override void OnBeforeScore(JokerRuntimeContext runtimeContext, int jokerSlotIndex)
    {
        if (hasTriggeredThisBlind || runtimeContext == null || !runtimeContext.isFirstPlayedHandThisBlind || runtimeContext.playedCardsSubmitted == null || runtimeContext.playedCardsSubmitted.Count != 1)
        {
            return;
        }

        PlayingCard sourceCard = runtimeContext.playedCardsSubmitted[0];

        if (sourceCard == null || runtimeContext.deckManager == null)
        {
            return;
        }

        hasTriggeredThisBlind = true;
        PlayingCard copiedCard = sourceCard.CloneCard(runtimeContext.deckManager.GetNextUniqueId(runtimeContext.handManager?.CurrentHand));

        runtimeContext.deckManager.AddNewOwnedCardToDrawPile(copiedCard);
        runtimeContext.cardsToAddToHandAfterPlayedCardsRemoved.Add(copiedCard);
        runtimeContext.NotifyPlayingCardAdded(copiedCard, Name);
        UnityEngine.Debug.Log($"{Name}: copied {sourceCard.GetDisplayName()} into deck and hand");
    }
}
