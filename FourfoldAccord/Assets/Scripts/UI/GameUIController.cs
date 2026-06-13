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

    private void SetActiveIfAssigned(GameObject target, bool isActive)
    {
        if (target == null)
        {
            return;
        }

        target.SetActive(isActive);
    }
}
