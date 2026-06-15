public class SpellCard
{
    public SpellCardType spellType;
    public int cost;

    public string Name => GetName(spellType);
    public string Description => GetDescription(spellType);
    public bool RequiresSingleSelectedHandCard => RequiresSingleSelectedHandCardFor(spellType);

    public SpellCard(SpellCardType spellType)
    {
        this.spellType = spellType;
        cost = GetCost(spellType);
    }

    public static string GetName(SpellCardType spellType)
    {
        switch (spellType)
        {
            case SpellCardType.AuricCovenant:
                return "Auric Covenant";
            case SpellCardType.StoneboundOath:
                return "Stonebound Oath";
            case SpellCardType.FortuneInscription:
                return "Fortune Inscription";
            case SpellCardType.CrimsonSealRite:
                return "Crimson Seal Rite";
            case SpellCardType.GildedSealRite:
                return "Gilded Seal Rite";
            case SpellCardType.HermitsVault:
                return "Hermit’s Vault";
            case SpellCardType.GallowsOffering:
                return "Gallows Offering";
            case SpellCardType.AscendantBlessing:
                return "Ascendant Blessing";
            default:
                return "Unknown Spell";
        }
    }

    public static string GetDescription(SpellCardType spellType)
    {
        switch (spellType)
        {
            case SpellCardType.AuricCovenant:
                return "Set 1 selected hand card to Gold.";
            case SpellCardType.StoneboundOath:
                return "Set 1 selected hand card to Stone.";
            case SpellCardType.FortuneInscription:
                return "Set 1 selected hand card to Lucky.";
            case SpellCardType.CrimsonSealRite:
                return "Set 1 selected hand card's seal to Red.";
            case SpellCardType.GildedSealRite:
                return "Set 1 selected hand card's seal to Gold.";
            case SpellCardType.HermitsVault:
                return "Gain gold equal to your current gold, up to $20.";
            case SpellCardType.GallowsOffering:
                return "Destroy 1 selected hand card and gain $5.";
            case SpellCardType.AscendantBlessing:
                return "Upgrade the last played hand type by 1 level.";
            default:
                return "No effect.";
        }
    }

    public static int GetCost(SpellCardType spellType)
    {
        switch (spellType)
        {
            case SpellCardType.GallowsOffering:
                return 3;
            case SpellCardType.CrimsonSealRite:
            case SpellCardType.GildedSealRite:
                return 5;
            case SpellCardType.AscendantBlessing:
                return 6;
            default:
                return 4;
        }
    }

    private static bool RequiresSingleSelectedHandCardFor(SpellCardType spellType)
    {
        switch (spellType)
        {
            case SpellCardType.HermitsVault:
            case SpellCardType.AscendantBlessing:
                return false;
            default:
                return true;
        }
    }
}
