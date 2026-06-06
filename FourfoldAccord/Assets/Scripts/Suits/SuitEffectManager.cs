using System;
using System.Collections.Generic;
using System.Linq;

public class SuitEffectManager
{
    public void ApplyBaseSuitEffects(ScoreContext scoreContext)
    {
        if (scoreContext == null)
        {
            return;
        }

        ApplyHeartsEffect(scoreContext);
        ApplySpadesEffect(scoreContext);
        ApplyDiamondsEffect(scoreContext);
        ApplyClubsEffect(scoreContext);
        RecalculateFinalScore(scoreContext);
    }

    private void ApplyHeartsEffect(ScoreContext scoreContext)
    {
        if (GetSuitCount(scoreContext, Suit.Hearts) < 1)
        {
            return;
        }

        scoreContext.chips += 10;
        scoreContext.triggeredSuitEffectLog.Add("Hearts triggered: +10 chips");
    }

    private void ApplySpadesEffect(ScoreContext scoreContext)
    {
        List<PlayingCard> scoringSpades = GetScoringCardsOfSuit(scoreContext, Suit.Spades);

        if (scoringSpades.Count == 0)
        {
            return;
        }

        PlayingCard highestSpade = scoringSpades
            .OrderByDescending(card => card.rank)
            .First();

        scoreContext.mult += 0.5f;
        scoreContext.triggeredSuitEffectLog.Add($"Spades triggered by {highestSpade.GetDisplayName()}: +0.5 mult");
    }

    private void ApplyDiamondsEffect(ScoreContext scoreContext)
    {
        int diamondCount = GetSuitCount(scoreContext, Suit.Diamonds);

        if (diamondCount < 1 || diamondCount > 2)
        {
            return;
        }

        scoreContext.goldReward += 1;
        scoreContext.triggeredSuitEffectLog.Add("Diamonds triggered: +1 gold");
    }

    private void ApplyClubsEffect(ScoreContext scoreContext)
    {
        if (GetSuitCount(scoreContext, Suit.Clubs) < 1 || scoreContext.cardChipValues.Count == 0)
        {
            return;
        }

        KeyValuePair<PlayingCard, int> highestChipCard = scoreContext.cardChipValues
            .OrderByDescending(cardChipValue => cardChipValue.Value)
            .First();

        scoreContext.chips += highestChipCard.Value;
        scoreContext.triggeredSuitEffectLog.Add($"Clubs triggered: retriggered {highestChipCard.Key.GetDisplayName()} for +{highestChipCard.Value} chips");
    }

    private int GetSuitCount(ScoreContext scoreContext, Suit suit)
    {
        if (!scoreContext.suitCounts.ContainsKey(suit))
        {
            return 0;
        }

        return scoreContext.suitCounts[suit];
    }

    private List<PlayingCard> GetScoringCardsOfSuit(ScoreContext scoreContext, Suit suit)
    {
        List<PlayingCard> cardsOfSuit = new List<PlayingCard>();

        for (int i = 0; i < scoreContext.playedCards.Count; i++)
        {
            if (scoreContext.playedCards[i].suit == suit)
            {
                cardsOfSuit.Add(scoreContext.playedCards[i]);
            }
        }

        return cardsOfSuit;
    }

    private void RecalculateFinalScore(ScoreContext scoreContext)
    {
        scoreContext.finalScore = (int)Math.Round(scoreContext.chips * scoreContext.mult);
    }
}
