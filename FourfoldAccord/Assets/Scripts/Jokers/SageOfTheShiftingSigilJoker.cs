using System;
using System.Collections.Generic;

public class SageOfTheShiftingSigilJoker : JokerBase
{
    private readonly Random random = new Random();
    private Suit currentTargetSuit;
    private bool hasTargetSuit;

    public override string Name => "Sage of the Shifting Sigil";
    public override string Description => "A target suit changes each Blind. Scoring target-suit cards give X1.5 Mult.";
    public override int Cost => 8;
    public override string CurrentEffectText => hasTargetSuit ? $"Target: {currentTargetSuit}" : "Target: none";

    public override void OnBlindStarted(IReadOnlyList<PlayingCard> ownedCards)
    {
        Array suits = Enum.GetValues(typeof(Suit));
        currentTargetSuit = (Suit)suits.GetValue(random.Next(suits.Length));
        hasTargetSuit = true;
        UnityEngine.Debug.Log($"{Name} target suit this blind: {currentTargetSuit}");
    }

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null)
        {
            return;
        }

        EnsureTargetSuit();

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[i];
            PlayingCard card = scoreEvent != null ? scoreEvent.card : null;

            if (card == null || !CardTraitUtility.GetEffectiveSuits(card, scoreContext.ruleContext).Contains(currentTargetSuit))
            {
                continue;
            }

            scoreContext.mult *= 1.5f;
            scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, "*1.5", Name, scoreEvent, 0, 0f, 1.5f));
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: {card.GetDisplayName()} matched {currentTargetSuit}, X1.5 Mult");
        }
    }

    private void EnsureTargetSuit()
    {
        if (!hasTargetSuit)
        {
            OnBlindStarted(null);
        }
    }
}
