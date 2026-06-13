using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Button playButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button sortBySuitButton;
    [SerializeField] private Button sortByRankButton;
    [SerializeField] private TMP_Text blindNameText;
    [SerializeField] private TMP_Text targetScoreText;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text handTypeText;
    [SerializeField] private TMP_Text handsText;
    [SerializeField] private TMP_Text discardsText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text anteNumberText;
    [SerializeField] private HandCardView[] playedCardViews;
    [SerializeField] private TMP_Text resolutionInfoText;

    public event Action<int> HandCardClicked;
    public event Action PlayButtonClicked;
    public event Action DiscardButtonClicked;
    public event Action SortBySuitButtonClicked;
    public event Action SortByRankButtonClicked;

    public GameUIState CurrentState { get; private set; }

    private void OnEnable()
    {
        AddButtonListener(playButton, HandlePlayButtonClicked);
        AddButtonListener(discardButton, HandleDiscardButtonClicked);
        AddButtonListener(sortBySuitButton, HandleSortBySuitButtonClicked);
        AddButtonListener(sortByRankButton, HandleSortByRankButtonClicked);
    }

    private void OnDisable()
    {
        RemoveButtonListener(playButton, HandlePlayButtonClicked);
        RemoveButtonListener(discardButton, HandleDiscardButtonClicked);
        RemoveButtonListener(sortBySuitButton, HandleSortBySuitButtonClicked);
        RemoveButtonListener(sortByRankButton, HandleSortByRankButtonClicked);
    }

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
        SetActionButtonsInteractable(newState == GameUIState.PlayingBlind);

        if (enteringRunFailed)
        {
            SetText(blindNameText, "Run Failed");
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
                cardView.SetCard(i, currentHand[i], cardSpriteDatabase, HandleHandCardClicked);
            }
            else
            {
                cardView.Clear();
            }
        }
    }

    public void RefreshBlindStatus(
        string blindName,
        int targetScore,
        int currentScore,
        string latestHandType,
        int handsRemaining,
        int discardsRemaining,
        int currentGold,
        int anteNumber)
    {
        SetText(blindNameText, blindName);
        SetText(targetScoreText, $"Target: {targetScore}");
        SetText(currentScoreText, $"Score: {currentScore}");
        SetText(handTypeText, $"Hand: {latestHandType}");
        SetText(handsText, $"Hands: {handsRemaining}");
        SetText(discardsText, $"Discards: {discardsRemaining}");
        SetText(goldText, $"Gold: ${currentGold}");
        SetText(anteNumberText, $"Ante: {anteNumber}");
    }

    public void RefreshPlayedCards(IReadOnlyList<PlayingCard> playedCards)
    {
        if (playedCardViews == null)
        {
            return;
        }

        for (int i = 0; i < playedCardViews.Length; i++)
        {
            HandCardView cardView = playedCardViews[i];

            if (cardView == null)
            {
                continue;
            }

            if (playedCards != null && i < playedCards.Count)
            {
                cardView.SetCard(i, playedCards[i], cardSpriteDatabase, null);
            }
            else
            {
                cardView.Clear();
            }
        }
    }

    public void RefreshResolutionInfo(ScoreContext scoreContext)
    {
        if (scoreContext == null)
        {
            SetText(resolutionInfoText, "No hand played yet.");
            return;
        }

        string resolutionText =
            $"Hand Type: {scoreContext.handType}\n" +
            $"Chips: {scoreContext.chips}  Mult: {scoreContext.mult}\n" +
            $"Final Score: {scoreContext.finalScore}\n" +
            $"Gold Reward: +{scoreContext.goldReward}\n\n" +
            $"Suit Effects:\n{scoreContext.GetSuitEffectDebugText()}\n\n" +
            $"Joker Effects:\n{scoreContext.GetJokerEffectDebugText()}";

        SetText(resolutionInfoText, resolutionText);
    }

    private void HandleHandCardClicked(int handIndex)
    {
        if (CurrentState != GameUIState.PlayingBlind)
        {
            return;
        }

        HandCardClicked?.Invoke(handIndex);
    }

    private void HandlePlayButtonClicked()
    {
        if (CurrentState == GameUIState.PlayingBlind)
        {
            PlayButtonClicked?.Invoke();
        }
    }

    private void HandleDiscardButtonClicked()
    {
        if (CurrentState == GameUIState.PlayingBlind)
        {
            DiscardButtonClicked?.Invoke();
        }
    }

    private void HandleSortBySuitButtonClicked()
    {
        if (CurrentState == GameUIState.PlayingBlind)
        {
            SortBySuitButtonClicked?.Invoke();
        }
    }

    private void HandleSortByRankButtonClicked()
    {
        if (CurrentState == GameUIState.PlayingBlind)
        {
            SortByRankButtonClicked?.Invoke();
        }
    }

    private void SetActionButtonsInteractable(bool isInteractable)
    {
        SetButtonInteractable(playButton, isInteractable);
        SetButtonInteractable(discardButton, isInteractable);
        SetButtonInteractable(sortBySuitButton, isInteractable);
        SetButtonInteractable(sortByRankButton, isInteractable);
    }

    private void SetActiveIfAssigned(GameObject target, bool isActive)
    {
        if (target == null)
        {
            return;
        }

        target.SetActive(isActive);
    }

    private void SetButtonInteractable(Button button, bool isInteractable)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = isInteractable;
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.text = value;
    }

    private void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    private void RemoveButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
    }
}
