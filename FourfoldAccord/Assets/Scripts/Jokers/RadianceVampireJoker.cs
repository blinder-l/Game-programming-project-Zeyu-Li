using System.Collections.Generic;

public class RadianceVampireJoker : JokerBase
{
    private float xMult = 1f;

    public override string Name => "Radiance Vampire";
    public override string Description => "Absorbs Enhancements from scoring cards and gains X0.1 Mult for each.";
    public override int Cost => 7;
    public override string CurrentEffectText => $"X{FormatMultiplier(xMult)} Mult";

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null)
        {
            return;
        }

        HashSet<string> absorbedInstanceIds = new HashSet<string>();

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            PlayingCard card = scoreContext.cardScoreEvents[i]?.card;

            if (card == null || card.enhancement == CardEnhancement.None)
            {
                continue;
            }

            string instanceKey = GetInstanceKey(card);

            if (absorbedInstanceIds.Contains(instanceKey))
            {
                continue;
            }

            absorbedInstanceIds.Add(instanceKey);
            CardEnhancement absorbedEnhancement = card.enhancement;
            card.enhancement = CardEnhancement.None;
            float previous = xMult;
            xMult += 0.1f;
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: absorbed {absorbedEnhancement} from {card.GetDisplayName()}, grew from X{FormatMultiplier(previous)} to X{FormatMultiplier(xMult)}");
        }

        if (xMult <= 1f)
        {
            return;
        }

        scoreContext.mult *= xMult;
        scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(jokerSlotIndex, $"*{FormatMultiplier(xMult)}", Name, null, 0, 0f, xMult));
        scoreContext.triggeredJokerEffectLog.Add($"{Name} applied: X{FormatMultiplier(xMult)} Mult");
    }

    private string GetInstanceKey(PlayingCard card)
    {
        return !string.IsNullOrEmpty(card.instanceId) ? card.instanceId : card.uniqueId.ToString();
    }

    private string FormatMultiplier(float value)
    {
        return value.ToString("0.##");
    }
}
