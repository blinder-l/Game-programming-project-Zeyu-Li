using System;
using System.Text;

public class PlayingCard
{
    public int uniqueId;
    public string instanceId;
    public Suit suit;
    public Rank rank;
    public bool isSelected;
    public int timesPlayed;
    public int timesScored;
    public int timesDiscarded;
    public CardEnhancement enhancement;
    public CardSeal seal;
    public CardEdition edition;
    public int permanentBonusChips;
    public bool isDebuffed;

    public bool IsStone => enhancement == CardEnhancement.Stone;
    public bool HasRank => !IsStone;
    public bool HasSuit => !IsStone;
    public bool HasEnhancement => enhancement != CardEnhancement.None;
    public bool HasSeal => seal != CardSeal.None;
    public bool HasEdition => edition != CardEdition.None;

    public PlayingCard(int uniqueId, Suit suit, Rank rank)
    {
        this.uniqueId = uniqueId;
        instanceId = Guid.NewGuid().ToString("N");
        this.suit = suit;
        this.rank = rank;
        isSelected = false;
        timesPlayed = 0;
        timesScored = 0;
        timesDiscarded = 0;
        enhancement = CardEnhancement.None;
        seal = CardSeal.None;
        edition = CardEdition.None;
        permanentBonusChips = 0;
        isDebuffed = false;
    }

    public PlayingCard CloneCard(int newUniqueId)
    {
        PlayingCard clonedCard = new PlayingCard(newUniqueId, suit, rank)
        {
            enhancement = enhancement,
            seal = seal,
            edition = edition,
            permanentBonusChips = permanentBonusChips
        };

        return clonedCard;
    }

    // Returns a readable card name for console debugging.
    public string GetDisplayName()
    {
        StringBuilder builder = new StringBuilder($"{rank} of {suit}");

        if (enhancement != CardEnhancement.None)
        {
            builder.Append($" [{enhancement}]");
        }

        if (seal != CardSeal.None)
        {
            builder.Append($" [{seal} Seal]");
        }

        if (edition != CardEdition.None)
        {
            builder.Append($" [{edition}]");
        }

        if (permanentBonusChips != 0)
        {
            builder.Append($" [{FormatSignedNumber(permanentBonusChips)} Chips]");
        }

        if (isDebuffed)
        {
            builder.Append(" [Debuffed]");
        }

        return builder.ToString();
    }

    public override string ToString()
    {
        return GetDisplayName();
    }

    private string FormatSignedNumber(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }
}
