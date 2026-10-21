#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FishingDatabaseEditor : EditorWindow
{
    private FishingManager targetManager;
    private FishingDatabase targetDatabase;
    private WaterType selectedWaterType = WaterType.Lake;
    private Vector2 scrollPos;

    [MenuItem("Tools/Fishing Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<FishingDatabaseEditor>("Fishing Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Fishing Database Mass Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Assign or auto-find FishingManager
        targetManager = (FishingManager)EditorGUILayout.ObjectField("Fishing Manager", targetManager, typeof(FishingManager), true);

        if (targetManager == null)
        {
            targetManager = FindFirstObjectByType<FishingManager>();
        }

        EditorGUILayout.Space();
        selectedWaterType = (WaterType)EditorGUILayout.EnumPopup("Filter by Water Type", selectedWaterType);
        EditorGUILayout.Space();

        // Automatically pull the correct database from the manager based on the selected water type
        if (targetManager != null)
        {
            targetDatabase = targetManager.GetDatabaseForWaterType(selectedWaterType);
        }

        // Allow manually overriding/assigning the database asset directly if preferred
        targetDatabase = (FishingDatabase)EditorGUILayout.ObjectField("Fishing Database Asset", targetDatabase, typeof(FishingDatabase), false);

        if (targetDatabase == null)
        {
            EditorGUILayout.HelpBox($"No FishingDatabase found in the Manager for '{selectedWaterType}'. Assign one manually or run the Auto-Populator tool.", MessageType.Warning);
            return;
        }

        SerializedObject serializedDatabase = new SerializedObject(targetDatabase);

        EditorGUILayout.Space();

        // Global Rarity Distribution Weight Configuration Section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Global Tier Distribution Roll Weights", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Controls how likely the player is to hit a specific rarity tier across all water types.", MessageType.Info);

        SerializedProperty commonW = serializedDatabase.FindProperty("_commonWeight");
        SerializedProperty uncommonW = serializedDatabase.FindProperty("_uncommonWeight");
        SerializedProperty rareW = serializedDatabase.FindProperty("_rareWeight");
        SerializedProperty epicW = serializedDatabase.FindProperty("_epicWeight");
        SerializedProperty legendaryW = serializedDatabase.FindProperty("_legendaryWeight");

        EditorGUILayout.PropertyField(commonW, new GUIContent("Common Weight"));
        EditorGUILayout.PropertyField(uncommonW, new GUIContent("Uncommon Weight"));
        EditorGUILayout.PropertyField(rareW, new GUIContent("Rare Weight"));
        EditorGUILayout.PropertyField(epicW, new GUIContent("Epic Weight"));
        EditorGUILayout.PropertyField(legendaryW, new GUIContent("Legendary Weight"));

        float totalTierWeight = commonW.floatValue + uncommonW.floatValue + rareW.floatValue + epicW.floatValue + legendaryW.floatValue;
        EditorGUILayout.LabelField($"Total Tier Roll Weight: {totalTierWeight:F1}", EditorStyles.miniBoldLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        SerializedProperty fishDatabaseProp = serializedDatabase.FindProperty("_fishList");

        if (fishDatabaseProp == null || fishDatabaseProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("Your Fish Database is currently empty. Add fish to your FishingDatabase asset.", MessageType.Info);
            serializedDatabase.ApplyModifiedProperties();
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        bool modified = false;

        EditorGUILayout.LabelField($"Fish in {selectedWaterType} Zones (Grouped by Rarity):", EditorStyles.boldLabel);

        Dictionary<Rarity, List<(SerializedProperty fishProp, SerializedObject fishObj, SerializedProperty nameProp, SerializedProperty dropProp, int index)>> rarityGroups = 
            new Dictionary<Rarity, List<(SerializedProperty, SerializedObject, SerializedProperty, SerializedProperty, int)>>();

        for (int i = 0; i < fishDatabaseProp.arraySize; i++)
        {
            SerializedProperty fishElement = fishDatabaseProp.GetArrayElementAtIndex(i);
            if (fishElement.objectReferenceValue == null) continue;

            SerializedObject fishObject = new SerializedObject(fishElement.objectReferenceValue);
            SerializedProperty nameProp = fishObject.FindProperty("fishName");
            SerializedProperty waterProp = fishObject.FindProperty("requiredWaterType");
            SerializedProperty dropChanceProp = fishObject.FindProperty("dropChance");
            SerializedProperty rarityProp = fishObject.FindProperty("rarity");

            if ((WaterType)waterProp.enumValueIndex == selectedWaterType)
            {
                Rarity rarity = (Rarity)rarityProp.enumValueIndex;
                if (!rarityGroups.ContainsKey(rarity))
                {
                    rarityGroups[rarity] = new List<(SerializedProperty, SerializedObject, SerializedProperty, SerializedProperty, int)>();
                }
                rarityGroups[rarity].Add((fishElement, fishObject, nameProp, dropChanceProp, i));
            }
        }

        if (rarityGroups.Count == 0)
        {
            EditorGUILayout.HelpBox($"No fish are currently assigned to the '{selectedWaterType}' water type.", MessageType.None);
        }
        else
        {
            foreach (var kvp in rarityGroups)
            {
                Rarity rarity = kvp.Key;
                var fishList = kvp.Value;

                EditorGUILayout.BeginVertical("box");
                
                float rarityTotal = 0f;
                foreach (var item in fishList)
                {
                    rarityTotal += item.dropProp.floatValue;
                }

                GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
                GUI.backgroundColor = Mathf.Abs(rarityTotal - 100f) > 0.01f ? new Color(1f, 0.6f, 0.6f) : Color.white;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{rarity} Tier (Total: {rarityTotal:F1}%)", headerStyle);

                if (GUILayout.Button("Normalize to 100%", GUILayout.Width(130)))
                {
                    if (fishList.Count > 0 && rarityTotal > 0f)
                    {
                        float factor = 100f / rarityTotal;
                        foreach (var item in fishList)
                        {
                            item.dropProp.floatValue *= factor;
                            item.fishObj.ApplyModifiedProperties();
                        }
                        modified = true;
                    }
                }

                if (GUILayout.Button("Equalize Split", GUILayout.Width(100)))
                {
                    if (fishList.Count > 0)
                    {
                        float equalShare = 100f / fishList.Count;
                        foreach (var item in fishList)
                        {
                            item.dropProp.floatValue = equalShare;
                            item.fishObj.ApplyModifiedProperties();
                        }
                        modified = true;
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;

                if (Mathf.Abs(rarityTotal - 100f) > 0.01f)
                {
                    EditorGUILayout.HelpBox($"Drop chances should add up to 100% (Current: {rarityTotal:F1}%). Click 'Normalize' to scale them automatically.", MessageType.Warning);
                }

                foreach (var item in fishList)
                {
                    EditorGUILayout.BeginHorizontal();
                    string displayName = string.IsNullOrEmpty(item.nameProp.stringValue) ? $"Fish {item.index}" : item.nameProp.stringValue;
                    EditorGUILayout.LabelField($"   {displayName}", GUILayout.Width(160));

                    float oldChance = item.dropProp.floatValue;
                    float newChance = EditorGUILayout.Slider(oldChance, 0f, 100f);
                    if (!Mathf.Approximately(newChance, oldChance))
                    {
                        item.dropProp.floatValue = newChance;
                        item.fishObj.ApplyModifiedProperties();
                        modified = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.EndScrollView();

        serializedDatabase.ApplyModifiedProperties();
        if (modified)
        {
            EditorUtility.SetDirty(targetDatabase);
        }
    }
}
#endif