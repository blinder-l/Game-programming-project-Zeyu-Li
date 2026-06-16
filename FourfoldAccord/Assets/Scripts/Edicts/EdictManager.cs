using System;
using System.Collections.Generic;

public class EdictManager
{
    private readonly List<EdictCard> purchasedEdicts = new List<EdictCard>();
    private readonly HashSet<EdictCardType> purchasedTypes = new HashSet<EdictCardType>();

    public IReadOnlyList<EdictCard> PurchasedEdicts => purchasedEdicts;
    public int PurchasedCount => purchasedEdicts.Count;
    public int TotalEdictCount => Enum.GetValues(typeof(EdictCardType)).Length;
    public bool HasAllEdictsPurchased => PurchasedCount >= TotalEdictCount;

    public int AdditionalHandsPerBlind => HasEdict(EdictCardType.Reserve) ? 1 : 0;
    public int AdditionalDiscardsPerBlind => HasEdict(EdictCardType.Spare) ? 1 : 0;
    public int ShopPriceDiscount => HasEdict(EdictCardType.Bargain) ? 1 : 0;
    public int InterestCapBonus => HasEdict(EdictCardType.Interest) ? 5 : 0;
    public int FixedBlindRewardBonus => HasEdict(EdictCardType.Spoils) ? 2 : 0;
    public int HandTypeBaseChipsBonus => HasEdict(EdictCardType.Doctrine) ? 10 : 0;

    public bool HasEdict(EdictCardType edictType)
    {
        return purchasedTypes.Contains(edictType);
    }

    public bool TryPurchase(EdictCard edictCard, out string message)
    {
        if (edictCard == null)
        {
            message = "Cannot purchase Edict: invalid Edict card.";
            return false;
        }

        if (HasEdict(edictCard.edictType))
        {
            message = $"Cannot purchase Edict: {edictCard.Name} is already purchased.";
            return false;
        }

        purchasedTypes.Add(edictCard.edictType);
        purchasedEdicts.Add(edictCard);
        message = $"Purchased Edict: {edictCard.Name}";
        return true;
    }

    public List<EdictCardType> GetUnpurchasedEdictTypes()
    {
        List<EdictCardType> availableTypes = new List<EdictCardType>();

        foreach (EdictCardType edictType in Enum.GetValues(typeof(EdictCardType)))
        {
            if (!HasEdict(edictType))
            {
                availableTypes.Add(edictType);
            }
        }

        return availableTypes;
    }
}
