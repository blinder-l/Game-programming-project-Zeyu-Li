using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class JokerSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorJokerSpriteFolder = "Assets/Spirites/Joker Cards";
    [SerializeField] private JokerSpriteEntry[] jokerSprites = new JokerSpriteEntry[0];

    private void Awake()
    {
        EnsureJokerSpritesPopulated();
    }

    public Sprite GetSprite(JokerBase joker)
    {
        if (joker == null)
        {
            return null;
        }

        return GetSprite(joker.SpriteKey);
    }

    public Sprite GetSprite(string jokerName)
    {
        EnsureJokerSpritesPopulated();

        if (string.IsNullOrEmpty(jokerName) || jokerSprites == null)
        {
            return null;
        }

        for (int i = 0; i < jokerSprites.Length; i++)
        {
            JokerSpriteEntry entry = jokerSprites[i];

            if (entry == null || string.IsNullOrEmpty(entry.jokerName))
            {
                continue;
            }

            if (string.Equals(entry.jokerName, jokerName, StringComparison.OrdinalIgnoreCase))
            {
                return entry.sprite;
            }
        }

        Debug.LogWarning($"Missing Joker sprite for SpriteKey: {jokerName}");
        return null;
    }

    private void EnsureJokerSpritesPopulated()
    {
#if UNITY_EDITOR
        if (jokerSprites == null || jokerSprites.Length == 0)
        {
            AutoPopulateJokerSprites();
        }
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (jokerSprites == null || jokerSprites.Length == 0)
        {
            AutoPopulateJokerSprites();
        }
    }

    [ContextMenu("Auto Populate Joker Sprites")]
    private void AutoPopulateJokerSprites()
    {
        List<JokerSpriteEntry> entries = new List<JokerSpriteEntry>();
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { editorJokerSpriteFolder });

        for (int i = 0; i < spriteGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                continue;
            }

            entries.Add(new JokerSpriteEntry(ExtractEnglishJokerName(path), sprite));
        }

        jokerSprites = entries.ToArray();
    }

    private string ExtractEnglishJokerName(string assetPath)
    {
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        int firstEnglishIndex = -1;

        for (int i = 0; i < fileName.Length; i++)
        {
            if (fileName[i] <= 127 && char.IsLetter(fileName[i]))
            {
                firstEnglishIndex = i;
                break;
            }
        }

        if (firstEnglishIndex < 0)
        {
            return fileName;
        }

        return fileName.Substring(firstEnglishIndex).Trim();
    }
#endif
}

[Serializable]
public class JokerSpriteEntry
{
    public string jokerName;
    public Sprite sprite;

    public JokerSpriteEntry(string jokerName, Sprite sprite)
    {
        this.jokerName = jokerName;
        this.sprite = sprite;
    }
}
