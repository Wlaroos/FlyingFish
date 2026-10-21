#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FishingDatabaseAutoPopulator : EditorWindow
{
    private string fishAssetFolderPath = "Assets/ScriptableObjects/Fish";
    private string databaseSavePath = "Assets/ScriptableObjects/FishingDatabases";

    [MenuItem("Tools/Fishing/Auto-Create & Populate Databases")]
    public static void ShowWindow()
    {
        GetWindow<FishingDatabaseAutoPopulator>("Auto-Populate Fishing DB");
    }

    private void OnGUI()
    {
        GUILayout.Label("Automatic Fishing Database Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "1. Scan 'Assets/ScriptableObjects/Fish' for all FishData assets.\n" +
            "2. Automatically create a FishingDatabase asset for each WaterType (Lake, Ice, Ocean).\n" +
            "3. Fill each database with its respective matching fish.\n" +
            "4. Automatically link them to the FishingManager in your active scene.", 
            MessageType.Info);

        EditorGUILayout.Space();
        fishAssetFolderPath = EditorGUILayout.TextField("Fish Source Folder", fishAssetFolderPath);
        databaseSavePath = EditorGUILayout.TextField("Database Save Folder", databaseSavePath);

        EditorGUILayout.Space();
        if (GUILayout.Button("Run Auto-Population", GUILayout.Height(35)))
        {
            GenerateAndPopulateDatabases();
        }
    }

    private void GenerateAndPopulateDatabases()
    {
        // Ensure target save folder exists
        EnsureFolderExists(databaseSavePath);

        // Find all FishData assets specifically within the requested folder path
        string[] fishGuids = AssetDatabase.FindAssets("t:FishData", new[] { fishAssetFolderPath });
        List<FishData> allFish = new List<FishData>();
        
        foreach (string guid in fishGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FishData fish = AssetDatabase.LoadAssetAtPath<FishData>(path);
            if (fish != null)
            {
                allFish.Add(fish);
            }
        }

        Debug.Log($"Found {allFish.Count} FishData assets in '{fishAssetFolderPath}'.");

        // Group fish by WaterType
        Dictionary<WaterType, List<FishData>> fishByWaterType = new Dictionary<WaterType, List<FishData>>();
        foreach (WaterType waterType in System.Enum.GetValues(typeof(WaterType)))
        {
            fishByWaterType[waterType] = new List<FishData>();
        }

        foreach (var fish in allFish)
        {
            if (fishByWaterType.ContainsKey(fish.requiredWaterType))
            {
                fishByWaterType[fish.requiredWaterType].Add(fish);
            }
        }

        // Create or load a FishingDatabase asset for each WaterType
        List<FishingDatabase> createdDatabases = new List<FishingDatabase>();

        foreach (var kvp in fishByWaterType)
        {
            WaterType waterType = kvp.Key;
            List<FishData> fishList = kvp.Value;

            string assetPath = $"{databaseSavePath}/{waterType}_FishingDatabase.asset";
            FishingDatabase db = AssetDatabase.LoadAssetAtPath<FishingDatabase>(assetPath);

            if (db == null)
            {
                db = CreateInstance<FishingDatabase>();
                AssetDatabase.CreateAsset(db, assetPath);
                Debug.Log($"Created new FishingDatabase asset at: {assetPath}");
            }

            // Use SerializedObject to safely update private fields inside FishingDatabase
            SerializedObject serializedDb = new SerializedObject(db);
            
            // Set Water Type field
            SerializedProperty waterTypeProp = serializedDb.FindProperty("_waterType");
            if (waterTypeProp != null) waterTypeProp.enumValueIndex = (int)waterType;

            // Populate Fish List
            SerializedProperty fishListProp = serializedDb.FindProperty("_fishList");
            if (fishListProp != null)
            {
                fishListProp.ClearArray();
                fishListProp.arraySize = fishList.Count;
                for (int i = 0; i < fishList.Count; i++)
                {
                    fishListProp.GetArrayElementAtIndex(i).objectReferenceValue = fishList[i];
                }
            }

            serializedDb.ApplyModifiedProperties();
            EditorUtility.SetDirty(db);
            createdDatabases.Add(db);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Automatically assign databases to the FishingManager in the scene
        FishingManager manager = FindFirstObjectByType<FishingManager>();
        if (manager != null)
        {
            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty waterDatabasesProp = serializedManager.FindProperty("_waterDatabases");

            if (waterDatabasesProp != null)
            {
                waterDatabasesProp.ClearArray();
                waterDatabasesProp.arraySize = createdDatabases.Count;
                for (int i = 0; i < createdDatabases.Count; i++)
                {
                    waterDatabasesProp.GetArrayElementAtIndex(i).objectReferenceValue = createdDatabases[i];
                }
                serializedManager.ApplyModifiedProperties();
                EditorUtility.SetDirty(manager);
                Debug.Log("Successfully assigned databases to FishingManager in the scene.");
            }
        }
        else
        {
            Debug.LogWarning("No FishingManager found in the active scene to auto-assign databases to.");
        }

        EditorUtility.DisplayDialog("Success!", "Fishing databases have been successfully generated from your fish directory and assigned!", "OK");
    }

    private void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = nextPath;
            }
        }
    }
}
#endif