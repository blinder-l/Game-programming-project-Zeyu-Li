using System.Collections.Generic;

public class ScoreContext
{
    public List<PlayingCard> playedCards;
    public PokerHandType handType;
    public int chips;
    public float mult;
    public int finalScore;

    public ScoreContext(List<PlayingCard> playedCards, PokerHandType handType, int chips, float mult, int finalScore)
    {
        this.playedCards = playedCards;
        this.handType = handType;
        this.chips = chips;
        this.mult = mult;
        this.finalScore = finalScore;
    }
}
