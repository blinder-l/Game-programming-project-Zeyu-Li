using System;
using System.Collections.Generic;
using System.Text;

public class HandTypeLevelManager
{
    private readonly Dictionary<PokerHandType, int> handTypeLevels = new Dictionary<PokerHandType, int>();
    private readonly Dictionary<PokerHandType, HandTypeLevelDefinition> definitions = new Dictionary<PokerHandType, HandTypeLevelDefinition>();

    public HandTypeLevelManager()
    {
        InitializeDefinitions();

        foreach (PokerHandType handType in Enum.GetValues(typeof(PokerHandType)))
        {
            handTypeLevels[handType] = 1;
        }
    }

    public int GetLevel(PokerHandType handType)
    {
        return handTypeLevels.ContainsKey(handType) ? handTypeLevels[handType] : 1;
    }

    public int GetCurrentBaseChips(PokerHandType handType)
    {
        HandTypeLevelDefinition definition = GetDefinition(handType);
        return definition.levelOneChips + ((GetLevel(handType) - 1) * definition.chipsPerLevel);
    }

    public float GetCurrentBaseMult(PokerHandType handType)
    {
        HandTypeLevelDefinition definition = GetDefinition(handType);
        return definition.levelOneMult + ((GetLevel(handType) - 1) * definition.multPerLevel);
    }

    public void Upgrade(PokerHandType handType)
    {
        handTypeLevels[handType] = GetLevel(handType) + 1;
    }

    public string GetLevelDebugText()
    {
        StringBuilder builder = new StringBuilder();

        foreach (PokerHandType handType in Enum.GetValues(typeof(PokerHandType)))
        {
            builder.AppendLine($"{handType}: Lv{GetLevel(handType)} | {GetCurrentBaseChips(handType)} chips x {GetCurrentBaseMult(handType)} mult");
        }

        return builder.ToString();
    }

    private HandTypeLevelDefinition GetDefinition(PokerHandType handType)
    {
        return definitions.ContainsKey(handType) ? definitions[handType] : definitions[PokerHandType.HighCard];
    }

    private void InitializeDefinitions()
    {
        definitions[PokerHandType.HighCard] = new HandTypeLevelDefinition(5, 1f, 10, 1f);
        definitions[PokerHandType.Pair] = new HandTypeLevelDefinition(10, 2f, 15, 1f);
        definitions[PokerHandType.TwoPair] = new HandTypeLevelDefinition(20, 2f, 20, 1f);
        definitions[PokerHandType.ThreeOfAKind] = new HandTypeLevelDefinition(30, 3f, 20, 2f);
        definitions[PokerHandType.Straight] = new HandTypeLevelDefinition(30, 4f, 30, 3f);
        definitions[PokerHandType.Flush] = new HandTypeLevelDefinition(35, 4f, 15, 2f);
        definitions[PokerHandType.FullHouse] = new HandTypeLevelDefinition(40, 4f, 25, 2f);
        definitions[PokerHandType.FourOfAKind] = new HandTypeLevelDefinition(60, 7f, 30, 3f);
        definitions[PokerHandType.StraightFlush] = new HandTypeLevelDefinition(100, 8f, 40, 4f);
    }
}

public struct HandTypeLevelDefinition
{
    public readonly int levelOneChips;
    public readonly float levelOneMult;
    public readonly int chipsPerLevel;
    public readonly float multPerLevel;

    public HandTypeLevelDefinition(int levelOneChips, float levelOneMult, int chipsPerLevel, float multPerLevel)
    {
        this.levelOneChips = levelOneChips;
        this.levelOneMult = levelOneMult;
        this.chipsPerLevel = chipsPerLevel;
        this.multPerLevel = multPerLevel;
    }
}
