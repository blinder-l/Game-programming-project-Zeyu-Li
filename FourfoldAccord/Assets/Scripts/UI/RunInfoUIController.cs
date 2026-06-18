using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunInfoUIController : MonoBehaviour
{
    private GameObject runInfoPanel;
    private GameObject handTypeInfoContainer;
    private GameObject suitInfoContainer;
    private GameObject edictInfoContainer;
    private GameObject generatedInfoContainer;
    private Button handTypeButton;
    private Button suitButton;
    private Button edictButton;
    private Button returnButton;
    private TMP_Text generatedInfoText;
    private readonly List<HandTypeInfoRow> handTypeRows = new List<HandTypeInfoRow>();
    private readonly List<SuitInfoRow> suitRows = new List<SuitInfoRow>();
    private readonly List<EdictSlotView> edictSlotViews = new List<EdictSlotView>();
    private readonly PokerHandType[] fallbackHandTypeOrder =
    {
        PokerHandType.StraightFlush,
        PokerHandType.FourOfAKind,
        PokerHandType.FullHouse,
        PokerHandType.Flush,
        PokerHandType.Straight,
        PokerHandType.ThreeOfAKind,
        PokerHandType.TwoPair,
        PokerHandType.Pair,
        PokerHandType.HighCard
    };

    private readonly HandTypeRowBinding[] sceneHandTypeRows =
    {
        new HandTypeRowBinding("StraightFlushBox", PokerHandType.StraightFlush),
        new HandTypeRowBinding("FourOfAKindBox", PokerHandType.FourOfAKind),
        new HandTypeRowBinding("FullHouseBox", PokerHandType.FullHouse),
        new HandTypeRowBinding("FlushBox", PokerHandType.Flush),
        new HandTypeRowBinding("StraightBox", PokerHandType.Straight),
        new HandTypeRowBinding("ThreeOfAKindBox", PokerHandType.ThreeOfAKind),
        new HandTypeRowBinding("TwoPairBox", PokerHandType.TwoPair),
        new HandTypeRowBinding("OnePairBox", PokerHandType.Pair),
        new HandTypeRowBinding("HighCardBox", PokerHandType.HighCard)
    };

    private readonly SuitRowBinding[] sceneSuitRows =
    {
        new SuitRowBinding("HeartBox", Suit.Hearts),
        new SuitRowBinding("SpadeBox", Suit.Spades),
        new SuitRowBinding("DiamondsBox", Suit.Diamonds),
        new SuitRowBinding("ClubsBox", Suit.Clubs)
    };

    private HandTypeLevelManager handTypeLevelManager;
    private SuitMasteryManager suitMasteryManager;
    private IReadOnlyDictionary<PokerHandType, int> handTypePlayCounts;
    private IReadOnlyList<EdictCard> purchasedEdicts;
    [SerializeField] private EdictSpriteDatabase edictSpriteDatabase;
    private CardTooltipController tooltipController;
    private RunInfoTab currentTab = RunInfoTab.HandTypes;

    public void Initialize(Transform canvasRoot)
    {
        if (canvasRoot == null)
        {
            Debug.LogError("Failed to bind RunInfoPanel: Canvas root is null");
            return;
        }

        runInfoPanel = FindObjectIncludingInactive(canvasRoot, "RunInfoPanel");

        if (runInfoPanel == null)
        {
            Debug.LogError("Failed to bind RunInfoPanel");
            return;
        }

        handTypeInfoContainer = FindObjectIncludingInactive(runInfoPanel.transform, "HandTypeInfoContainer");
        suitInfoContainer = FindObjectIncludingInactive(runInfoPanel.transform, "SuitInfoContainer");
        edictInfoContainer = FindObjectIncludingInactive(runInfoPanel.transform, "EdictInfoContainer");
        handTypeButton = BindButton(runInfoPanel.transform, "HandTypeButton", ShowHandTypeInfo);
        suitButton = BindButton(runInfoPanel.transform, "Suit", ShowSuitInfo);
        edictButton = BindOptionalButton(runInfoPanel.transform, "Edict", ShowVoucherInfo);
        if (edictButton == null)
        {
            edictButton = BindButton(runInfoPanel.transform, "Voucher", ShowVoucherInfo);
        }
        returnButton = BindButton(runInfoPanel.transform, "ReturnButton", Hide);
        ResolveEdictSpriteDatabase();
        BindHandTypeRows();
        BindSuitRows();
        BindEdictSlots();
        EnsureGeneratedInfoContainer();
        Hide();
        Debug.Log("Bound RunInfoPanel");
    }

    public void SetDataSources(
        HandTypeLevelManager handTypeLevels,
        SuitMasteryManager suitMastery,
        IReadOnlyDictionary<PokerHandType, int> playCounts)
    {
        SetDataSources(handTypeLevels, suitMastery, playCounts, null);
    }

    public void SetDataSources(
        HandTypeLevelManager handTypeLevels,
        SuitMasteryManager suitMastery,
        IReadOnlyDictionary<PokerHandType, int> playCounts,
        IReadOnlyList<EdictCard> purchasedEdicts)
    {
        handTypeLevelManager = handTypeLevels;
        suitMasteryManager = suitMastery;
        handTypePlayCounts = playCounts;
        this.purchasedEdicts = purchasedEdicts;
        Debug.Log($"RunInfo data sources set. HandTypeLevelManager: {handTypeLevelManager != null}, SuitMasteryManager: {suitMasteryManager != null}, play counts: {handTypePlayCounts != null}, edicts: {this.purchasedEdicts != null}");
    }

    public void SetTooltipController(CardTooltipController controller)
    {
        tooltipController = controller;

        for (int i = 0; i < edictSlotViews.Count; i++)
        {
            edictSlotViews[i]?.SetTooltipController(tooltipController);
        }
    }

    public void ShowDefault()
    {
        if (runInfoPanel == null)
        {
            Debug.LogError("Cannot show RunInfoPanel: RunInfoPanel is not bound");
            return;
        }

        runInfoPanel.SetActive(true);
        runInfoPanel.transform.SetAsLastSibling();
        ShowHandTypeInfo();
        Debug.Log("RunInfoPanel shown");
    }

    public void Hide()
    {
        if (runInfoPanel != null)
        {
            runInfoPanel.SetActive(false);
        }

        Debug.Log("RunInfoPanel hidden");
    }

    public void Refresh()
    {
        if (runInfoPanel == null || !runInfoPanel.activeInHierarchy)
        {
            return;
        }

        ShowCurrentTab();
    }

    private void ShowHandTypeInfo()
    {
        currentTab = RunInfoTab.HandTypes;
        SetHandTypeRowsActive(true);
        SetSuitRowsActive(false);
        SetEdictInfoActive(false);
        SetGeneratedInfoActive(false);

        if (handTypeLevelManager == null)
        {
            Debug.LogError("Cannot show hand type info: HandTypeLevelManager is null");
            return;
        }

        for (int i = 0; i < handTypeRows.Count; i++)
        {
            HandTypeInfoRow row = handTypeRows[i];

            if (!TryGetRowHandType(row, i, out PokerHandType handType))
            {
                row.SetActive(false);
                continue;
            }

            int playCount = GetHandTypePlayCount(handType);
            row.SetActive(true);
            row.SetHandTypeInfo(
                $"Lv {handTypeLevelManager.GetLevel(handType)}",
                GetDisplayName(handType),
                handTypeLevelManager.GetCurrentBaseChips(handType).ToString(),
                handTypeLevelManager.GetCurrentBaseMult(handType).ToString(),
                playCount.ToString());

            Debug.Log($"RunInfo hand type row updated: {handType}, Lv {handTypeLevelManager.GetLevel(handType)}, chips {handTypeLevelManager.GetCurrentBaseChips(handType)}, mult {handTypeLevelManager.GetCurrentBaseMult(handType)}, times {playCount}");
        }

        Debug.Log("RunInfoPanel showing hand type info");
    }

    private bool TryGetRowHandType(HandTypeInfoRow row, int rowIndex, out PokerHandType handType)
    {
        if (row != null && row.HasBoundHandType)
        {
            handType = row.BoundHandType;
            return true;
        }

        if (rowIndex >= 0 && rowIndex < fallbackHandTypeOrder.Length)
        {
            handType = fallbackHandTypeOrder[rowIndex];
            Debug.LogWarning($"RunInfo row '{row?.RootName}' could not parse its HandTypeText. Using fallback hand type {handType}.");
            return true;
        }

        handType = PokerHandType.HighCard;
        Debug.LogWarning($"RunInfo row '{row?.RootName}' has no matching hand type and no fallback entry.");
        return false;
    }

    private string GetDisplayName(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.HighCard:
                return "High Card";
            case PokerHandType.Pair:
                return "One Pair";
            case PokerHandType.TwoPair:
                return "Two Pair";
            case PokerHandType.ThreeOfAKind:
                return "Three of a Kind";
            case PokerHandType.Straight:
                return "Straight";
            case PokerHandType.Flush:
                return "Flush";
            case PokerHandType.FullHouse:
                return "Full House";
            case PokerHandType.FourOfAKind:
                return "Four of a Kind";
            case PokerHandType.StraightFlush:
                return "Straight Flush";
            default:
                return handType.ToString();
        }
    }

    private int GetHandTypePlayCount(PokerHandType handType)
    {
        if (handTypePlayCounts == null || !handTypePlayCounts.ContainsKey(handType))
        {
            return 0;
        }

        return handTypePlayCounts[handType];
    }

    private void ShowSuitInfo()
    {
        currentTab = RunInfoTab.Suits;
        SetHandTypeRowsActive(false);
        SetSuitRowsActive(true);
        SetEdictInfoActive(false);
        SetGeneratedInfoActive(false);

        if (suitMasteryManager == null)
        {
            Debug.LogError("Cannot show suit info: SuitMasteryManager is null");
            return;
        }

        for (int i = 0; i < suitRows.Count; i++)
        {
            SuitInfoRow row = suitRows[i];
            Suit suit = row.BoundSuit;
            int level = suitMasteryManager.GetLevel(suit);

            row.SetActive(true);
            row.SetSuitInfo(
                $"Lv{level}",
                GetSuitDisplayName(suit),
                GetSuitEffectDescription(suit, level));

            Debug.Log($"RunInfo suit row updated: {suit}, Lv {level}, effect: {GetSuitEffectDescription(suit, level)}");
        }

        Debug.Log("RunInfoPanel showing suit info");
    }

    private void ShowVoucherInfo()
    {
        currentTab = RunInfoTab.Vouchers;
        SetHandTypeRowsActive(false);
        SetSuitRowsActive(false);
        SetEdictInfoActive(edictInfoContainer != null && edictSlotViews.Count > 0);
        SetGeneratedInfoActive(edictInfoContainer == null || edictSlotViews.Count == 0);

        if (edictInfoContainer != null && edictSlotViews.Count > 0)
        {
            RefreshEdictSlots();
        }
        else if (generatedInfoText != null)
        {
            generatedInfoText.text = "Edicts\n\nNo Edict slots found.";
        }

        Debug.Log("RunInfoPanel showing Edict info");
    }

    private void ShowCurrentTab()
    {
        switch (currentTab)
        {
            case RunInfoTab.Suits:
                ShowSuitInfo();
                break;
            case RunInfoTab.Vouchers:
                ShowVoucherInfo();
                break;
            default:
                ShowHandTypeInfo();
                break;
        }
    }

    private void BindHandTypeRows()
    {
        handTypeRows.Clear();

        if (handTypeInfoContainer == null)
        {
            Debug.LogError("Failed to bind HandTypeInfoContainer");
            return;
        }

        for (int i = 0; i < sceneHandTypeRows.Length; i++)
        {
            HandTypeRowBinding rowBinding = sceneHandTypeRows[i];
            GameObject rowObject = FindObjectIncludingInactive(handTypeInfoContainer.transform, rowBinding.rowObjectName);

            if (rowObject == null)
            {
                Debug.LogWarning($"RunInfo hand type row not found by name: {rowBinding.rowObjectName}");
                continue;
            }

            HandTypeInfoRow row = new HandTypeInfoRow(rowObject, rowBinding.handType);
            handTypeRows.Add(row);
            Debug.Log($"Bound hand type info row: {rowObject.name}, hand type = {row.BoundHandType}");
        }

        if (handTypeRows.Count == 0)
        {
            BindHandTypeRowsByStructureFallback();
        }

        Debug.Log($"Bound {handTypeRows.Count} hand type info rows");
    }

    private void BindSuitRows()
    {
        suitRows.Clear();

        if (suitInfoContainer == null)
        {
            Debug.LogError("Failed to bind SuitInfoContainer");
            return;
        }

        for (int i = 0; i < sceneSuitRows.Length; i++)
        {
            SuitRowBinding rowBinding = sceneSuitRows[i];
            GameObject rowObject = FindObjectIncludingInactive(suitInfoContainer.transform, rowBinding.rowObjectName);

            if (rowObject == null)
            {
                Debug.LogWarning($"RunInfo suit row not found by name: {rowBinding.rowObjectName}");
                continue;
            }

            SuitInfoRow row = new SuitInfoRow(rowObject, rowBinding.suit);
            suitRows.Add(row);
            Debug.Log($"Bound suit info row: {rowObject.name}, suit = {row.BoundSuit}");
        }

        Debug.Log($"Bound {suitRows.Count} suit info rows");
    }

    private void BindEdictSlots()
    {
        edictSlotViews.Clear();

        if (edictInfoContainer == null)
        {
            Debug.LogWarning("RunInfo EdictInfoContainer not found.");
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            string slotName = $"EdictSlot{i + 1}";
            GameObject slotObject = FindObjectIncludingInactive(edictInfoContainer.transform, slotName);

            if (slotObject == null)
            {
                Debug.LogWarning($"RunInfo Edict slot not found: {slotName}");
                continue;
            }

            EdictSlotView slotView = slotObject.GetComponent<EdictSlotView>();

            if (slotView == null)
            {
                slotView = slotObject.AddComponent<EdictSlotView>();
            }

            slotView.SetTooltipController(tooltipController);
            slotView.SetEdict(null, edictSpriteDatabase);
            edictSlotViews.Add(slotView);
            Debug.Log($"Bound RunInfo Edict slot: {slotName}");
        }

        Debug.Log($"Bound {edictSlotViews.Count} Edict slots");
    }

    private void RefreshEdictSlots()
    {
        ResolveEdictSpriteDatabase();

        for (int i = 0; i < edictSlotViews.Count; i++)
        {
            EdictCard edictCard = purchasedEdicts != null && i < purchasedEdicts.Count ? purchasedEdicts[i] : null;
            edictSlotViews[i].SetTooltipController(tooltipController);
            edictSlotViews[i].SetEdict(edictCard, edictSpriteDatabase);
        }
    }

    private void BindHandTypeRowsByStructureFallback()
    {
        Debug.LogWarning("RunInfo explicit hand type rows were not found. Falling back to structural row scan.");

        for (int i = 0; i < handTypeInfoContainer.transform.childCount; i++)
        {
            Transform child = handTypeInfoContainer.transform.GetChild(i);
            HandTypeInfoRow row = new HandTypeInfoRow(child.gameObject);

            if (!row.HasRequiredTextFields)
            {
                continue;
            }

            handTypeRows.Add(row);
            Debug.Log($"Bound fallback hand type info row: {child.name}, hand type = {(row.HasBoundHandType ? row.BoundHandType.ToString() : "unresolved")}");
        }
    }

    private void EnsureGeneratedInfoContainer()
    {
        if (generatedInfoContainer != null || runInfoPanel == null)
        {
            return;
        }

        generatedInfoContainer = new GameObject("GeneratedRunInfoText", typeof(RectTransform));
        generatedInfoContainer.transform.SetParent(runInfoPanel.transform, false);
        RectTransform rectTransform = generatedInfoContainer.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.08f, 0.12f);
        rectTransform.anchorMax = new Vector2(0.92f, 0.82f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        generatedInfoText = generatedInfoContainer.AddComponent<TextMeshProUGUI>();
        generatedInfoText.raycastTarget = false;
        generatedInfoText.fontSize = 28f;
        generatedInfoText.alignment = TextAlignmentOptions.TopLeft;
        generatedInfoText.text = string.Empty;
        generatedInfoContainer.SetActive(false);
    }

    private Button BindButton(Transform root, string objectName, UnityEngine.Events.UnityAction onClicked)
    {
        GameObject buttonObject = FindObjectIncludingInactive(root, objectName);

        if (buttonObject == null)
        {
            Debug.LogError($"Failed to bind {objectName}");
            return null;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        Image image = buttonObject.GetComponent<Image>();

        if (image != null)
        {
            image.raycastTarget = true;
            button.targetGraphic = image;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClicked);
        button.interactable = true;
        DisableTextRaycasts(buttonObject);
        Debug.Log($"Bound {objectName}");
        return button;
    }

    private Button BindOptionalButton(Transform root, string objectName, UnityEngine.Events.UnityAction onClicked)
    {
        GameObject buttonObject = FindObjectIncludingInactive(root, objectName);

        if (buttonObject == null)
        {
            return null;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        Image image = buttonObject.GetComponent<Image>();

        if (image != null)
        {
            image.raycastTarget = true;
            button.targetGraphic = image;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClicked);
        button.interactable = true;
        DisableTextRaycasts(buttonObject);
        Debug.Log($"Bound {objectName}");
        return button;
    }

    private void SetHandTypeRowsActive(bool isActive)
    {
        if (handTypeInfoContainer != null)
        {
            handTypeInfoContainer.SetActive(isActive);
        }
    }

    private void SetSuitRowsActive(bool isActive)
    {
        if (suitInfoContainer != null)
        {
            suitInfoContainer.SetActive(isActive);
        }
    }

    private void SetEdictInfoActive(bool isActive)
    {
        if (edictInfoContainer != null)
        {
            edictInfoContainer.SetActive(isActive);
        }
    }

    private void SetGeneratedInfoActive(bool isActive)
    {
        if (generatedInfoContainer != null)
        {
            generatedInfoContainer.SetActive(isActive);
        }
    }

    private void DisableTextRaycasts(GameObject rootObject)
    {
        TMP_Text[] textComponents = rootObject.GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < textComponents.Length; i++)
        {
            textComponents[i].raycastTarget = false;
        }
    }

    private GameObject FindObjectIncludingInactive(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private void ResolveEdictSpriteDatabase()
    {
        if (edictSpriteDatabase != null)
        {
            return;
        }

        edictSpriteDatabase = UnityEngine.Object.FindFirstObjectByType<EdictSpriteDatabase>();

        if (edictSpriteDatabase == null)
        {
            Debug.LogError("EdictSpriteDatabase not found. Create a SpriteDatabases scene object and add EdictSpriteDatabase with populated sprite entries.");
        }
    }

    private string GetSuitDisplayName(Suit suit)
    {
        switch (suit)
        {
            case Suit.Hearts:
                return "Hearts";
            case Suit.Spades:
                return "Spades";
            case Suit.Diamonds:
                return "Diamonds";
            default:
                return "Clubs";
        }
    }

    private string GetSuitEffectDescription(Suit suit, int level)
    {
        if (level < 1)
        {
            return "No effect unlocked";
        }

        switch (suit)
        {
            case Suit.Hearts:
                if (level >= 3)
                {
                    return "+20 Chips; 2+ Hearts: +0.5 Mult";
                }

                return level >= 2 ? "+15 Chips" : "+10 Chips";
            case Suit.Spades:
                if (level >= 3)
                {
                    return "+1.0 Mult; +10 Chips";
                }

                return level >= 2 ? "+0.8 Mult" : "+0.5 Mult";
            case Suit.Diamonds:
                if (level >= 3)
                {
                    return "1-2 Diamonds: +3 Gold";
                }

                return level >= 2 ? "1-2 Diamonds: +2 Gold" : "1-2 Diamonds: +1 Gold";
            case Suit.Clubs:
                if (level >= 3)
                {
                    return "Retrigger highest rank chips; +5 Chips; +0.5 Mult";
                }

                return level >= 2 ? "Retrigger highest rank chips; +5 Chips" : "Retrigger highest rank chips";
            default:
                return string.Empty;
        }
    }
}

public enum RunInfoTab
{
    HandTypes,
    Suits,
    Vouchers
}

public struct HandTypeRowBinding
{
    public readonly string rowObjectName;
    public readonly PokerHandType handType;

    public HandTypeRowBinding(string rowObjectName, PokerHandType handType)
    {
        this.rowObjectName = rowObjectName;
        this.handType = handType;
    }
}

public struct SuitRowBinding
{
    public readonly string rowObjectName;
    public readonly Suit suit;

    public SuitRowBinding(string rowObjectName, Suit suit)
    {
        this.rowObjectName = rowObjectName;
        this.suit = suit;
    }
}

public class HandTypeInfoRow
{
    private readonly GameObject root;
    private readonly TMP_Text rankText;
    private readonly TMP_Text handTypeText;
    private readonly TMP_Text chipsText;
    private readonly TMP_Text multText;
    private readonly TMP_Text timesText;
    private readonly bool hasBoundHandType;
    private readonly PokerHandType boundHandType;

    public string RootName => root != null ? root.name : string.Empty;
    public bool HasBoundHandType => hasBoundHandType;
    public PokerHandType BoundHandType => boundHandType;
    public bool HasRequiredTextFields => rankText != null && handTypeText != null && chipsText != null && multText != null && timesText != null;

    public HandTypeInfoRow(GameObject root)
        : this(root, null)
    {
    }

    public HandTypeInfoRow(GameObject root, PokerHandType explicitHandType)
        : this(root, (PokerHandType?)explicitHandType)
    {
    }

    private HandTypeInfoRow(GameObject root, PokerHandType? explicitHandType)
    {
        this.root = root;
        rankText = FindText(root.transform, "RankText");
        handTypeText = FindText(root.transform, "HandTypeText");
        chipsText = FindText(root.transform, "ChipsText");
        multText = FindText(root.transform, "MultText");
        timesText = FindText(root.transform, "TimesNumberText");

        if (explicitHandType.HasValue)
        {
            boundHandType = explicitHandType.Value;
            hasBoundHandType = true;
        }
        else
        {
            hasBoundHandType = TryParseHandType(handTypeText != null ? handTypeText.text : string.Empty, out boundHandType);
        }

        if (!HasRequiredTextFields)
        {
            Debug.LogWarning($"RunInfo row '{RootName}' missing text binding. Rank: {rankText != null}, HandType: {handTypeText != null}, Chips: {chipsText != null}, Mult: {multText != null}, Times: {timesText != null}");
        }
    }

    public void SetActive(bool isActive)
    {
        if (root != null)
        {
            root.SetActive(isActive);
        }
    }

    public void SetHandTypeInfo(string rank, string handType, string chips, string mult, string times)
    {
        SetText(rankText, rank);
        SetText(handTypeText, handType);
        SetText(chipsText, chips);
        SetText(multText, mult);
        SetText(timesText, times);
    }

    private static TMP_Text FindText(Transform root, string objectName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                TMP_Text text = child.GetComponent<TMP_Text>();

                if (text != null)
                {
                    text.raycastTarget = false;
                }

                return text;
            }
        }

        return null;
    }

    private static void SetText(TMP_Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }

    private static bool TryParseHandType(string displayName, out PokerHandType handType)
    {
        string normalized = NormalizeHandTypeName(displayName);

        switch (normalized)
        {
            case "straightflush":
                handType = PokerHandType.StraightFlush;
                return true;
            case "fourofakind":
                handType = PokerHandType.FourOfAKind;
                return true;
            case "fullhouse":
                handType = PokerHandType.FullHouse;
                return true;
            case "flush":
                handType = PokerHandType.Flush;
                return true;
            case "straight":
                handType = PokerHandType.Straight;
                return true;
            case "threeofakind":
                handType = PokerHandType.ThreeOfAKind;
                return true;
            case "twopair":
                handType = PokerHandType.TwoPair;
                return true;
            case "onepair":
            case "pair":
                handType = PokerHandType.Pair;
                return true;
            case "highcard":
                handType = PokerHandType.HighCard;
                return true;
            default:
                handType = PokerHandType.HighCard;
                return false;
        }
    }

    private static string NormalizeHandTypeName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return string.Empty;
        }

        return displayName
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Trim()
            .ToLowerInvariant();
    }
}

public class SuitInfoRow
{
    private readonly GameObject root;
    private readonly TMP_Text rankText;
    private readonly TMP_Text suitText;
    private readonly TMP_Text effectText;
    private readonly Suit boundSuit;

    public Suit BoundSuit => boundSuit;

    public SuitInfoRow(GameObject root, Suit boundSuit)
    {
        this.root = root;
        this.boundSuit = boundSuit;
        rankText = FindText(root.transform, "RankText");
        suitText = FindText(root.transform, "SuitText");
        effectText = FindText(root.transform, "EffectText");

        if (rankText == null || suitText == null || effectText == null)
        {
            Debug.LogWarning($"RunInfo suit row '{(root != null ? root.name : string.Empty)}' missing text binding. Rank: {rankText != null}, Suit: {suitText != null}, Effect: {effectText != null}");
        }
    }

    public void SetActive(bool isActive)
    {
        if (root != null)
        {
            root.SetActive(isActive);
        }
    }

    public void SetSuitInfo(string rank, string suit, string effect)
    {
        SetText(rankText, rank);
        SetText(suitText, suit);
        SetText(effectText, effect);
    }

    private static TMP_Text FindText(Transform root, string objectName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                TMP_Text text = child.GetComponent<TMP_Text>();

                if (text != null)
                {
                    text.raycastTarget = false;
                }

                return text;
            }
        }

        return null;
    }

    private static void SetText(TMP_Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
