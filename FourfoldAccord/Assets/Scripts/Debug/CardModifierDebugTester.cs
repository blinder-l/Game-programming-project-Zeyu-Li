using System.Collections.Generic;
using UnityEngine;

public class CardModifierDebugTester : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private PrototypeBootstrap bootstrap;

    public void Initialize(PrototypeBootstrap prototypeBootstrap)
    {
        bootstrap = prototypeBootstrap;
        Debug.Log("CardModifierDebugTester initialized.");
    }

    private void Awake()
    {
        if (bootstrap == null)
        {
            bootstrap = GetComponent<PrototypeBootstrap>();
        }
    }

    private void Update()
    {
        if (!IsShiftHeld())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            SetEnhancement(CardEnhancement.Gold, "Set enhancement to Gold");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SetEnhancement(CardEnhancement.Stone, "Set enhancement to Stone");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SetEnhancement(CardEnhancement.Lucky, "Set enhancement to Lucky");
        }

        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SetSeal(CardSeal.Gold, "Set seal to Gold");
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SetSeal(CardSeal.Red, "Set seal to Red");
        }

        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SetSeal(CardSeal.Blue, "Set seal to Blue");
        }

        if (Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            SetSeal(CardSeal.Purple, "Set seal to Purple");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            AddPermanentBonusChips();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetModifierTestState();
        }
    }

    private void SetEnhancement(CardEnhancement enhancement, string action)
    {
        PlayingCard card = GetTargetCard();

        if (card == null)
        {
            return;
        }

        card.enhancement = enhancement;
        RefreshAndLog(card, action);
    }

    private void SetSeal(CardSeal seal, string action)
    {
        PlayingCard card = GetTargetCard();

        if (card == null)
        {
            return;
        }

        card.seal = seal;
        RefreshAndLog(card, action);
    }

    private void AddPermanentBonusChips()
    {
        PlayingCard card = GetTargetCard();

        if (card == null)
        {
            return;
        }

        card.permanentBonusChips += 5;
        RefreshAndLog(card, "Added +5 permanent bonus chips");
    }

    private void ResetModifierTestState()
    {
        PlayingCard card = GetTargetCard();

        if (card == null)
        {
            return;
        }

        card.enhancement = CardEnhancement.None;
        card.seal = CardSeal.None;
        card.permanentBonusChips = 0;
        card.isDebuffed = false;
        RefreshAndLog(card, "Cleared modifier test state");
    }

    private PlayingCard GetTargetCard()
    {
        HandManager handManager = bootstrap != null ? bootstrap.GetDebugHandManager() : null;

        if (handManager == null)
        {
            Debug.Log("Card modifier debug failed: HandManager is unavailable.");
            return null;
        }

        List<PlayingCard> selectedCards = handManager.GetSelectedCards();

        if (selectedCards.Count > 0)
        {
            return selectedCards[0];
        }

        if (handManager.CurrentHandCount > 0)
        {
            return handManager.CurrentHand[0];
        }

        Debug.Log("Card modifier debug failed: no current hand card is available.");
        return null;
    }

    private void RefreshAndLog(PlayingCard card, string action)
    {
        if (bootstrap != null)
        {
            bootstrap.RefreshDebugCardModifierDisplay();
        }

        Debug.Log(
            $"Card Modifier Debug: {action}\n" +
            $"Card: {card.GetDisplayName()}\n" +
            $"uniqueId: {card.uniqueId}\n" +
            $"instanceId: {card.instanceId}\n" +
            $"enhancement: {card.enhancement}\n" +
            $"seal: {card.seal}\n" +
            $"permanentBonusChips: {card.permanentBonusChips}");
    }

    private bool IsShiftHeld()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
#endif
}
