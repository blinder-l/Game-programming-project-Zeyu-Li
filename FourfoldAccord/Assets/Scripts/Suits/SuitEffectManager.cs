using System;
using System.Collections.Generic;
using System.Linq;

public class SuitEffectManager
{
    public void ApplyBaseSuitEffects(ScoreContext scoreContext)
    {
        ApplySuitEffects(scoreContext, null);
    }

    public void ApplySuitEffects(ScoreContext scoreContext, SuitMasteryManager suitMasteryManager)
    {
        if (scoreContext == null)
        {
            return;
        }

        ApplyHeartsEffect(scoreContext, suitMasteryManager);
        ApplySpadesEffect(scoreContext, suitMasteryManager);
        ApplyDiamondsEffect(scoreContext, suitMasteryManager);
        ApplyClubsEffect(scoreContext, suitMasteryManager);
        RecalculateFinalScore(scoreContext);
    }

    private void ApplyHeartsEffect(ScoreContext scoreContext, SuitMasteryManager suitMasteryManager)
    {
        int heartCount = GetSuitCount(scoreContext, Suit.Hearts);

        if (heartCount < 1)
        {
            return;
        }

        int level = GetSuitLevel(suitMasteryManager, Suit.Hearts);
        int chipBonus = GetHeartsChipBonus(level);

        scoreContext.chips += chipBonus;
        scoreContext.triggeredSuitEffectLog.Add($"Hearts Lv{level} triggered: +{chipBonus} chips");

        if (level >= 3 && heartCount >= 2)
        {
            scoreContext.mult += 0.5f;
            scoreContext.triggeredSuitEffectLog.Add("Hearts Lv3 bonus triggered: +0.5 mult for 2+ Hearts");
        }
    }

    private void ApplySpadesEffect(ScoreContext scoreContext, SuitMasteryManager suitMasteryManager)
    {
        List<PlayingCard> scoringSpades = GetScoringCardsOfSuit(scoreContext, Suit.Spades);

        if (scoringSpades.Count == 0)
        {
            return;
        }

        PlayingCard highestSpade = scoringSpades
            .OrderByDescending(card => card.rank)
            .First();

        int level = GetSuitLevel(suitMasteryManager, Suit.Spades);
        float multBonus = GetSpadesMultBonus(level);

        scoreContext.mult += multBonus;
        scoreContext.triggeredSuitEffectLog.Add($"Spades Lv{level} triggered by {highestSpade.GetDisplayName()}: +{multBonus} mult");

        if (level >= 3)
        {
            scoreContext.chips += 10;
            scoreContext.triggeredSuitEffectLog.Add("Spades Lv3 bonus triggered: +10 chips");
        }
    }

    private void ApplyDiamondsEffect(ScoreContext scoreContext, SuitMasteryManager suitMasteryManager)
    {
        int diamondCount = GetSuitCount(scoreContext, Suit.Diamonds);

        if (diamondCount < 1 || diamondCount > 2)
        {
            return;
        }

        int level = GetSuitLevel(suitMasteryManager, Suit.Diamonds);
        int goldBonus = GetDiamondsGoldBonus(level);

        scoreContext.goldReward += goldBonus;
        scoreContext.triggeredSuitEffectLog.Add($"Diamonds Lv{level} triggered: +{goldBonus} gold");
    }

    private void ApplyClubsEffect(ScoreContext scoreContext, SuitMasteryManager suitMasteryManager)
    {
        if (GetSuitCount(scoreContext, Suit.Clubs) < 1 || scoreContext.cardChipValues.Count == 0)
        {
            return;
        }

        KeyValuePair<PlayingCard, int> highestChipCard = scoreContext.cardChipValues
            .OrderByDescending(cardChipValue => cardChipValue.Value)
            .First();

        int level = GetSuitLevel(suitMasteryManager, Suit.Clubs);

        scoreContext.chips += highestChipCard.Value;
        scoreContext.triggeredSuitEffectLog.Add($"Clubs Lv{level} triggered: retriggered {highestChipCard.Key.GetDisplayName()} for +{highestChipCard.Value} chips");

        if (level >= 2)
        {
            scoreContext.chips += 5;
            scoreContext.triggeredSuitEffectLog.Add("Clubs Lv2 bonus triggered: +5 chips");
        }

        if (level >= 3)
        {
            scoreContext.mult += 0.5f;
            scoreContext.triggeredSuitEffectLog.Add("Clubs Lv3 bonus triggered: +0.5 mult");
        }
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

    private int GetSuitLevel(SuitMasteryManager suitMasteryManager, Suit suit)
    {
        if (suitMasteryManager == null)
        {
            return 0;
        }

        return suitMasteryManager.GetLevel(suit);
    }

    private int GetHeartsChipBonus(int level)
    {
        if (level >= 2)
        {
            return 20;
        }

        if (level >= 1)
        {
            return 15;
        }

        return 10;
    }

    private float GetSpadesMultBonus(int level)
    {
        if (level >= 2)
        {
            return 1.0f;
        }

        if (level >= 1)
        {
            return 0.8f;
        }

        return 0.5f;
    }

    private int GetDiamondsGoldBonus(int level)
    {
        if (level >= 3)
        {
            return 3;
        }

        if (level >= 1)
        {
            return 2;
        }

        return 1;
    }
}
