using System.Collections.Generic;

public class PokerHandResult
{
    public PokerHandType handType;
    public List<PlayingCard> scoringCards;

    public PokerHandResult(PokerHandType handType, List<PlayingCard> scoringCards)
    {
        this.handType = handType;
        this.scoringCards = scoringCards;
    }
}
