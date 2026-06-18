using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class PlanetSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorPlanetSpriteFolder = "Assets/Spirites/Planet cards";
    [SerializeField] private PlanetSpriteEntry[] planetSprites = new PlanetSpriteEntry[0];
    private Dictionary<PlanetCardType, Sprite> spriteLookup;

    private void Awake()
    {
        BuildLookup();
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
        EnsureLookup();

        if (spriteLookup == null)
        {
            return null;
        }

        if (spriteLookup.TryGetValue(planetType, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"Missing Planet sprite for {planetType}");
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
        spriteLookup = new Dictionary<PlanetCardType, Sprite>();

        if (planetSprites == null)
        {
            return;
        }

        for (int i = 0; i < planetSprites.Length; i++)
        {
            PlanetSpriteEntry entry = planetSprites[i];

            if (entry == null)
            {
                continue;
            }

            spriteLookup[entry.planetType] = entry.sprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }

    [ContextMenu("Auto Populate Planet Sprites")]
    public void AutoPopulatePlanetSprites()
    {
        List<PlanetSpriteEntry> entries = new List<PlanetSpriteEntry>();

        foreach (PlanetCardType planetType in Enum.GetValues(typeof(PlanetCardType)))
        {
            string path = $"{editorPlanetSpriteFolder}/{planetType}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            entries.Add(new PlanetSpriteEntry(planetType, sprite));
        }

        planetSprites = entries.ToArray();
        BuildLookup();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
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
