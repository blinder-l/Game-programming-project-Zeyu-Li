using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpellSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorSpellSpriteFolder = "Assets/Spirites/Spell cards";
    [SerializeField] private SpellSpriteEntry[] spellSprites = new SpellSpriteEntry[0];

    private void Awake()
    {
        EnsureSpellSpritesPopulated();
    }

    public Sprite GetSprite(SpellCard spellCard)
    {
        if (spellCard == null)
        {
            return null;
        }

        return GetSprite(spellCard.spellType);
    }

    public Sprite GetSprite(SpellCardType spellType)
    {
        EnsureSpellSpritesPopulated();

        if (spellSprites == null)
        {
            return null;
        }

        for (int i = 0; i < spellSprites.Length; i++)
        {
            SpellSpriteEntry entry = spellSprites[i];

            if (entry.spellType == spellType)
            {
                if (entry.sprite == null)
                {
                    Debug.LogWarning($"Missing Spell sprite for {SpellCard.GetName(spellType)}");
                }

                return entry.sprite;
            }
        }

        Debug.LogWarning($"Missing Spell sprite entry for {SpellCard.GetName(spellType)}");
        return null;
    }

    private void EnsureSpellSpritesPopulated()
    {
#if UNITY_EDITOR
        if (spellSprites == null || spellSprites.Length == 0)
        {
            AutoPopulateSpellSprites();
        }
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spellSprites == null || spellSprites.Length == 0)
        {
            AutoPopulateSpellSprites();
        }
    }

    [ContextMenu("Auto Populate Spell Sprites")]
    private void AutoPopulateSpellSprites()
    {
        List<SpellSpriteEntry> entries = new List<SpellSpriteEntry>();

        foreach (SpellCardType spellType in Enum.GetValues(typeof(SpellCardType)))
        {
            string path = $"{editorSpellSpriteFolder}/{SpellCard.GetName(spellType)}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            entries.Add(new SpellSpriteEntry(spellType, sprite));
        }

        spellSprites = entries.ToArray();
    }
#endif
}

[Serializable]
public class SpellSpriteEntry
{
    public SpellCardType spellType;
    public Sprite sprite;

    public SpellSpriteEntry(SpellCardType spellType, Sprite sprite)
    {
        this.spellType = spellType;
        this.sprite = sprite;
    }
}
