using System.Collections.Generic;

public class WitnessOfTheFirstExposureJoker : JokerBase
{
    public override string Name => "Witness of the First Exposure";
    public override string Description => "The first scoring face card gives X2 Mult.";
    public override int Cost => 5;

    public override void ApplyScoreEffect(ScoreContext scoreContext, int jokerSlotIndex)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null)
        {
            return;
        }

        HashSet<string> checkedInstanceIds = new HashSet<string>();

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            PlayingCard card = scoreContext.cardScoreEvents[i] != null ? scoreContext.cardScoreEvents[i].card : null;

            if (card == null)
            {
                continue;
            }

            string instanceKey = GetInstanceKey(card);

            if (checkedInstanceIds.Contains(instanceKey))
            {
                continue;
            }

            checkedInstanceIds.Add(instanceKey);

            if (!CardTraitUtility.IsFaceCardEffective(card, scoreContext.ruleContext))
            {
                continue;
            }

            scoreContext.mult *= 2f;
            scoreContext.jokerScoreEvents?.Add(new JokerScoreEvent(
                jokerSlotIndex,
                "*2",
                Name,
                scoreContext.cardScoreEvents[i],
                0,
                0f,
                2f));
            string pareidoliaText = !CardTraitUtility.IsNaturalFaceCard(card) &&
                scoreContext.ruleContext != null &&
                scoreContext.ruleContext.hasPareidolia
                    ? " via Masquerader of a Hundred Faces"
                    : string.Empty;
            scoreContext.triggeredJokerEffectLog.Add($"{Name}: first face scoring card {card.GetDisplayName()}{pareidoliaText}, X2 Mult");
            return;
        }
    }

    private string GetInstanceKey(PlayingCard card)
    {
        return !string.IsNullOrEmpty(card.instanceId) ? card.instanceId : card.uniqueId.ToString();
    }
}
