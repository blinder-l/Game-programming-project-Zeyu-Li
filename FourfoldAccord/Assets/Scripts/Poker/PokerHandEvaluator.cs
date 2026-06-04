using System.Collections.Generic;

public class PokerHandEvaluator
{
    public PokerHandResult Evaluate(List<PlayingCard> playedCards)
    {
        if (playedCards == null)
        {
            return new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>());
        }

        return new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>(playedCards));
    }
}
