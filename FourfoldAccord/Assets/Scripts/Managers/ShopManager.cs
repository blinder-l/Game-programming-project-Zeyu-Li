using System;
using System.Collections.Generic;
using System.Text;

public class ShopManager
{
    public const int RerollCost = 2;

    public const int ShopOptionCount = 4;
    public const int ConsumableOfferCount = 2;

    private readonly List<ShopOffer> currentOffers = new List<ShopOffer>();
    private readonly List<ConsumableShopOffer> currentConsumableOffers = new List<ConsumableShopOffer>();
    private readonly Random random = new Random();
    private EdictShopOffer currentEdictOffer;
    private int shopPriceDiscount;
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
    public IReadOnlyList<ConsumableShopOffer> CurrentConsumableOffers => currentConsumableOffers;
    public EdictShopOffer CurrentEdictOffer => currentEdictOffer;

    public void SetShopPriceDiscount(int discount)
    {
        shopPriceDiscount = Math.Max(0, discount);
    }

    public int GetDiscountedShopPrice(int baseCost)
    {
        return Math.Max(1, baseCost - shopPriceDiscount);
    }

    public void GenerateOffers()
    {
        GenerateOffers(null, null, true);
    }

    public void GenerateOffers(JokerManager jokerManager)
    {
        GenerateOffers(jokerManager, null, true);
    }

    public void GenerateOffers(JokerManager jokerManager, EdictManager edictManager)
    {
        GenerateOffers(jokerManager, edictManager, true);
    }

    public void GenerateOffers(JokerManager jokerManager, EdictManager edictManager, bool refreshEdictOffer)
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
            currentConsumableOffers.Add(CreateRandomConsumableOffer());
        }

        if (refreshEdictOffer)
        {
            currentEdictOffer = CreateRandomEdictOffer(edictManager);
        }

        UnityEngine.Debug.Log("Generated 4 Joker offers.");
        UnityEngine.Debug.Log("Generated 2 Consumable offers.");
        UnityEngine.Debug.Log(currentEdictOffer != null && currentEdictOffer.EdictCard != null
            ? $"Generated Edict offer: {currentEdictOffer.EdictCard.Name}"
            : "No Edict offer generated.");
    }

    public void GenerateOffers(JokerManager jokerManager, EdictManager edictManager, bool refreshEdictOffer, int discount)
    {
        SetShopPriceDiscount(discount);
        GenerateOffers(jokerManager, edictManager, refreshEdictOffer);
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

        int cost = GetDiscountedShopPrice(offer.Joker.Cost);

        if (currentGold < cost)
        {
            message = $"Cannot purchase: not enough gold. Cost {cost}, current gold {currentGold}";
            return false;
        }

        if (!jokerManager.TryEquipJoker(offer.Joker))
        {
            message = "Cannot purchase: Joker slots full";
            return false;
        }

        newGold = currentGold - cost;
        offer.MarkSold();
        message = $"Purchased Joker: {offer.Joker.Name}, cost {cost}, remaining gold {newGold}";
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
        GenerateOffers(jokerManager, null, true);
        message = $"Shop rerolled. Cost {RerollCost}, remaining gold {newGold}";
        return true;
    }

    public bool TryReroll(int currentGold, JokerManager jokerManager, EdictManager edictManager, out string message, out int newGold)
    {
        newGold = currentGold;

        if (currentGold < RerollCost)
        {
            message = $"Cannot reroll: not enough gold. Cost {RerollCost}, current gold {currentGold}";
            return false;
        }

        newGold = currentGold - RerollCost;
        GenerateOffers(jokerManager, edictManager, false);
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

        ConsumableShopOffer offer = currentConsumableOffers[index];

        if (offer == null || !offer.IsPlanet || offer.PlanetCard == null)
        {
            message = "Cannot purchase Planet: selected consumable is not a Planet card";
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

        int cost = GetDiscountedShopPrice(offer.PlanetCard.cost);

        if (currentGold < cost)
        {
            message = $"Cannot purchase Planet: not enough gold. Cost {cost}, current gold {currentGold}";
            return false;
        }

        handTypeLevelManager.Upgrade(offer.PlanetCard.targetHandType);
        newGold = currentGold - cost;
        offer.MarkSold();
        message = $"Purchased Planet: {offer.PlanetCard.Name}, cost {cost}, upgraded {offer.PlanetCard.targetHandType} to Lv {handTypeLevelManager.GetLevel(offer.PlanetCard.targetHandType)}, remaining gold {newGold}";
        return true;
    }

    public bool TryPurchaseSpellOffer(
        int index,
        int currentGold,
        bool hasFreeConsumableSlot,
        out SpellCard spellCard,
        out string message,
        out int newGold)
    {
        spellCard = null;
        newGold = currentGold;

        if (index < 0 || index >= currentConsumableOffers.Count)
        {
            message = "Cannot purchase Spell: invalid offer index";
            return false;
        }

        ConsumableShopOffer offer = currentConsumableOffers[index];

        if (offer == null || !offer.IsSpell || offer.SpellCard == null)
        {
            message = "Cannot purchase Spell: selected consumable is not a Spell card";
            return false;
        }

        if (offer.IsSold)
        {
            message = "Cannot purchase Spell: offer already sold";
            return false;
        }

        if (!hasFreeConsumableSlot)
        {
            message = "Cannot buy spell card: consumable slots are full.";
            return false;
        }

        int cost = GetDiscountedShopPrice(offer.SpellCard.cost);

        if (currentGold < cost)
        {
            message = $"Cannot purchase Spell: not enough gold. Cost {cost}, current gold {currentGold}";
            return false;
        }

        spellCard = offer.SpellCard;
        newGold = currentGold - cost;
        offer.MarkSold();
        message = $"Purchased Spell: {spellCard.Name}, cost {cost}, remaining gold {newGold}";
        return true;
    }

    public bool TryPurchaseEdictOffer(
        int currentGold,
        EdictManager edictManager,
        out EdictCard edictCard,
        out string message,
        out int newGold)
    {
        edictCard = null;
        newGold = currentGold;

        if (currentEdictOffer == null || currentEdictOffer.EdictCard == null)
        {
            message = "Cannot purchase Edict: no Edict offer available.";
            return false;
        }

        if (currentEdictOffer.IsSold)
        {
            message = "Cannot purchase Edict: offer already sold.";
            return false;
        }

        if (edictManager == null)
        {
            message = "Cannot purchase Edict: EdictManager is null.";
            return false;
        }

        if (edictManager.HasEdict(currentEdictOffer.EdictCard.edictType))
        {
            message = $"Cannot purchase Edict: {currentEdictOffer.EdictCard.Name} is already purchased.";
            return false;
        }

        int cost = currentEdictOffer.EdictCard.cost;

        if (currentGold < cost)
        {
            message = $"Cannot purchase Edict: not enough gold. Cost {cost}, current gold {currentGold}";
            return false;
        }

        if (!edictManager.TryPurchase(currentEdictOffer.EdictCard, out string purchaseMessage))
        {
            message = purchaseMessage;
            return false;
        }

        edictCard = currentEdictOffer.EdictCard;
        newGold = currentGold - cost;
        currentEdictOffer.MarkSold();
        message = $"{purchaseMessage}, cost {cost}, remaining gold {newGold}";
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
            builder.AppendLine($"{i + 1}. {joker.Name} (Cost: {offer.GetCost(shopPriceDiscount)}) - {status} - {joker.Description}");
        }

        if (currentConsumableOffers.Count > 0)
        {
            builder.AppendLine("Consumables:");

            for (int i = 0; i < currentConsumableOffers.Count; i++)
            {
                ConsumableShopOffer offer = currentConsumableOffers[i];
                string status = offer.IsSold ? "Sold" : "Available";

                if (offer.IsPlanet)
                {
                    PlanetCard planetCard = offer.PlanetCard;
                    builder.AppendLine($"{i + 1}. {planetCard.Name} (Cost: {offer.GetCost(shopPriceDiscount)}) - {status} - {planetCard.Description}");
                }
                else if (offer.IsSpell)
                {
                    SpellCard spellCard = offer.SpellCard;
                    builder.AppendLine($"{i + 1}. {spellCard.Name} (Cost: {offer.GetCost(shopPriceDiscount)}) - {status} - {spellCard.Description}");
                }
            }
        }

        if (currentEdictOffer != null && currentEdictOffer.EdictCard != null)
        {
            string status = currentEdictOffer.IsSold ? "Sold" : "Available";
            EdictCard edictCard = currentEdictOffer.EdictCard;
            builder.AppendLine($"Edict: {edictCard.Name} (Cost: {edictCard.cost}) - {status} - {edictCard.Description}");
        }
        else
        {
            builder.AppendLine("Edict: none available");
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

    private ConsumableShopOffer CreateRandomConsumableOffer()
    {
        Array planetTypes = Enum.GetValues(typeof(PlanetCardType));
        Array spellTypes = Enum.GetValues(typeof(SpellCardType));
        int totalTypes = planetTypes.Length + spellTypes.Length;
        int randomIndex = random.Next(totalTypes);

        if (randomIndex < planetTypes.Length)
        {
            PlanetCardType planetType = (PlanetCardType)planetTypes.GetValue(randomIndex);
            return new ConsumableShopOffer(new PlanetCard(planetType));
        }

        SpellCardType spellType = (SpellCardType)spellTypes.GetValue(randomIndex - planetTypes.Length);
        return new ConsumableShopOffer(new SpellCard(spellType));
    }

    private EdictShopOffer CreateRandomEdictOffer(EdictManager edictManager)
    {
        if (edictManager == null || edictManager.HasAllEdictsPurchased)
        {
            return null;
        }

        List<EdictCardType> availableTypes = edictManager.GetUnpurchasedEdictTypes();

        if (availableTypes.Count == 0)
        {
            return null;
        }

        EdictCardType edictType = availableTypes[random.Next(availableTypes.Count)];
        return new EdictShopOffer(new EdictCard(edictType));
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

    public int GetCost(int discount)
    {
        return Joker != null ? Math.Max(1, Joker.Cost - Math.Max(0, discount)) : 0;
    }
}

public class ConsumableShopOffer
{
    public PlanetCard PlanetCard { get; }
    public SpellCard SpellCard { get; }
    public bool IsSold { get; private set; }
    public bool IsPlanet => PlanetCard != null;
    public bool IsSpell => SpellCard != null;

    public ConsumableShopOffer(PlanetCard planetCard)
    {
        PlanetCard = planetCard;
    }

    public ConsumableShopOffer(SpellCard spellCard)
    {
        SpellCard = spellCard;
    }

    public void MarkSold()
    {
        IsSold = true;
    }

    public int GetCost(int discount)
    {
        int baseCost = 0;

        if (IsPlanet)
        {
            baseCost = PlanetCard.cost;
        }
        else if (IsSpell)
        {
            baseCost = SpellCard.cost;
        }

        return Math.Max(1, baseCost - Math.Max(0, discount));
    }
}

public class EdictShopOffer
{
    public EdictCard EdictCard { get; }
    public bool IsSold { get; private set; }

    public EdictShopOffer(EdictCard edictCard)
    {
        EdictCard = edictCard;
    }

    public void MarkSold()
    {
        IsSold = true;
    }
}
