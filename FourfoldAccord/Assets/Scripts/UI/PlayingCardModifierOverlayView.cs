using UnityEngine;
using UnityEngine.UI;

public class PlayingCardModifierOverlayView : MonoBehaviour
{
    private const string EnhancementLayerName = "EnhancementOverlay";
    private const string PermanentBonusLayerName = "PermanentBonusChipsOverlay";
    private const string SealLayerName = "SealOverlay";

    [SerializeField] private RectTransform cardVisualRoot;
    [SerializeField] private Image enhancementOverlay;
    [SerializeField] private Image permanentBonusOverlay;
    [SerializeField] private Image sealOverlay;

    private CardModifierSpriteDatabase spriteDatabase;

    public void Bind(RectTransform visualRoot, CardModifierSpriteDatabase database)
    {
        cardVisualRoot = visualRoot;
        spriteDatabase = database;
        EnsureLayers();
    }

    public void Refresh(PlayingCard card)
    {
        EnsureLayers();

        if (card == null || spriteDatabase == null)
        {
            Clear();
            return;
        }

        SetOverlay(enhancementOverlay, spriteDatabase.GetEnhancementSprite(card.enhancement));
        SetOverlay(permanentBonusOverlay, card.permanentBonusChips != 0 ? spriteDatabase.GetPermanentBonusChipsSprite() : null);
        SetOverlay(sealOverlay, spriteDatabase.GetSealSprite(card.seal));
    }

    public void Clear()
    {
        SetOverlay(enhancementOverlay, null);
        SetOverlay(permanentBonusOverlay, null);
        SetOverlay(sealOverlay, null);
    }

    private void EnsureLayers()
    {
        if (cardVisualRoot == null)
        {
            return;
        }

        enhancementOverlay = EnsureOverlayImage(enhancementOverlay, EnhancementLayerName, 1);
        permanentBonusOverlay = EnsureOverlayImage(permanentBonusOverlay, PermanentBonusLayerName, 2);
        sealOverlay = EnsureOverlayImage(sealOverlay, SealLayerName, 3);
    }

    private Image EnsureOverlayImage(Image currentImage, string layerName, int siblingIndex)
    {
        if (currentImage != null)
        {
            currentImage.transform.SetSiblingIndex(Mathf.Min(siblingIndex, cardVisualRoot.childCount - 1));
            return currentImage;
        }

        Transform existingTransform = cardVisualRoot.Find(layerName);
        RectTransform overlayRect = existingTransform as RectTransform;

        if (overlayRect == null)
        {
            GameObject overlayObject = new GameObject(layerName, typeof(RectTransform));
            overlayObject.transform.SetParent(cardVisualRoot, false);
            overlayRect = overlayObject.GetComponent<RectTransform>();
        }

        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.localScale = Vector3.one;

        Image image = overlayRect.GetComponent<Image>();

        if (image == null)
        {
            image = overlayRect.gameObject.AddComponent<Image>();
        }

        image.raycastTarget = false;
        image.preserveAspect = false;
        overlayRect.SetSiblingIndex(Mathf.Min(siblingIndex, cardVisualRoot.childCount - 1));
        return image;
    }

    private void SetOverlay(Image overlayImage, Sprite sprite)
    {
        if (overlayImage == null)
        {
            return;
        }

        overlayImage.sprite = sprite;
        overlayImage.enabled = sprite != null;
        overlayImage.gameObject.SetActive(sprite != null);
    }
}
