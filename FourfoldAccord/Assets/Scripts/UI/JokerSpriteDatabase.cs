using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class JokerSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorJokerSpriteFolder = "Assets/Spirites/Joker Cards";
    [SerializeField] private JokerSpriteEntry[] jokerSprites = new JokerSpriteEntry[0];
    private Dictionary<string, Sprite> spriteLookup;

    private void Awake()
    {
        BuildLookup();
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
        EnsureLookup();

        if (string.IsNullOrEmpty(jokerName) || spriteLookup == null)
        {
            return null;
        }

        if (spriteLookup.TryGetValue(jokerName, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"Missing Joker sprite for SpriteKey: {jokerName}");
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

        if (jokerSprites == null)
        {
            return;
        }

        for (int i = 0; i < jokerSprites.Length; i++)
        {
            JokerSpriteEntry entry = jokerSprites[i];

            if (entry == null || string.IsNullOrEmpty(entry.jokerName))
            {
                continue;
            }

            spriteLookup[entry.jokerName] = entry.sprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }

    [ContextMenu("Auto Populate Joker Sprites")]
    public void AutoPopulateJokerSprites()
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
        BuildLookup();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
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
