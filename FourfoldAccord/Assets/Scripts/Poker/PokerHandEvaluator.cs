using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokerHandEvaluator
{
    public PokerHandResult Evaluate(List<PlayingCard> playedCards)
    {
        return Evaluate(playedCards, null);
    }

    public PokerHandResult Evaluate(List<PlayingCard> playedCards, JokerRuleContext ruleContext)
    {
        if (playedCards == null || playedCards.Count == 0)
        {
            return new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>());
        }

        List<PlayingCard> stoneCards = playedCards
            .Where(card => card != null && card.IsStone)
            .ToList();
        List<PlayingCard> rankSuitCards = playedCards
            .Where(card => card != null && !card.IsStone)
            .ToList();

        if (stoneCards.Count > 0)
        {
            Debug.Log($"Poker hand evaluation ignored {stoneCards.Count} Stone Card(s)");
        }

        PokerHandResult result = EvaluateRankSuitCards(rankSuitCards, ruleContext);
        AddStoneCardsToScoringCards(result, stoneCards);
        ApplySplashScoringCards(result, playedCards, ruleContext);
        return result;
    }

    private PokerHandResult EvaluateRankSuitCards(List<PlayingCard> playedCards, JokerRuleContext ruleContext)
    {
        if (playedCards == null || playedCards.Count == 0)
        {
            return new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>());
        }

        Dictionary<Rank, List<PlayingCard>> cardsByRank = GroupCardsByRank(playedCards);
        List<List<PlayingCard>> rankGroups = cardsByRank.Values
            .OrderByDescending(group => group.Count)
            .ThenByDescending(group => group[0].rank)
            .ToList();
        List<PlayingCard> straightFlushCards = GetStraightFlushCards(playedCards, ruleContext);
        List<PlayingCard> flushCards = GetFlushCards(playedCards, ruleContext);
        List<PlayingCard> straightCards = GetStraightCards(playedCards, ruleContext);

        if (straightFlushCards.Count > 0)
        {
            return new PokerHandResult(PokerHandType.StraightFlush, straightFlushCards);
        }

        List<PlayingCard> fourOfAKind = rankGroups.FirstOrDefault(group => group.Count == 4);
        if (fourOfAKind != null)
        {
            return new PokerHandResult(PokerHandType.FourOfAKind, new List<PlayingCard>(fourOfAKind));
        }

        List<PlayingCard> threeOfAKind = rankGroups.FirstOrDefault(group => group.Count == 3);
        List<PlayingCard> pair = rankGroups.FirstOrDefault(group => group.Count == 2);

        if (threeOfAKind != null && pair != null)
        {
            List<PlayingCard> fullHouseCards = new List<PlayingCard>();
            fullHouseCards.AddRange(threeOfAKind);
            fullHouseCards.AddRange(pair);
            return new PokerHandResult(PokerHandType.FullHouse, fullHouseCards);
        }

        if (flushCards.Count > 0)
        {
            return new PokerHandResult(PokerHandType.Flush, flushCards);
        }

        if (straightCards.Count > 0)
        {
            return new PokerHandResult(PokerHandType.Straight, straightCards);
        }

        if (threeOfAKind != null)
        {
            return new PokerHandResult(PokerHandType.ThreeOfAKind, new List<PlayingCard>(threeOfAKind));
        }

        List<List<PlayingCard>> pairs = rankGroups
            .Where(group => group.Count == 2)
            .ToList();

        if (pairs.Count >= 2)
        {
            List<PlayingCard> twoPairCards = new List<PlayingCard>();
            twoPairCards.AddRange(pairs[0]);
            twoPairCards.AddRange(pairs[1]);
            return new PokerHandResult(PokerHandType.TwoPair, twoPairCards);
        }

        if (pairs.Count == 1)
        {
            return new PokerHandResult(PokerHandType.Pair, new List<PlayingCard>(pairs[0]));
        }

        PlayingCard highestCard = playedCards
            .OrderByDescending(card => card.rank)
            .First();

        return new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard> { highestCard });
    }

    private void ApplySplashScoringCards(PokerHandResult result, List<PlayingCard> playedCards, JokerRuleContext ruleContext)
    {
        if (result == null || playedCards == null || ruleContext == null || !ruleContext.hasSplash)
        {
            return;
        }

        if (result.scoringCards == null)
        {
            result.scoringCards = new List<PlayingCard>();
        }

        AddUniqueCards(result.scoringCards, playedCards);
    }

    private void AddStoneCardsToScoringCards(PokerHandResult result, List<PlayingCard> stoneCards)
    {
        if (result == null || stoneCards == null || stoneCards.Count == 0)
        {
            return;
        }

        if (result.scoringCards == null)
        {
            result.scoringCards = new List<PlayingCard>();
        }

        AddUniqueCards(result.scoringCards, stoneCards);
    }

    private void AddUniqueCards(List<PlayingCard> targetCards, List<PlayingCard> cardsToAdd)
    {
        if (targetCards == null || cardsToAdd == null)
        {
            return;
        }

        HashSet<string> existingKeys = new HashSet<string>();

        for (int i = 0; i < targetCards.Count; i++)
        {
            existingKeys.Add(GetCardInstanceKey(targetCards[i]));
        }

        for (int i = 0; i < cardsToAdd.Count; i++)
        {
            PlayingCard card = cardsToAdd[i];

            if (card == null)
            {
                continue;
            }

            string key = GetCardInstanceKey(card);

            if (existingKeys.Contains(key))
            {
                continue;
            }

            targetCards.Add(card);
            existingKeys.Add(key);
        }
    }

    private string GetCardInstanceKey(PlayingCard card)
    {
        if (card == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(card.instanceId))
        {
            return card.instanceId;
        }

        return card.uniqueId.ToString();
    }

    private Dictionary<Rank, List<PlayingCard>> GroupCardsByRank(List<PlayingCard> cards)
    {
        Dictionary<Rank, List<PlayingCard>> cardsByRank = new Dictionary<Rank, List<PlayingCard>>();

        for (int i = 0; i < cards.Count; i++)
        {
            PlayingCard card = cards[i];

            if (!cardsByRank.ContainsKey(card.rank))
            {
                cardsByRank[card.rank] = new List<PlayingCard>();
            }

            cardsByRank[card.rank].Add(card);
        }

        return cardsByRank;
    }

    private List<PlayingCard> GetFlushCards(List<PlayingCard> cards, JokerRuleContext ruleContext)
    {
        int requiredCount = GetStraightOrFlushRequirement(ruleContext);

        if (cards.Count < requiredCount)
        {
            return new List<PlayingCard>();
        }

        List<PlayingCard> bestFlushCards = new List<PlayingCard>();

        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            List<PlayingCard> matchingCards = cards
                .Where(card => CardTraitUtility.GetEffectiveSuits(card, ruleContext).Contains(suit))
                .OrderByDescending(card => card.rank)
                .ToList();

            if (matchingCards.Count >= requiredCount && matchingCards.Count > bestFlushCards.Count)
            {
                bestFlushCards = matchingCards;
            }
        }

        return bestFlushCards.Count >= requiredCount ? bestFlushCards : new List<PlayingCard>();
    }

    private List<PlayingCard> GetStraightCards(List<PlayingCard> cards, JokerRuleContext ruleContext)
    {
        int requiredCount = GetStraightOrFlushRequirement(ruleContext);

        if (cards.Count < requiredCount)
        {
            return new List<PlayingCard>();
        }

        List<PlayingCard> distinctRankCards = cards
            .GroupBy(card => card.rank)
            .Select(group => group.OrderByDescending(card => card.suit).First())
            .OrderBy(card => (int)card.rank)
            .ToList();

        if (distinctRankCards.Count < requiredCount)
        {
            return new List<PlayingCard>();
        }

        List<PlayingCard> bestCards = FindBestStraightCards(distinctRankCards, requiredCount, ruleContext);

        if (requiredCount == 4)
        {
            List<PlayingCard> fiveCardStraight = FindBestStraightCards(distinctRankCards, 5, ruleContext);

            if (fiveCardStraight.Count == 5)
            {
                return fiveCardStraight;
            }
        }

        return bestCards;
    }

    private List<PlayingCard> GetStraightFlushCards(List<PlayingCard> cards, JokerRuleContext ruleContext)
    {
        int requiredCount = GetStraightOrFlushRequirement(ruleContext);

        if (cards.Count < requiredCount)
        {
            return new List<PlayingCard>();
        }

        List<PlayingCard> bestStraightFlushCards = new List<PlayingCard>();

        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            List<PlayingCard> matchingCards = cards
                .Where(card => CardTraitUtility.GetEffectiveSuits(card, ruleContext).Contains(suit))
                .ToList();
            List<PlayingCard> straightCards = GetStraightCards(matchingCards, ruleContext);

            if (straightCards.Count > bestStraightFlushCards.Count)
            {
                bestStraightFlushCards = straightCards;
            }
        }

        return bestStraightFlushCards.Count >= requiredCount ? bestStraightFlushCards : new List<PlayingCard>();
    }

    private List<PlayingCard> FindBestStraightCards(
        List<PlayingCard> distinctRankCards,
        int requiredCount,
        JokerRuleContext ruleContext)
    {
        if (distinctRankCards == null || distinctRankCards.Count < requiredCount)
        {
            return new List<PlayingCard>();
        }

        int allowedGap = ruleContext != null && ruleContext.hasShortcut ? 2 : 1;
        List<PlayingCard> bestCards = new List<PlayingCard>();

        for (int startIndex = 0; startIndex < distinctRankCards.Count; startIndex++)
        {
            List<PlayingCard> candidateCards = new List<PlayingCard> { distinctRankCards[startIndex] };
            int previousRankValue = (int)distinctRankCards[startIndex].rank;

            for (int nextIndex = startIndex + 1; nextIndex < distinctRankCards.Count; nextIndex++)
            {
                int nextRankValue = (int)distinctRankCards[nextIndex].rank;
                int gap = nextRankValue - previousRankValue;

                if (gap < 1)
                {
                    continue;
                }

                if (gap > allowedGap)
                {
                    break;
                }

                candidateCards.Add(distinctRankCards[nextIndex]);
                previousRankValue = nextRankValue;

                if (candidateCards.Count == requiredCount)
                {
                    break;
                }
            }

            if (candidateCards.Count == requiredCount &&
                (bestCards.Count == 0 || candidateCards.Last().rank > bestCards.Last().rank))
            {
                bestCards = candidateCards;
            }
        }

        return bestCards
            .OrderByDescending(card => card.rank)
            .ToList();
    }

    private int GetStraightOrFlushRequirement(JokerRuleContext ruleContext)
    {
        return ruleContext != null && ruleContext.hasFourFingers ? 4 : 5;
    }
}
