using System;
using System.Collections.Generic;
using System.Text;

public class ShopManager
{
    public const int RerollCost = 2;

    public const int ShopOptionCount = 4;

    private readonly List<ShopOffer> currentOffers = new List<ShopOffer>();
    private readonly Random random = new Random();

    public IReadOnlyList<ShopOffer> CurrentOffers => currentOffers;
    public IReadOnlyList<ShopOffer> ShopOptions => currentOffers;

    public void GenerateOffers()
    {
        currentOffers.Clear();

        for (int i = 0; i < ShopOptionCount; i++)
        {
            currentOffers.Add(new ShopOffer(CreateRandomJoker()));
        }

        UnityEngine.Debug.Log("Generated 4 Joker offers.");
    }

    public void GenerateShopOptions()
    {
        GenerateOffers();
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
        newGold = currentGold;

        if (currentGold < RerollCost)
        {
            message = $"Cannot reroll: not enough gold. Cost {RerollCost}, current gold {currentGold}";
            return false;
        }

        newGold = currentGold - RerollCost;
        GenerateOffers();
        message = $"Shop rerolled. Cost {RerollCost}, remaining gold {newGold}";
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

        return builder.ToString();
    }

    private JokerBase CreateRandomJoker()
    {
        int jokerType = random.Next(6);

        switch (jokerType)
        {
            case 0:
                return new SuitRetriggerJoker(Suit.Hearts);
            case 1:
                return new SuitRetriggerJoker(Suit.Diamonds);
            case 2:
                return new SuitRetriggerJoker(Suit.Clubs);
            case 3:
                return new SuitRetriggerJoker(Suit.Spades);
            case 4:
                return new HighRiskMultiplierJoker();
            default:
                return new StoredDiscardMultiplierJoker();
        }
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
