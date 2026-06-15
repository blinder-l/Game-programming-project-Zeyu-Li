public class BrokerOfForfeitJoker : JokerBase
{
    private bool hasCheckedFirstDiscardThisBlind;

    public override string Name => "Broker of Forfeit";
    public override string Description => "On the first discard of each Blind, if exactly 1 card is discarded, destroy it and gain $3.";
    public override int Cost => 6;

    public override void OnBlindStarted(JokerRuntimeContext runtimeContext)
    {
        hasCheckedFirstDiscardThisBlind = false;
    }

    public override void OnDiscardAction(JokerDiscardContext discardContext, int jokerSlotIndex)
    {
        if (discardContext == null || hasCheckedFirstDiscardThisBlind || !discardContext.isFirstDiscardThisBlind)
        {
            return;
        }

        hasCheckedFirstDiscardThisBlind = true;

        if (discardContext.selectedCards == null || discardContext.selectedCards.Count != 1)
        {
            UnityEngine.Debug.Log($"{Name}: first discard was not exactly 1 card, no effect this Blind.");
            return;
        }

        PlayingCard destroyedCard = discardContext.selectedCards[0];

        if (destroyedCard == null)
        {
            return;
        }

        discardContext.cardsToDiscard.Remove(destroyedCard);
        discardContext.destroyedCards.Add(destroyedCard);
        discardContext.AddGold(3);
        UnityEngine.Debug.Log($"{Name}: destroyed {destroyedCard.GetDisplayName()}, +$3");
    }
}
