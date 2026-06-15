using System;
using System.Collections.Generic;

public class ScoreManager
{
    private readonly SuitEffectManager suitEffectManager = new SuitEffectManager();
    private readonly Random random = new Random();

    public ScoreContext CalculateScore(PokerHandResult pokerHandResult)
    {
        return CalculateScore(pokerHandResult, null, null);
    }

    public ScoreContext CalculateScore(PokerHandResult pokerHandResult, HandTypeLevelManager handTypeLevelManager)
    {
        return CalculateScore(pokerHandResult, null, null, handTypeLevelManager);
    }

    public ScoreContext CalculateScore(PokerHandResult pokerHandResult, SuitMasteryManager suitMasteryManager)
    {
        return CalculateScore(pokerHandResult, suitMasteryManager, null);
    }

    public ScoreContext CalculateScore(PokerHandResult pokerHandResult, SuitMasteryManager suitMasteryManager, JokerManager jokerManager)
    {
        return CalculateScore(pokerHandResult, suitMasteryManager, jokerManager, null);
    }

    public ScoreContext CalculateScore(
        PokerHandResult pokerHandResult,
        SuitMasteryManager suitMasteryManager,
        JokerManager jokerManager,
        HandTypeLevelManager handTypeLevelManager)
    {
        if (pokerHandResult == null)
        {
            pokerHandResult = new PokerHandResult(PokerHandType.HighCard, new List<PlayingCard>());
        }

        List<PlayingCard> scoringCards = pokerHandResult.scoringCards ?? new List<PlayingCard>();
        List<string> triggeredCardEffectLog = new List<string>();
        List<CardScoreEvent> cardScoreEvents = new List<CardScoreEvent>();
        int bonusCardGoldReward = 0;
        float cardEffectMultBonus = 0f;
        Dictionary<PlayingCard, int> cardChipValues = GetCardChipValues(
            scoringCards,
            triggeredCardEffectLog,
            cardScoreEvents,
            out bonusCardGoldReward,
            out cardEffectMultBonus);
        Dictionary<Suit, int> suitCounts = GetSuitCounts(scoringCards);
        int baseChips = GetBaseChips(pokerHandResult.handType, handTypeLevelManager);
        int rankChips = GetRankChips(cardChipValues);
        int chips = baseChips + rankChips;
        float mult = GetBaseMult(pokerHandResult.handType, handTypeLevelManager) + cardEffectMultBonus;

        ScoreContext scoreContext = new ScoreContext(
            new List<PlayingCard>(scoringCards),
            pokerHandResult.handType,
            baseChips,
            rankChips,
            chips,
            mult,
            0,
            cardChipValues,
            cardScoreEvents,
            suitCounts,
            0,
            bonusCardGoldReward,
            triggeredCardEffectLog,
            new List<string>(),
            new List<string>());

        suitEffectManager.ApplySuitEffects(scoreContext, suitMasteryManager);
        jokerManager?.ApplyScoreJokers(scoreContext);
        scoreContext.RecalculateFinalScore();

        return scoreContext;
    }

    private int GetBaseChips(PokerHandType handType, HandTypeLevelManager handTypeLevelManager)
    {
        if (handTypeLevelManager != null)
        {
            return handTypeLevelManager.GetCurrentBaseChips(handType);
        }

        return GetBaseChips(handType);
    }

    private int GetBaseChips(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.Pair:
                return 20;
            case PokerHandType.TwoPair:
                return 40;
            case PokerHandType.ThreeOfAKind:
                return 60;
            case PokerHandType.Straight:
                return 100;
            case PokerHandType.Flush:
                return 120;
            case PokerHandType.FullHouse:
                return 160;
            case PokerHandType.FourOfAKind:
                return 220;
            case PokerHandType.StraightFlush:
                return 300;
            default:
                return 10;
        }
    }

    private float GetBaseMult(PokerHandType handType, HandTypeLevelManager handTypeLevelManager)
    {
        if (handTypeLevelManager != null)
        {
            return handTypeLevelManager.GetCurrentBaseMult(handType);
        }

        return GetBaseMult(handType);
    }

    private float GetBaseMult(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.Pair:
                return 1.2f;
            case PokerHandType.TwoPair:
                return 1.5f;
            case PokerHandType.ThreeOfAKind:
                return 2.0f;
            case PokerHandType.Straight:
                return 2.0f;
            case PokerHandType.Flush:
                return 2.5f;
            case PokerHandType.FullHouse:
                return 3.0f;
            case PokerHandType.FourOfAKind:
                return 4.0f;
            case PokerHandType.StraightFlush:
                return 5.0f;
            default:
                return 1.0f;
        }
    }

    private Dictionary<PlayingCard, int> GetCardChipValues(
        List<PlayingCard> cards,
        List<string> triggeredCardEffectLog,
        List<CardScoreEvent> cardScoreEvents,
        out int bonusCardGoldReward,
        out float multBonus)
    {
        Dictionary<PlayingCard, int> cardChipValues = new Dictionary<PlayingCard, int>();
        bonusCardGoldReward = 0;
        multBonus = 0f;

        for (int i = 0; i < cards.Count; i++)
        {
            PlayingCard card = cards[i];

            if (card == null)
            {
                continue;
            }

            int cardBonusGoldReward;
            float cardMultBonus;
            int cardChipValue = ScoreSingleCard(
                card,
                true,
                false,
                triggeredCardEffectLog,
                cardScoreEvents,
                out cardBonusGoldReward,
                out cardMultBonus);

            if (cardChipValues.ContainsKey(card))
            {
                cardChipValues[card] += cardChipValue;
            }
            else
            {
                cardChipValues[card] = cardChipValue;
            }

            bonusCardGoldReward += cardBonusGoldReward;
            multBonus += cardMultBonus;
        }

        return cardChipValues;
    }

    private int ScoreSingleCard(
        PlayingCard card,
        bool allowRedSealRetrigger,
        bool isRetrigger,
        List<string> triggeredCardEffectLog,
        List<CardScoreEvent> cardScoreEvents,
        out int bonusCardGoldReward,
        out float multBonus)
    {
        bonusCardGoldReward = 0;
        multBonus = 0f;

        int cardChips;

        if (card.IsStone)
        {
            cardChips = 50 + card.permanentBonusChips;
            triggeredCardEffectLog.Add($"{card.GetDisplayName()}: stone chips 50{FormatPermanentBonus(card)} = {cardChips}");
        }
        else
        {
            int rankChipValue = GetRankChipValue(card.rank);
            cardChips = rankChipValue + card.permanentBonusChips;
            triggeredCardEffectLog.Add($"{card.GetDisplayName()}: rank chips {rankChipValue}{FormatPermanentBonus(card)} = {cardChips}");
        }

        cardScoreEvents.Add(new CardScoreEvent(card, cardChips, isRetrigger));

        if (card.enhancement == CardEnhancement.Lucky)
        {
            bool triggeredLuckyMult = RollChance(1, 5);
            bool triggeredLuckyGold = RollChance(1, 15);

            if (triggeredLuckyMult)
            {
                multBonus += 20f;
                triggeredCardEffectLog.Add($"{card.GetDisplayName()}: Lucky Mult triggered +20 Mult");
            }
            else
            {
                triggeredCardEffectLog.Add($"{card.GetDisplayName()}: Lucky Mult did not trigger");
            }

            if (triggeredLuckyGold)
            {
                bonusCardGoldReward += 20;
                triggeredCardEffectLog.Add($"{card.GetDisplayName()}: Lucky Money triggered +$20");
            }
            else
            {
                triggeredCardEffectLog.Add($"{card.GetDisplayName()}: Lucky Money did not trigger");
            }
        }

        if (card.seal == CardSeal.Gold)
        {
            bonusCardGoldReward += 3;
            triggeredCardEffectLog.Add($"{card.GetDisplayName()}: Gold Seal +$3");
        }

        if (card.seal == CardSeal.Red)
        {
            if (allowRedSealRetrigger)
            {
                triggeredCardEffectLog.Add($"{card.GetDisplayName()}: Red Seal retriggered once");

                int retriggerBonusGoldReward;
                float retriggerMultBonus;
                cardChips += ScoreSingleCard(
                    card,
                    false,
                    true,
                    triggeredCardEffectLog,
                    cardScoreEvents,
                    out retriggerBonusGoldReward,
                    out retriggerMultBonus);
                bonusCardGoldReward += retriggerBonusGoldReward;
                multBonus += retriggerMultBonus;
            }
            else
            {
                triggeredCardEffectLog.Add($"{card.GetDisplayName()}: Red Seal retrigger skipped during retrigger");
            }
        }

        return cardChips;
    }

    private bool RollChance(int numerator, int denominator)
    {
        if (numerator <= 0 || denominator <= 0)
        {
            return false;
        }

        if (numerator >= denominator)
        {
            return true;
        }

        return random.Next(denominator) < numerator;
    }

    private string FormatPermanentBonus(PlayingCard card)
    {
        if (card.permanentBonusChips == 0)
        {
            return string.Empty;
        }

        return $" + permanent {FormatSignedNumber(card.permanentBonusChips)}";
    }

    private string FormatSignedNumber(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private int GetRankChips(Dictionary<PlayingCard, int> cardChipValues)
    {
        int rankChips = 0;

        foreach (KeyValuePair<PlayingCard, int> cardChipValue in cardChipValues)
        {
            rankChips += cardChipValue.Value;
        }

        return rankChips;
    }

    private int GetRankChipValue(Rank rank)
    {
        switch (rank)
        {
            case Rank.Two:
                return 2;
            case Rank.Three:
                return 3;
            case Rank.Four:
                return 4;
            case Rank.Five:
                return 5;
            case Rank.Six:
                return 6;
            case Rank.Seven:
                return 7;
            case Rank.Eight:
                return 8;
            case Rank.Nine:
                return 9;
            case Rank.Ace:
                return 11;
            default:
                return 10;
        }
    }

    private Dictionary<Suit, int> GetSuitCounts(List<PlayingCard> cards)
    {
        Dictionary<Suit, int> suitCounts = new Dictionary<Suit, int>();

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            suitCounts[suit] = 0;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null || !cards[i].HasSuit)
            {
                continue;
            }

            suitCounts[cards[i].suit]++;
        }

        return suitCounts;
    }
}
