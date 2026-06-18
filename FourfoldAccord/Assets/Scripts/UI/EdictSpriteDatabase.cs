using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class EdictSpriteDatabase : MonoBehaviour
{
    [SerializeField] private string editorEdictSpriteFolder = "Assets/Spirites/Edict";
    [SerializeField] private EdictSpriteEntry[] edictSprites = new EdictSpriteEntry[0];
    private Dictionary<EdictCardType, Sprite> spriteLookup;

    private void Awake()
    {
        BuildLookup();
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
        EnsureLookup();

        if (spriteLookup == null)
        {
            return null;
        }

        if (spriteLookup.TryGetValue(edictType, out Sprite sprite))
        {
            if (sprite == null)
            {
                Debug.LogWarning($"Missing Edict sprite for {EdictCard.GetName(edictType)}");
            }

            return sprite;
        }

        Debug.LogWarning($"Missing Edict sprite entry for {EdictCard.GetName(edictType)}");
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
        spriteLookup = new Dictionary<EdictCardType, Sprite>();

        if (edictSprites == null)
        {
            return;
        }

        for (int i = 0; i < edictSprites.Length; i++)
        {
            EdictSpriteEntry entry = edictSprites[i];

            if (entry == null)
            {
                continue;
            }

            spriteLookup[entry.edictType] = entry.sprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }

    [ContextMenu("Auto Populate Edict Sprites")]
    public void AutoPopulateEdictSprites()
    {
        List<EdictSpriteEntry> entries = new List<EdictSpriteEntry>();

        foreach (EdictCardType edictType in Enum.GetValues(typeof(EdictCardType)))
        {
            string path = $"{editorEdictSpriteFolder}/{EdictCard.GetName(edictType)}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            entries.Add(new EdictSpriteEntry(edictType, sprite));
        }

        edictSprites = entries.ToArray();
        BuildLookup();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
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
