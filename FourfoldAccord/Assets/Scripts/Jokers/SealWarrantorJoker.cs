using System;

public class SealWarrantorJoker : JokerBase
{
    private readonly Random random = new Random();

    public override string Name => "Seal Warrantor";
    public override string Description => "At the start of each Blind, add a random playing card with a random Seal to your deck.";
    public override int Cost => 6;

    public override void OnBlindStarted(JokerRuntimeContext runtimeContext)
    {
        if (runtimeContext == null || runtimeContext.deckManager == null)
        {
            return;
        }

        PlayingCard card = new PlayingCard(
            runtimeContext.deckManager.GetNextUniqueId(runtimeContext.handManager?.CurrentHand),
            GetRandomSuit(),
            GetRandomRank())
        {
            seal = GetRandomSeal()
        };

        if (runtimeContext.handManager == null || !runtimeContext.handManager.TryAddCardToHand(card))
        {
            runtimeContext.deckManager.AddNewOwnedCardToDrawPile(card);
        }

        runtimeContext.NotifyPlayingCardAdded(card, Name);
        UnityEngine.Debug.Log($"{Name}: added {card.GetDisplayName()} to deck and hand");
    }

    private Suit GetRandomSuit()
    {
        Array suits = Enum.GetValues(typeof(Suit));
        return (Suit)suits.GetValue(random.Next(suits.Length));
    }

    private Rank GetRandomRank()
    {
        Array ranks = Enum.GetValues(typeof(Rank));
        return (Rank)ranks.GetValue(random.Next(ranks.Length));
    }

    private CardSeal GetRandomSeal()
    {
        CardSeal[] seals =
        {
            CardSeal.Gold,
            CardSeal.Red,
            CardSeal.Blue,
            CardSeal.Purple
        };

        return seals[random.Next(seals.Length)];
    }
}
