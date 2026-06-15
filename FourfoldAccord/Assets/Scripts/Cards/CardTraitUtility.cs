using System.Collections.Generic;

public static class CardTraitUtility
{
    public static bool IsFaceCardEffective(PlayingCard card, JokerRuleContext ruleContext)
    {
        if (card == null || !card.HasRank)
        {
            return false;
        }

        if (IsNaturalFaceCard(card))
        {
            return true;
        }

        return ruleContext != null && ruleContext.hasPareidolia;
    }

    public static bool IsNaturalFaceCard(PlayingCard card)
    {
        if (card == null || !card.HasRank)
        {
            return false;
        }

        return card.rank == Rank.Jack || card.rank == Rank.Queen || card.rank == Rank.King;
    }

    public static List<Suit> GetEffectiveSuits(PlayingCard card, JokerRuleContext ruleContext)
    {
        List<Suit> suits = new List<Suit>();

        if (card == null || !card.HasSuit)
        {
            return suits;
        }

        if (ruleContext != null && ruleContext.hasSmearedJoker)
        {
            if (card.suit == Suit.Hearts || card.suit == Suit.Diamonds)
            {
                suits.Add(Suit.Hearts);
                suits.Add(Suit.Diamonds);
            }
            else
            {
                suits.Add(Suit.Spades);
                suits.Add(Suit.Clubs);
            }

            return suits;
        }

        suits.Add(card.suit);
        return suits;
    }
}
