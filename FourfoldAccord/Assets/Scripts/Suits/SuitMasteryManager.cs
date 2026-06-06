using System;
using System.Collections.Generic;
using System.Text;

public class SuitMasteryManager
{
    private readonly Dictionary<Suit, int> suitXp = new Dictionary<Suit, int>();
    private readonly Dictionary<Suit, int> suitLevels = new Dictionary<Suit, int>();

    public SuitMasteryManager()
    {
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            suitXp[suit] = 0;
            suitLevels[suit] = 0;
        }
    }

    public List<Suit> AddXpForScoringSuits(Dictionary<Suit, int> suitCounts)
    {
        List<Suit> gainedXpSuits = new List<Suit>();

        foreach (KeyValuePair<Suit, int> suitCount in suitCounts)
        {
            if (suitCount.Value <= 0)
            {
                continue;
            }

            suitXp[suitCount.Key]++;
            suitLevels[suitCount.Key] = CalculateLevel(suitXp[suitCount.Key]);
            gainedXpSuits.Add(suitCount.Key);
        }

        return gainedXpSuits;
    }

    public int GetXp(Suit suit)
    {
        return suitXp[suit];
    }

    public int GetLevel(Suit suit)
    {
        return suitLevels[suit];
    }

    public string GetMasteryDebugText()
    {
        StringBuilder builder = new StringBuilder();

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            builder.AppendLine($"{suit}: Lv{GetLevel(suit)} ({GetXp(suit)} XP)");
        }

        return builder.ToString();
    }

    public string GetXpGainDebugText(List<Suit> gainedXpSuits)
    {
        if (gainedXpSuits.Count == 0)
        {
            return "No suit mastery XP gained";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < gainedXpSuits.Count; i++)
        {
            Suit suit = gainedXpSuits[i];
            builder.AppendLine($"{suit}: +1 XP -> Lv{GetLevel(suit)} ({GetXp(suit)} XP)");
        }

        return builder.ToString();
    }

    private int CalculateLevel(int xp)
    {
        if (xp >= 10)
        {
            return 3;
        }

        if (xp >= 6)
        {
            return 2;
        }

        if (xp >= 3)
        {
            return 1;
        }

        return 0;
    }
}
