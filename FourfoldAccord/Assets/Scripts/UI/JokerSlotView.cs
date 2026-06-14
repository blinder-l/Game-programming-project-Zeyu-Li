using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JokerSlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text jokerNameText;

    private void Awake()
    {
        ResolveText();
    }

    public void SetJoker(JokerBase joker)
    {
        ResolveText();

        if (jokerNameText == null)
        {
            return;
        }

        jokerNameText.text = joker != null ? joker.Name : "Empty";
    }

    private void ResolveText()
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

        if (slotImage != null)
        {
            slotImage.raycastTarget = false;
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
