using System.Collections.Generic;
using UnityEngine;

public class GildedMaskbearerJoker : JokerBase
{
    public override string Name => "Gilded Maskbearer";
    public override string Description => "Scoring J, Q, and K become Gold Cards.";
    public override int Cost => 7;
    public override string CurrentEffectText => "";

    public override void OnAfterHandScored(ScoreContext scoreContext)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null)
        {
            return;
        }

        HashSet<string> convertedInstanceIds = new HashSet<string>();

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[i];
            PlayingCard card = scoreEvent != null ? scoreEvent.card : null;

            if (card == null || !CardTraitUtility.IsFaceCardEffective(card, scoreContext.ruleContext))
            {
                continue;
            }

            string instanceKey = string.IsNullOrEmpty(card.instanceId) ? card.uniqueId.ToString() : card.instanceId;

            if (convertedInstanceIds.Contains(instanceKey))
            {
                continue;
            }

            convertedInstanceIds.Add(instanceKey);

            if (card.enhancement != CardEnhancement.Gold)
            {
                card.enhancement = CardEnhancement.Gold;
                string logMessage = $"{Name}: {card.GetDisplayName()} became Gold Card";
                scoreContext.triggeredJokerEffectLog.Add(logMessage);
                Debug.Log(logMessage);
            }
        }
    }

}
