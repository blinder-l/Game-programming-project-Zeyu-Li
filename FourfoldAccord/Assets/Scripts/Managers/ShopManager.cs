using System;
using System.Collections.Generic;
using System.Text;

public class ShopManager
{
    public const int RerollCost = 2;

    public const int ShopOptionCount = 4;
    public const int ConsumableOfferCount = 2;

    private readonly List<ShopOffer> currentOffers = new List<ShopOffer>();
    private readonly List<PlanetShopOffer> currentConsumableOffers = new List<PlanetShopOffer>();
    private readonly Random random = new Random();
    private readonly List<Func<JokerBase>> jokerFactories = new List<Func<JokerBase>>
    {
        () => new WaymarkPilgrimJoker(),
        () => new GildedMaskbearerJoker(),
        () => new StoneboundEffigierJoker(),
        () => new FortuneFamiliarJoker(),
        () => new ArbiterOfFourSealsJoker(),
        () => new PathfinderOfTheHiddenRouteJoker(),
        () => new HeraldOfTheFullTideJoker(),
        () => new MasqueraderOfAHundredFacesJoker(),
        () => new SigilSmearerJoker(),
        () => new WitnessOfTheFirstExposureJoker(),
        () => new RiderOfTheLongRouteJoker(),
        () => new PriestOfTheSupernovaRemnantJoker(),
        () => new FirstEchoHeraldJoker(),
        () => new HeraldOfTragicomedyJoker(),
        () => new DuststepRipperJoker(),
        () => new BaronOfSovereignGraceJoker(),
        () => new BloodstoneDivinerJoker(),
        () => new SageOfTheShiftingSigilJoker(),
        () => new IconOfChosenFateJoker(),
        () => new LittleCrownPageJoker(),
        () => new BloodlineScribeJoker(),
        () => new BrokerOfForfeitJoker(),
        () => new SealWarrantorJoker(),
        () => new PhantasmalImprinterJoker(),
        () => new RadianceVampireJoker(),
        () => new AugurOfConstellationsJoker(),
        () => new HarbingerOfSixfoldOmenJoker(),
        () => new AshenCodexBurnerJoker(),
        () => new WatcherOfTheDivergentObeliskJoker(),
        () => new OathkeeperOfTheEmberCampJoker(),
        () => new JesterOfTheTwinCourtsJoker()
    };

    public IReadOnlyList<ShopOffer> CurrentOffers => currentOffers;
    public IReadOnlyList<ShopOffer> ShopOptions => currentOffers;
    public IReadOnlyList<PlanetShopOffer> CurrentConsumableOffers => currentConsumableOffers;

    public void GenerateOffers()
    {
        GenerateOffers(null);
    }

    public void GenerateOffers(JokerManager jokerManager)
    {
        currentOffers.Clear();
        currentConsumableOffers.Clear();

        List<Func<JokerBase>> availableFactories = GetAvailableJokerFactories(jokerManager);

        for (int i = 0; i < ShopOptionCount && availableFactories.Count > 0; i++)
        {
            int factoryIndex = random.Next(availableFactories.Count);
            currentOffers.Add(new ShopOffer(availableFactories[factoryIndex]()));
            availableFactories.RemoveAt(factoryIndex);
        }

        for (int i = 0; i < ConsumableOfferCount; i++)
        {
            currentConsumableOffers.Add(new PlanetShopOffer(CreateRandomPlanetCard()));
        }

        UnityEngine.Debug.Log("Generated 4 Joker offers.");
        UnityEngine.Debug.Log("Generated 2 Planet offers.");
    }

    public void GenerateShopOptions()
    {
        GenerateOffers();
    }

    public void GenerateShopOptions(JokerManager jokerManager)
    {
        GenerateOffers(jokerManager);
    }

    public JokerBase GetOption(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= currentOffers.Count || currentOffers[optionIndex].IsSold)
        {
            return null;
        }

        return currentOffers[optionIndex].Joker;
    }

    public void RemoveOption(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= currentOffers.Count)
        {
            return;
        }

        currentOffers[optionIndex].MarkSold();
    }

    public bool TryPurchaseOffer(int index, int currentGold, JokerManager jokerManager, out string message, out int newGold)
    {
        newGold = currentGold;

        if (index < 0 || index >= currentOffers.Count)
        {
            message = "Cannot purchase: invalid offer index";
            return false;
        }

        ShopOffer offer = currentOffers[index];

        if (offer == null || offer.Joker == null)
        {
            message = "Cannot purchase: invalid offer index";
            return false;
        }

        if (offer.IsSold)
        {
            message = "Cannot purchase: offer already sold";
            return false;
        }

        if (jokerManager == null)
        {
            message = "Cannot purchase: JokerManager is null";
            return false;
        }

        if (jokerManager.EquippedJokers.Count >= jokerManager.MaxJokerSlots)
        {
            message = "Cannot purchase: Joker slots full";
            return false;
        }

        if (HasEquippedJoker(jokerManager, offer.Joker.Name))
        {
            message = $"Cannot purchase: {offer.Joker.Name} is already equipped";
            return false;
        }

        if (currentGold < offer.Joker.Cost)
        {
            message = $"Cannot purchase: not enough gold. Cost {offer.Joker.Cost}, current gold {currentGold}";
            return false;
        }

        if (!jokerManager.TryEquipJoker(offer.Joker))
        {
            message = "Cannot purchase: Joker slots full";
            return false;
        }

        newGold = currentGold - offer.Joker.Cost;
        offer.MarkSold();
        message = $"Purchased Joker: {offer.Joker.Name}, cost {offer.Joker.Cost}, remaining gold {newGold}";
        return true;
    }

    public bool TryReroll(int currentGold, out string message, out int newGold)
    {
        return TryReroll(currentGold, null, out message, out newGold);
    }

    public bool TryReroll(int currentGold, JokerManager jokerManager, out string message, out int newGold)
    {
        newGold = currentGold;

        if (currentGold < RerollCost)
        {
            message = $"Cannot reroll: not enough gold. Cost {RerollCost}, current gold {currentGold}";
            return false;
        }

        newGold = currentGold - RerollCost;
        GenerateOffers(jokerManager);
        message = $"Shop rerolled. Cost {RerollCost}, remaining gold {newGold}";
        return true;
    }

    public bool TryPurchasePlanetOffer(int index, int currentGold, HandTypeLevelManager handTypeLevelManager, out string message, out int newGold)
    {
        newGold = currentGold;

        if (index < 0 || index >= currentConsumableOffers.Count)
        {
            message = "Cannot purchase Planet: invalid offer index";
            return false;
        }

        PlanetShopOffer offer = currentConsumableOffers[index];

        if (offer == null || offer.PlanetCard == null)
        {
            message = "Cannot purchase Planet: invalid offer index";
            return false;
        }

        if (offer.IsSold)
        {
            message = "Cannot purchase Planet: offer already sold";
            return false;
        }

        if (handTypeLevelManager == null)
        {
            message = "Cannot purchase Planet: HandTypeLevelManager is null";
            return false;
        }

        if (currentGold < offer.PlanetCard.cost)
        {
            message = $"Cannot purchase Planet: not enough gold. Cost {offer.PlanetCard.cost}, current gold {currentGold}";
            return false;
        }

        handTypeLevelManager.Upgrade(offer.PlanetCard.targetHandType);
        newGold = currentGold - offer.PlanetCard.cost;
        offer.MarkSold();
        message = $"Purchased Planet: {offer.PlanetCard.Name}, upgraded {offer.PlanetCard.targetHandType} to Lv {handTypeLevelManager.GetLevel(offer.PlanetCard.targetHandType)}, remaining gold {newGold}";
        return true;
    }

    public string GetShopDebugText()
    {
        if (currentOffers.Count == 0)
        {
            return "No shop options";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < currentOffers.Count; i++)
        {
            ShopOffer offer = currentOffers[i];
            string status = offer.IsSold ? "Sold" : "Available";
            JokerBase joker = offer.Joker;
            builder.AppendLine($"{i + 1}. {joker.Name} (Cost: {joker.Cost}) - {status} - {joker.Description}");
        }

        if (currentConsumableOffers.Count > 0)
        {
            builder.AppendLine("Consumables:");

            for (int i = 0; i < currentConsumableOffers.Count; i++)
            {
                PlanetShopOffer offer = currentConsumableOffers[i];
                string status = offer.IsSold ? "Sold" : "Available";
                PlanetCard planetCard = offer.PlanetCard;
                builder.AppendLine($"{i + 1}. {planetCard.Name} (Cost: {planetCard.cost}) - {status} - {planetCard.Description}");
            }
        }

        return builder.ToString();
    }

    private JokerBase CreateRandomJoker()
    {
        int jokerType = random.Next(jokerFactories.Count);
        return jokerFactories[jokerType]();
    }

    private List<Func<JokerBase>> GetAvailableJokerFactories(JokerManager jokerManager)
    {
        List<Func<JokerBase>> availableFactories = new List<Func<JokerBase>>();

        for (int i = 0; i < jokerFactories.Count; i++)
        {
            JokerBase previewJoker = jokerFactories[i]();

            if (previewJoker == null || HasEquippedJoker(jokerManager, previewJoker.Name))
            {
                continue;
            }

            availableFactories.Add(jokerFactories[i]);
        }

        return availableFactories;
    }

    private bool HasEquippedJoker(JokerManager jokerManager, string jokerName)
    {
        if (jokerManager == null || string.IsNullOrEmpty(jokerName))
        {
            return false;
        }

        IReadOnlyList<JokerBase> equippedJokers = jokerManager.EquippedJokers;

        for (int i = 0; i < equippedJokers.Count; i++)
        {
            if (equippedJokers[i] != null && string.Equals(equippedJokers[i].Name, jokerName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private PlanetCard CreateRandomPlanetCard()
    {
        Array planetTypes = Enum.GetValues(typeof(PlanetCardType));
        PlanetCardType planetType = (PlanetCardType)planetTypes.GetValue(random.Next(planetTypes.Length));
        return new PlanetCard(planetType);
    }
}

public class ShopOffer
{
    public JokerBase Joker { get; }
    public bool IsSold { get; private set; }

    public ShopOffer(JokerBase joker)
    {
        Joker = joker;
    }

    public void MarkSold()
    {
        IsSold = true;
    }
}

public class PlanetShopOffer
{
    public PlanetCard PlanetCard { get; }
    public bool IsSold { get; private set; }

    public PlanetShopOffer(PlanetCard planetCard)
    {
        PlanetCard = planetCard;
    }

    public void MarkSold()
    {
        IsSold = true;
    }
}
