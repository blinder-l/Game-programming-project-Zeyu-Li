using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JokerSlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text jokerNameText;
    [SerializeField] private CardTooltipTrigger tooltipTrigger;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetJoker(JokerBase joker)
    {
        ResolveReferences();

        if (jokerNameText == null)
        {
            return;
        }

        jokerNameText.text = joker != null ? joker.Name : "Empty";
        string tooltipName = joker != null ? joker.Name : "Empty Joker Slot";
        string tooltipEffect = joker != null ? joker.Description : "No Joker equipped.";
        tooltipTrigger?.SetTooltip(tooltipName, tooltipEffect);
        Debug.Log($"JokerSlot tooltip updated: {tooltipName}");
    }

    public void SetTooltipController(CardTooltipController tooltipController)
    {
        ResolveReferences();
        tooltipTrigger?.SetTooltipController(tooltipController);
    }

    private void ResolveReferences()
    {
        if (jokerNameText == null)
        {
            jokerNameText = GetComponentInChildren<TMP_Text>(true);
        }

        if (jokerNameText == null)
        {
            jokerNameText = CreateNameText();
        }

        if (jokerNameText != null)
        {
            jokerNameText.raycastTarget = false;
        }

        Image slotImage = GetComponent<Image>();

        if (slotImage == null)
        {
            slotImage = gameObject.AddComponent<Image>();
            slotImage.color = new Color(1f, 1f, 1f, 0.01f);
        }

        if (slotImage != null)
        {
            slotImage.raycastTarget = true;
        }

        if (tooltipTrigger == null)
        {
            tooltipTrigger = GetComponent<CardTooltipTrigger>();
        }

        if (tooltipTrigger == null)
        {
            tooltipTrigger = gameObject.AddComponent<CardTooltipTrigger>();
            Debug.Log($"Bound tooltip trigger: {gameObject.name}");
        }
    }

    private TMP_Text CreateNameText()
    {
        GameObject textObject = new GameObject("JokerNameText", typeof(RectTransform));
        textObject.transform.SetParent(transform, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.fontSize = 18f;
        text.color = Color.white;
        text.text = "Empty";
        return text;
    }
}
