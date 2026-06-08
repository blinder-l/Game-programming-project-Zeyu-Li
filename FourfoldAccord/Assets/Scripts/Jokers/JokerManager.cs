using System.Collections.Generic;
using System.Text;

public class JokerManager
{
    private readonly List<JokerBase> equippedJokers = new List<JokerBase>();

    public int MaxJokerSlots { get; } = 5;
    public IReadOnlyList<JokerBase> EquippedJokers => equippedJokers;

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
            joker.ApplyScoreEffect(scoreContext);

            if (joker.ShouldRemove)
            {
                scoreContext.triggeredJokerEffectLog.Add($"{joker.Name} was removed.");
                equippedJokers.RemoveAt(i);
                i--;
            }
        }
    }

    public void NotifyBlindPassed(RoundManager roundManager)
    {
        for (int i = 0; i < equippedJokers.Count; i++)
        {
            equippedJokers[i].OnBlindPassed(roundManager);
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
