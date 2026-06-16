public class EdictCard
{
    public EdictCardType edictType;
    public int cost;

    public string Name => GetName(edictType);
    public string Description => GetDescription(edictType);

    public EdictCard(EdictCardType edictType)
    {
        this.edictType = edictType;
        cost = GetCost(edictType);
    }

    public static string GetName(EdictCardType edictType)
    {
        switch (edictType)
        {
            case EdictCardType.Reserve:
                return "Reserve";
            case EdictCardType.Spare:
                return "Spare";
            case EdictCardType.Bargain:
                return "Bargain";
            case EdictCardType.Interest:
                return "Interest";
            case EdictCardType.Spoils:
                return "Spoils";
            case EdictCardType.Doctrine:
                return "Doctrine";
            default:
                return "Unknown Edict";
        }
    }

    public static string GetDescription(EdictCardType edictType)
    {
        switch (edictType)
        {
            case EdictCardType.Reserve:
                return "Each Battle starts with +1 hand.";
            case EdictCardType.Spare:
                return "Each Battle starts with +1 discard.";
            case EdictCardType.Bargain:
                return "Joker, Planet, and Spell shop prices are reduced by $1, to a minimum of $1.";
            case EdictCardType.Interest:
                return "CashOut interest cap is increased by $5.";
            case EdictCardType.Spoils:
                return "Base Battle reward is increased by $2.";
            case EdictCardType.Doctrine:
                return "All hand types gain +10 base Chips.";
            default:
                return "No effect.";
        }
    }

    public static int GetCost(EdictCardType edictType)
    {
        switch (edictType)
        {
            case EdictCardType.Spare:
            case EdictCardType.Bargain:
                return 8;
            case EdictCardType.Spoils:
                return 9;
            default:
                return 10;
        }
    }
}
