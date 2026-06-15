public class BossBlindContext
{
    public BossBlindType Type { get; set; }
    public bool HasDebuffedSuit { get; set; }
    public Suit DebuffedSuit { get; set; }

    public bool IsActive => Type != BossBlindType.None;
    public bool IsFlint => Type == BossBlindType.Flint;

    public bool IsCardDebuffed(PlayingCard card)
    {
        return HasDebuffedSuit
            && card != null
            && card.HasSuit
            && card.suit == DebuffedSuit;
    }
}
