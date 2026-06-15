using System.Collections.Generic;

public class IconOfChosenFateJoker : JokerBase
{
    private Rank targetRank;
    private Suit targetSuit;
    private bool hasTargetCard;

    public override string Name => "Icon of Chosen Fate";
    public override string Description => "A target card changes each Blind. Scoring matching cards give X2 Mult.";
    public override int Cost => 6;
    public override string CurrentEffectText => hasTargetCard ? $"Target: {FormatRank(targetRank)} of {targetSuit}" : "Target: none";

    public override void OnBlindStarted(IReadOnlyList<PlayingCard> ownedCards)
    {
        List<PlayingCard> candidates = new List<PlayingCard>();

        if (ownedCards != null)
        {
            for (int i = 0; i < ownedCards.Count; i++)
            {
                PlayingCard card = ownedCards[i];

                if (card != null && card.HasRank && card.HasSuit)
                {
                    candidates.Add(card);
                }
            }
        }

        if (candidates.Count == 0)
        {
            hasTargetCard = false;
            UnityEngine.Debug.Log($"{Name}: no non-Stone owned cards available for target.");
            return;
        }

        PlayingCard targetCard = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        targetRank = targetCard.rank;
        targetSuit = targetCard.suit;
        hasTargetCard = true;
        UnityEngine.Debug.Log($"{Name} target this blind: {FormatRank(targetRank)} of {targetSuit}");
    }

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null)
        {
            return;
        }

        if (!hasTargetCard)
        {
            OnBlindStarted(scoreContext.playedCards);
        }

        if (!hasTargetCard)
        {
            return;
        }

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[i];
            PlayingCard card = scoreEvent != null ? scoreEvent.card : null;

            if (card == null ||
                scoreContext.IsCardDebuffedByBoss(card) ||
                !card.HasRank ||
                card.rank != targetRank ||
                !CardTraitUtility.GetEffectiveSuits(card, scoreContext.ruleContext).Contains(targetSuit))
            {
                continue;
            }

            scoreContext.mult *= 2f;
            scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, "*2", Name, scoreEvent, 0, 0f, 2f));
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: {card.GetDisplayName()} matched target {FormatRank(targetRank)} of {targetSuit}, X2 Mult");
        }
    }

    private string FormatRank(Rank rank)
    {
        return rank == Rank.Ace ? "Ace" : rank.ToString();
    }
}
