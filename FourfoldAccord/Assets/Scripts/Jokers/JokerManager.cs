using System.Collections.Generic;
using System.Text;

public class JokerManager
{
    private readonly List<JokerBase> equippedJokers = new List<JokerBase>();

    public int MaxJokerSlots { get; } = 5;
    public IReadOnlyList<JokerBase> EquippedJokers => equippedJokers;

    public JokerRuleContext BuildRuleContext()
    {
        JokerRuleContext ruleContext = new JokerRuleContext();

        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i]?.ApplyRuleModifiers(ruleContext);
        }

        return ruleContext;
    }

    public bool TryEquipJoker(JokerBase joker)
    {
        if (joker == null || equippedJokers.Count >= MaxJokerSlots)
        {
            return false;
        }

        equippedJokers.Add(joker);
        return true;
    }

    public void RemoveJoker(JokerBase joker)
    {
        if (joker == null)
        {
            return;
        }

        equippedJokers.Remove(joker);
    }

    public bool TrySellJokerAt(int index, out JokerBase soldJoker)
    {
        soldJoker = null;

        if (index < 0 || index >= equippedJokers.Count)
        {
            return false;
        }

        soldJoker = equippedJokers[index];

        if (soldJoker == null)
        {
            return false;
        }

        equippedJokers.RemoveAt(index);
        return true;
    }

    public void ApplyScoreJokers(ScoreContext scoreContext)
    {
        if (scoreContext == null)
        {
            return;
        }

        for (int i = 0; i < equippedJokers.Count; i++)
        {
            JokerBase joker = equippedJokers[i];
            scoreContext.triggeredJokerEffectLog.Add($"Joker {i + 1}: {joker.Name}");
            joker.ApplyScoreEffect(scoreContext, i);

            if (joker.ShouldRemove)
            {
                scoreContext.triggeredJokerEffectLog.Add($"{joker.Name} was removed.");
                equippedJokers.RemoveAt(i);
                i--;
            }
        }
    }

    public List<CardRetriggerEffect> GetCardRetriggerEffects(
        PlayingCard card,
        int scoringCardIndex,
        IReadOnlyList<PlayingCard> scoringCards,
        JokerRuleContext ruleContext)
    {
        List<CardRetriggerEffect> retriggerEffects = new List<CardRetriggerEffect>();

        for (int i = 0; i < equippedJokers.Count; i++)
        {
            CardRetriggerEffect effect = equippedJokers[i]?.GetCardRetriggerEffect(
                card,
                scoringCardIndex,
                scoringCards,
                ruleContext,
                i);

            if (effect != null && effect.extraTriggerCount > 0)
            {
                retriggerEffects.Add(effect);
            }
        }

        return retriggerEffects;
    }

    public void NotifyBlindStarted(IReadOnlyList<PlayingCard> ownedCards)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnBlindStarted(ownedCards);
        }
    }

    public void NotifyBlindStarted(JokerRuntimeContext runtimeContext)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnBlindStarted(runtimeContext);
        }
    }

    public void NotifyBlindPassed(RoundManager roundManager)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnBlindPassed(roundManager);
        }
    }

    public void NotifyBlindPassed(JokerRuntimeContext runtimeContext)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnBlindPassed(runtimeContext);
        }
    }

    public void NotifyHandScored(ScoreContext scoreContext)
    {
        if (scoreContext == null)
        {
            return;
        }

        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnAfterHandScored(scoreContext);
        }
    }

    public void NotifyCardsPlayedBeforeRefill(ScoreContext scoreContext, JokerRuntimeContext runtimeContext)
    {
        if (scoreContext == null)
        {
            return;
        }

        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnAfterCardsPlayedBeforeRefill(scoreContext, runtimeContext, i);
        }
    }

    public void NotifyBeforeScore(JokerRuntimeContext runtimeContext)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnBeforeScore(runtimeContext, i);
        }
    }

    public void NotifyPlayingCardAdded(PlayingCard card, string source)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnPlayingCardAddedToDeck(card, source, i);
        }
    }

    public void NotifyPlanetCardUsed(PlanetCard planetCard, PokerHandType targetHandType)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnPlanetCardUsed(planetCard, targetHandType, i);
        }
    }

    public void NotifyJokerSold(JokerBase soldJoker)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnJokerSold(soldJoker, i);
        }
    }

    public void NotifyDiscardAction(JokerDiscardContext discardContext)
    {
        if (discardContext == null)
        {
            return;
        }

        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnDiscardAction(discardContext, i);
        }
    }

    public string GetJokerListDebugText()
    {
        if (equippedJokers.Count == 0)
        {
            return "No Jokers equipped";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < equippedJokers.Count; i++)
        {
            JokerBase joker = equippedJokers[i];
            builder.AppendLine($"{i + 1}. {joker.Name} - {joker.Description}");
        }

        return builder.ToString();
    }
}
