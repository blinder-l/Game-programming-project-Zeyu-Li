using System.Collections.Generic;
using System.Linq;

public class PokerHandEvaluator
{
    public PokerHandResult Evaluate(List<PlayingCard> playedCards)
    {
        if (playedCards == null || playedCards.Count == 0)
        {
            return new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>());
        }

        Dictionary<Rank, List<PlayingCard>> cardsByRank = GroupCardsByRank(playedCards);
        List<List<PlayingCard>> rankGroups = cardsByRank.Values
            .OrderByDescending(group => group.Count)
            .ToList();

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

        return new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>(playedCards));
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
}
