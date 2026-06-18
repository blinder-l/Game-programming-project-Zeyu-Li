using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class CardModifierSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorModifierSpriteFolder = "Assets/Spirites/Cards effect components";
    [SerializeField] private CardModifierSpriteEntry[] modifierSprites = new CardModifierSpriteEntry[0];
    private Dictionary<string, Sprite> spriteLookup;

    private void Awake()
    {
        BuildLookup();
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
        EnsureLookup();

        if (spriteLookup == null || string.IsNullOrEmpty(key))
        {
            return null;
        }

        if (spriteLookup.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"Missing card modifier sprite: {key}");
        return null;
    }

    private void EnsureLookup()
    {
        if (spriteLookup == null)
        {
            BuildLookup();
        }
    }

    private void BuildLookup()
    {
        spriteLookup = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        if (modifierSprites == null)
        {
            return;
        }

        for (int i = 0; i < modifierSprites.Length; i++)
        {
            CardModifierSpriteEntry entry = modifierSprites[i];

            if (entry == null || string.IsNullOrEmpty(entry.key))
            {
                continue;
            }

            spriteLookup[entry.key] = entry.sprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }

    [ContextMenu("Auto Populate Card Modifier Sprites")]
    public void AutoPopulateModifierSprites()
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
        BuildLookup();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
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
