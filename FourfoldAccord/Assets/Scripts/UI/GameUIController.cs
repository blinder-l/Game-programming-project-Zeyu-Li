using System.Collections.Generic;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameObject playStateRoot;
    [SerializeField] private GameObject cashOutPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject deckStatsPanel;
    [SerializeField] private GameObject currentHandStatsPanel;
    [SerializeField] private GameObject cardTooltipPanel;
    [SerializeField] private GameObject actionButtonsContainer;
    [SerializeField] private CardSpriteDatabase cardSpriteDatabase;
    [SerializeField] private HandCardView[] handCardViews;

    public GameUIState CurrentState { get; private set; }

    private void Awake()
    {
        SetState(GameUIState.PlayingBlind);
    }

    public void SetState(GameUIState newState)
    {
        bool enteringRunFailed = CurrentState != GameUIState.RunFailed && newState == GameUIState.RunFailed;
        CurrentState = newState;

        SetActiveIfAssigned(playStateRoot, newState == GameUIState.PlayingBlind || newState == GameUIState.RunFailed);
        SetActiveIfAssigned(cashOutPanel, newState == GameUIState.CashOut);
        SetActiveIfAssigned(shopPanel, newState == GameUIState.Shop);
        SetActiveIfAssigned(deckStatsPanel, false);
        SetActiveIfAssigned(currentHandStatsPanel, false);
        SetActiveIfAssigned(cardTooltipPanel, false);
        SetActiveIfAssigned(actionButtonsContainer, newState == GameUIState.PlayingBlind);

        if (enteringRunFailed)
        {
            Debug.Log("Run failed.");
        }
    }

    public void RefreshHand(IReadOnlyList<PlayingCard> currentHand)
    {
        if (handCardViews == null)
        {
            return;
        }

        for (int i = 0; i < handCardViews.Length; i++)
        {
            HandCardView cardView = handCardViews[i];

            if (cardView == null)
            {
                continue;
            }

            if (currentHand != null && i < currentHand.Count)
            {
                cardView.SetCard(currentHand[i], cardSpriteDatabase);
            }
            else
            {
                cardView.Clear();
            }
        }
    }

    private void SetActiveIfAssigned(GameObject target, bool isActive)
    {
        if (target == null)
        {
            return;
        }

        target.SetActive(isActive);
    }
}
