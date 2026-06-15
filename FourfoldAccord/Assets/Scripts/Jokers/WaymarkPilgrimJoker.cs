using System.Collections.Generic;
using UnityEngine;

public class WaymarkPilgrimJoker : JokerBase
{
    public override string Name => "Waymark Pilgrim";
    public override string Description => "Each scoring card gains +5 permanent Chips once per played hand.";
    public override int Cost => 5;
    public override string CurrentEffectText => "";

    public override void OnAfterHandScored(ScoreContext scoreContext)
    {
        if (scoreContext == null || scoreContext.cardScoreEvents == null)
        {
            return;
        }

        HashSet<string> upgradedInstanceIds = new HashSet<string>();

        for (int i = 0; i < scoreContext.cardScoreEvents.Count; i++)
        {
            CardScoreEvent scoreEvent = scoreContext.cardScoreEvents[i];
            PlayingCard card = scoreEvent != null ? scoreEvent.card : null;

            if (card == null)
            {
                continue;
            }

            string instanceKey = string.IsNullOrEmpty(card.instanceId) ? card.uniqueId.ToString() : card.instanceId;

            if (upgradedInstanceIds.Contains(instanceKey))
            {
                continue;
            }

            upgradedInstanceIds.Add(instanceKey);
            card.permanentBonusChips += 5;
            string logMessage = $"{Name}: {card.GetDisplayName()} gained +5 permanent chips";
            scoreContext.triggeredJokerEffectLog.Add(logMessage);
            Debug.Log(logMessage);
        }
    }
}
