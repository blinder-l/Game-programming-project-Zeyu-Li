using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlanetSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorPlanetSpriteFolder = "Assets/Spirites/Planet cards";
    [SerializeField] private PlanetSpriteEntry[] planetSprites = new PlanetSpriteEntry[0];

    private void Awake()
    {
        EnsurePlanetSpritesPopulated();
    }

    public Sprite GetSprite(PlanetCard planetCard)
    {
        if (planetCard == null)
        {
            return null;
        }

        return GetSprite(planetCard.planetType);
    }

    public Sprite GetSprite(PlanetCardType planetType)
    {
        EnsurePlanetSpritesPopulated();

        if (planetSprites == null)
        {
            return null;
        }

        for (int i = 0; i < planetSprites.Length; i++)
        {
            PlanetSpriteEntry entry = planetSprites[i];

            if (entry.planetType == planetType)
            {
                return entry.sprite;
            }
        }

        return null;
    }

    private void EnsurePlanetSpritesPopulated()
    {
#if UNITY_EDITOR
        if (planetSprites == null || planetSprites.Length == 0)
        {
            AutoPopulatePlanetSprites();
        }
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (planetSprites == null || planetSprites.Length == 0)
        {
            AutoPopulatePlanetSprites();
        }
    }

    [ContextMenu("Auto Populate Planet Sprites")]
    private void AutoPopulatePlanetSprites()
    {
        List<PlanetSpriteEntry> entries = new List<PlanetSpriteEntry>();

        foreach (PlanetCardType planetType in Enum.GetValues(typeof(PlanetCardType)))
        {
            string path = $"{editorPlanetSpriteFolder}/{planetType}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            entries.Add(new PlanetSpriteEntry(planetType, sprite));
        }

        planetSprites = entries.ToArray();
    }
#endif
}

[Serializable]
public class PlanetSpriteEntry
{
    public PlanetCardType planetType;
    public Sprite sprite;

    public PlanetSpriteEntry(PlanetCardType planetType, Sprite sprite)
    {
        this.planetType = planetType;
        this.sprite = sprite;
    }
}
