public class PlayingCard
{
    public int uniqueId;
    public Suit suit;
    public Rank rank;
    public bool isSelected;
    public int timesPlayed;
    public int timesScored;
    public int timesDiscarded;

    public PlayingCard(int uniqueId, Suit suit, Rank rank)
    {
        this.uniqueId = uniqueId;
        this.suit = suit;
        this.rank = rank;
        isSelected = false;
        timesPlayed = 0;
        timesScored = 0;
        timesDiscarded = 0;
    }

    // Returns a readable card name for console debugging.
    public string GetDisplayName()
    {
        return $"{rank} of {suit}";
    }

    public override string ToString()
    {
        return GetDisplayName();
    }
}
