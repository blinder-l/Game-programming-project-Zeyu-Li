using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardModifierSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorModifierSpriteFolder = "Assets/Spirites/Cards effect components";
    [SerializeField] private CardModifierSpriteEntry[] modifierSprites = new CardModifierSpriteEntry[0];

    private void Awake()
    {
        EnsureModifierSpritesPopulated();
    }

    public Sprite GetEnhancementSprite(CardEnhancement enhancement)
    {
        switch (enhancement)
        {
            case CardEnhancement.Stone:
                return GetSprite("card_stone");
            case CardEnhancement.Gold:
                return GetSprite("overlay_gold");
            case CardEnhancement.Lucky:
                return GetSprite("overlay_lucky");
            default:
                return null;
        }
    }

    public Sprite GetSealSprite(CardSeal seal)
    {
        switch (seal)
        {
            case CardSeal.Blue:
                return GetSprite("seal_blue");
            case CardSeal.Red:
                return GetSprite("seal_red");
            case CardSeal.Gold:
                return GetSprite("seal_gold");
            case CardSeal.Purple:
                return GetSprite("seal_purple");
            default:
                return null;
        }
    }

    public Sprite GetPermanentBonusChipsSprite()
    {
        return GetSprite("icon_permanent_bonus_chips");
    }

    private Sprite GetSprite(string key)
    {
        EnsureModifierSpritesPopulated();

        if (modifierSprites == null || string.IsNullOrEmpty(key))
        {
            return null;
        }

        for (int i = 0; i < modifierSprites.Length; i++)
        {
            CardModifierSpriteEntry entry = modifierSprites[i];

            if (entry != null && string.Equals(entry.key, key, StringComparison.OrdinalIgnoreCase))
            {
                return entry.sprite;
            }
        }

        Debug.LogWarning($"Missing card modifier sprite: {key}");
        return null;
    }

    private void EnsureModifierSpritesPopulated()
    {
#if UNITY_EDITOR
        if (modifierSprites == null || modifierSprites.Length == 0)
        {
            AutoPopulateModifierSprites();
        }
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (modifierSprites == null || modifierSprites.Length == 0)
        {
            AutoPopulateModifierSprites();
        }
    }

    [ContextMenu("Auto Populate Card Modifier Sprites")]
    private void AutoPopulateModifierSprites()
    {
        List<CardModifierSpriteEntry> entries = new List<CardModifierSpriteEntry>();
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { editorModifierSpriteFolder });

        for (int i = 0; i < spriteGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                continue;
            }

            string key = System.IO.Path.GetFileNameWithoutExtension(path);
            entries.Add(new CardModifierSpriteEntry(key, sprite));
        }

        modifierSprites = entries.ToArray();
    }
#endif
}

[Serializable]
public class CardModifierSpriteEntry
{
    public string key;
    public Sprite sprite;

    public CardModifierSpriteEntry(string key, Sprite sprite)
    {
        this.key = key;
        this.sprite = sprite;
    }
}
