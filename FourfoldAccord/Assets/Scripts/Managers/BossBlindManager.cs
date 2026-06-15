using System;
using System.Collections.Generic;
using UnityEngine;

public class BossBlindManager
{
    private readonly System.Random random = new System.Random();
    private readonly HashSet<PokerHandType> eyePlayedHandTypes = new HashSet<PokerHandType>();

    private int preparedBlindNumber = -1;

    public BossBlindDefinition CurrentDefinition { get; private set; } = GetNoneDefinition();
    public BossBlindType CurrentType => CurrentDefinition != null ? CurrentDefinition.Type : BossBlindType.None;
    public bool IsActive => CurrentType != BossBlindType.None;
    public string CurrentDisplayName => CurrentDefinition != null ? CurrentDefinition.DisplayName : string.Empty;
    public string CurrentRuleText => CurrentDefinition != null ? CurrentDefinition.RuleText : string.Empty;

    public void PrepareForBlind(int blindNumber, bool isBossBlind)
    {
        if (!isBossBlind)
        {
            Reset();
            preparedBlindNumber = blindNumber;
            return;
        }

        if (preparedBlindNumber == blindNumber && IsActive)
        {
            return;
        }

        CurrentDefinition = CreateRandomBossDefinition();
        preparedBlindNumber = blindNumber;
        eyePlayedHandTypes.Clear();
        Debug.Log($"Boss Active: {CurrentDefinition.DisplayName} - {CurrentDefinition.RuleText}");
    }

    public void StartPreparedBlind(int blindNumber, bool isBossBlind)
    {
        PrepareForBlind(blindNumber, isBossBlind);
        eyePlayedHandTypes.Clear();
    }

    public void Reset()
    {
        CurrentDefinition = GetNoneDefinition();
        eyePlayedHandTypes.Clear();
    }

    public int GetTargetScore(int baseTargetScore)
    {
        return baseTargetScore * Math.Max(1, CurrentDefinition.TargetScoreMultiplier);
    }

    public int GetStartingHands(int defaultHands)
    {
        return CurrentDefinition.StartingHandsOverride ?? defaultHands;
    }

    public int GetStartingDiscards(int defaultDiscards)
    {
        return CurrentDefinition.StartingDiscardsOverride ?? defaultDiscards;
    }

    public int GetHandSizeLimit(int defaultHandSize)
    {
        return Math.Max(1, defaultHandSize + CurrentDefinition.HandSizeAdjustment);
    }

    public BossBlindContext BuildContext()
    {
        return new BossBlindContext
        {
            Type = CurrentType,
            HasDebuffedSuit = CurrentDefinition.HasDebuffedSuit,
            DebuffedSuit = CurrentDefinition.DebuffedSuit
        };
    }

    public bool IsCardDebuffedByBoss(PlayingCard card)
    {
        return BuildContext().IsCardDebuffed(card);
    }

    public bool TryRecordEyeHandType(PokerHandType handType, out string message)
    {
        if (CurrentType != BossBlindType.Eye)
        {
            message = string.Empty;
            return true;
        }

        if (eyePlayedHandTypes.Contains(handType))
        {
            message = $"Boss Blocked Play: The Eye does not allow repeated hand type: {handType}";
            return false;
        }

        eyePlayedHandTypes.Add(handType);
        message = $"Boss Record: The Eye recorded hand type: {handType}";
        return true;
    }

    private BossBlindDefinition CreateRandomBossDefinition()
    {
        Array values = Enum.GetValues(typeof(BossBlindType));
        BossBlindType selectedType;

        do
        {
            selectedType = (BossBlindType)values.GetValue(random.Next(values.Length));
        }
        while (selectedType == BossBlindType.None);

        return GetDefinition(selectedType);
    }

    private static BossBlindDefinition GetDefinition(BossBlindType type)
    {
        switch (type)
        {
            case BossBlindType.Wall:
                return new BossBlindDefinition(type, "The Wall", "Target score x4.", 4);
            case BossBlindType.Needle:
                return new BossBlindDefinition(type, "The Needle", "Only one hand allowed.", 1, 1);
            case BossBlindType.Water:
                return new BossBlindDefinition(type, "The Water", "Discards set to 0.", 1, null, 0);
            case BossBlindType.Manacle:
                return new BossBlindDefinition(type, "The Manacle", "Hand size reduced by 1.", 1, null, null, -1);
            case BossBlindType.Psychic:
                return new BossBlindDefinition(type, "The Psychic", "Must play exactly 5 cards.");
            case BossBlindType.Flint:
                return new BossBlindDefinition(type, "The Flint", "Base chips and base mult halved.");
            case BossBlindType.Head:
                return new BossBlindDefinition(type, "The Head", "Hearts are debuffed.", 1, null, null, 0, true, Suit.Hearts);
            case BossBlindType.Goad:
                return new BossBlindDefinition(type, "The Goad", "Spades are debuffed.", 1, null, null, 0, true, Suit.Spades);
            case BossBlindType.Window:
                return new BossBlindDefinition(type, "The Window", "Diamonds are debuffed.", 1, null, null, 0, true, Suit.Diamonds);
            case BossBlindType.Club:
                return new BossBlindDefinition(type, "The Club", "Clubs are debuffed.", 1, null, null, 0, true, Suit.Clubs);
            case BossBlindType.Eye:
                return new BossBlindDefinition(type, "The Eye", "No repeated hand types this Blind.");
            default:
                return GetNoneDefinition();
        }
    }

    private static BossBlindDefinition GetNoneDefinition()
    {
        return new BossBlindDefinition(BossBlindType.None, string.Empty, string.Empty);
    }
}
