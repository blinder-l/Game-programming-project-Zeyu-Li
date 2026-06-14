public class PlanetCard
{
    public const int DefaultCost = 3;

    public PlanetCardType planetType;
    public PokerHandType targetHandType;
    public int cost;

    public string Name => planetType.ToString();

    public string Description => $"Upgrade {targetHandType} by 1 level.";

    public PlanetCard(PlanetCardType planetType)
    {
        this.planetType = planetType;
        targetHandType = GetTargetHandType(planetType);
        cost = DefaultCost;
    }

    public static PokerHandType GetTargetHandType(PlanetCardType planetType)
    {
        switch (planetType)
        {
            case PlanetCardType.Mercury:
                return PokerHandType.Pair;
            case PlanetCardType.Venus:
                return PokerHandType.ThreeOfAKind;
            case PlanetCardType.Earth:
                return PokerHandType.FullHouse;
            case PlanetCardType.Mars:
                return PokerHandType.FourOfAKind;
            case PlanetCardType.Jupiter:
                return PokerHandType.Flush;
            case PlanetCardType.Saturn:
                return PokerHandType.Straight;
            case PlanetCardType.Uranus:
                return PokerHandType.TwoPair;
            case PlanetCardType.Neptune:
                return PokerHandType.StraightFlush;
            default:
                return PokerHandType.HighCard;
        }
    }
}
