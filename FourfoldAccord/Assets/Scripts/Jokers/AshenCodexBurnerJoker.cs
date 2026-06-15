using System.Collections.Generic;

public class AshenCodexBurnerJoker : JokerBase
{
    private bool hasCheckedFirstDiscardThisBlind;

    public override string Name => "Ashen Codex Burner";
    public override string Description => "The first discard each Blind upgrades the discarded hand type by 1 level.";
    public override int Cost => 8;

    public override void OnBlindStarted(JokerRuntimeContext runtimeContext)
    {
        hasCheckedFirstDiscardThisBlind = false;
    }

    public override void OnDiscardAction(JokerDiscardContext discardContext, int jokerSlotIndex)
    {
        if (discardContext == null || hasCheckedFirstDiscardThisBlind || !discardContext.isFirstDiscardThisBlind)
        {
            return;
        }

        hasCheckedFirstDiscardThisBlind = true;

        if (discardContext.selectedCards == null || discardContext.selectedCards.Count == 0 || discardContext.pokerHandEvaluator == null || discardContext.handTypeLevelManager == null)
        {
            UnityEngine.Debug.Log($"{Name}: first discard had no valid cards to upgrade.");
            return;
        }

        PokerHandResult result = discardContext.pokerHandEvaluator.Evaluate(new List<PlayingCard>(discardContext.selectedCards), discardContext.ruleContext);
        discardContext.handTypeLevelManager.Upgrade(result.handType);
        UnityEngine.Debug.Log($"{Name}: first discard hand type {result.handType} upgraded by 1");
    }
}
