using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokerHandEvaluator
{
    public PokerHandResult Evaluate(List<PlayingCard> playedCards)
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

        PokerHandResult result = EvaluateRankSuitCards(rankSuitCards);
        AddStoneCardsToScoringCards(result, stoneCards);
        return result;
    }

    private PokerHandResult EvaluateRankSuitCards(List<PlayingCard> playedCards)
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
        bool isFlush = IsFlush(playedCards);
        bool isStraight = IsStraight(playedCards);

        if (isFlush && isStraight)
        {
            return new PokerHandResult(PokerHandType.StraightFlush, new List<PlayingCard>(playedCards));
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

        if (isFlush)
        {
            return new PokerHandResult(PokerHandType.Flush, new List<PlayingCard>(playedCards));
        }

        if (isStraight)
        {
            return new PokerHandResult(PokerHandType.Straight, new List<PlayingCard>(playedCards));
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

        result.scoringCards.AddRange(stoneCards);
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

    private bool IsFlush(List<PlayingCard> cards)
    {
        if (cards.Count != 5)
        {
            return false;
        }

        Suit firstSuit = cards[0].suit;

        for (int i = 1; i < cards.Count; i++)
        {
            if (cards[i].suit != firstSuit)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsStraight(List<PlayingCard> cards)
    {
        if (cards.Count != 5)
        {
            return false;
        }

        List<int> rankValues = cards
            .Select(card => (int)card.rank)
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        if (rankValues.Count != 5)
        {
            return false;
        }

        return rankValues[4] - rankValues[0] == 4;
    }
}
