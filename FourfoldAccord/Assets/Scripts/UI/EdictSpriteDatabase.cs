using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EdictSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorEdictSpriteFolder = "Assets/Spirites/Edict";
    [SerializeField] private EdictSpriteEntry[] edictSprites = new EdictSpriteEntry[0];

    private void Awake()
    {
        EnsureEdictSpritesPopulated();
    }

    public Sprite GetSprite(EdictCard edictCard)
    {
        if (edictCard == null)
        {
            return null;
        }

        return GetSprite(edictCard.edictType);
    }

    public Sprite GetSprite(EdictCardType edictType)
    {
        EnsureEdictSpritesPopulated();

        if (edictSprites == null)
        {
            return null;
        }

        for (int i = 0; i < edictSprites.Length; i++)
        {
            EdictSpriteEntry entry = edictSprites[i];

            if (entry.edictType == edictType)
            {
                if (entry.sprite == null)
                {
                    Debug.LogWarning($"Missing Edict sprite for {EdictCard.GetName(edictType)}");
                }

                return entry.sprite;
            }
        }

        Debug.LogWarning($"Missing Edict sprite entry for {EdictCard.GetName(edictType)}");
        return null;
    }

    private void EnsureEdictSpritesPopulated()
    {
#if UNITY_EDITOR
        if (edictSprites == null || edictSprites.Length == 0)
        {
            AutoPopulateEdictSprites();
        }
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (edictSprites == null || edictSprites.Length == 0)
        {
            AutoPopulateEdictSprites();
        }
    }

    [ContextMenu("Auto Populate Edict Sprites")]
    private void AutoPopulateEdictSprites()
    {
        List<EdictSpriteEntry> entries = new List<EdictSpriteEntry>();

        foreach (EdictCardType edictType in Enum.GetValues(typeof(EdictCardType)))
        {
            string path = $"{editorEdictSpriteFolder}/{EdictCard.GetName(edictType)}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            entries.Add(new EdictSpriteEntry(edictType, sprite));
        }

        edictSprites = entries.ToArray();
    }
#endif
}

[Serializable]
public class EdictSpriteEntry
{
    public EdictCardType edictType;
    public Sprite sprite;

    public EdictSpriteEntry(EdictCardType edictType, Sprite sprite)
    {
        this.edictType = edictType;
        this.sprite = sprite;
    }
}
