#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SpriteDatabaseSceneSetup
{
    private const string DatabaseObjectName = "SpriteDatabases";

    [MenuItem("Fourfold Accord/Sprite Databases/Create Or Populate Scene Databases")]
    public static void CreateOrPopulateSceneDatabases()
    {
        GameObject databaseObject = GameObject.Find(DatabaseObjectName);

        if (databaseObject == null)
        {
            databaseObject = new GameObject(DatabaseObjectName);
            Undo.RegisterCreatedObjectUndo(databaseObject, "Create SpriteDatabases");
        }

        CardSpriteDatabase cardDatabase = EnsureComponent<CardSpriteDatabase>(databaseObject);
        CardModifierSpriteDatabase modifierDatabase = EnsureComponent<CardModifierSpriteDatabase>(databaseObject);
        JokerSpriteDatabase jokerDatabase = EnsureComponent<JokerSpriteDatabase>(databaseObject);
        PlanetSpriteDatabase planetDatabase = EnsureComponent<PlanetSpriteDatabase>(databaseObject);
        SpellSpriteDatabase spellDatabase = EnsureComponent<SpellSpriteDatabase>(databaseObject);
        EdictSpriteDatabase edictDatabase = EnsureComponent<EdictSpriteDatabase>(databaseObject);

        cardDatabase.AutoPopulateCardSprites();
        modifierDatabase.AutoPopulateModifierSprites();
        jokerDatabase.AutoPopulateJokerSprites();
        planetDatabase.AutoPopulatePlanetSprites();
        spellDatabase.AutoPopulateSpellSprites();
        edictDatabase.AutoPopulateEdictSprites();

        EditorUtility.SetDirty(databaseObject);
        EditorSceneManager.MarkSceneDirty(databaseObject.scene);
        Selection.activeGameObject = databaseObject;

        Debug.Log("SpriteDatabases scene object created/updated and all sprite database entries populated. Save the scene to persist build-safe Sprite references.");
    }

    private static T EnsureComponent<T>(GameObject databaseObject) where T : Component
    {
        T component = databaseObject.GetComponent<T>();

        if (component != null)
        {
            return component;
        }

        component = Undo.AddComponent<T>(databaseObject);
        EditorUtility.SetDirty(databaseObject);
        return component;
    }
}
#endif
