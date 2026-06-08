using System;
using System.Collections.Generic;
using System.Text;

public class ShopManager
{
    private const int ShopOptionCount = 3;

    private readonly List<JokerBase> shopOptions = new List<JokerBase>();
    private readonly Random random = new Random();

    public IReadOnlyList<JokerBase> ShopOptions => shopOptions;

    public void GenerateShopOptions()
    {
        shopOptions.Clear();

        for (int i = 0; i < ShopOptionCount; i++)
        {
            shopOptions.Add(CreateRandomJoker());
        }
    }

    public JokerBase GetOption(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= shopOptions.Count)
        {
            return null;
        }

        return shopOptions[optionIndex];
    }

    public void RemoveOption(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= shopOptions.Count)
        {
            return;
        }

        shopOptions.RemoveAt(optionIndex);
    }

    public string GetShopDebugText()
    {
        if (shopOptions.Count == 0)
        {
            return "No shop options";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < shopOptions.Count; i++)
        {
            JokerBase joker = shopOptions[i];
            builder.AppendLine($"{i + 1}. {joker.Name} (Cost: {joker.Cost}) - {joker.Description}");
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
