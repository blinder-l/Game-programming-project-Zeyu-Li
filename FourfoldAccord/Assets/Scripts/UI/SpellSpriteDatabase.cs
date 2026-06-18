using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SpellSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorSpellSpriteFolder = "Assets/Spirites/Spell cards";
    [SerializeField] private SpellSpriteEntry[] spellSprites = new SpellSpriteEntry[0];
    private Dictionary<SpellCardType, Sprite> spriteLookup;

    private void Awake()
    {
        BuildLookup();
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
        EnsureLookup();

        if (spriteLookup == null)
        {
            return null;
        }

        if (spriteLookup.TryGetValue(spellType, out Sprite sprite))
        {
            if (sprite == null)
            {
                Debug.LogWarning($"Missing Spell sprite for {SpellCard.GetName(spellType)}");
            }

            return sprite;
        }

        Debug.LogWarning($"Missing Spell sprite entry for {SpellCard.GetName(spellType)}");
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
        spriteLookup = new Dictionary<SpellCardType, Sprite>();

        if (spellSprites == null)
        {
            return;
        }

        for (int i = 0; i < spellSprites.Length; i++)
        {
            SpellSpriteEntry entry = spellSprites[i];

            if (entry == null)
            {
                continue;
            }

            spriteLookup[entry.spellType] = entry.sprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }

    [ContextMenu("Auto Populate Spell Sprites")]
    public void AutoPopulateSpellSprites()
    {
        List<SpellSpriteEntry> entries = new List<SpellSpriteEntry>();

        foreach (SpellCardType spellType in Enum.GetValues(typeof(SpellCardType)))
        {
            string path = $"{editorSpellSpriteFolder}/{GetSpriteFileName(spellType)}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            entries.Add(new SpellSpriteEntry(spellType, sprite));
        }

        spellSprites = entries.ToArray();
        BuildLookup();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    private string GetSpriteFileName(SpellCardType spellType)
    {
        switch (spellType)
        {
            case SpellCardType.AuricCovenant:
                return "Auric Covenant";
            case SpellCardType.StoneboundOath:
                return "Stonebound Oath";
            case SpellCardType.FortuneInscription:
                return "Fortune Inscription";
            case SpellCardType.CrimsonSealRite:
                return "Crimson Seal Rite";
            case SpellCardType.GildedSealRite:
                return "Gilded Seal Rite";
            case SpellCardType.HermitsVault:
                return "Hermit’s Vault";
            case SpellCardType.GallowsOffering:
                return "Gallows Offering";
            case SpellCardType.AscendantBlessing:
                return "Ascendant Blessing";
            default:
                return SpellCard.GetName(spellType);
        }
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
