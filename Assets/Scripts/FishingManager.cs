using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement _playerMovement; 
    [SerializeField] private float _baseWaitTime = 2.5f;

    [Header("All Water Databases")]
    [SerializeField] private List<FishingDatabase> _waterDatabases = new List<FishingDatabase>();

    private Dictionary<WaterType, FishingDatabase> _databaseLookup;
    private bool _isFishing = false;

    private void Awake()
    {
        InitializePlayerReference();
        BuildDatabaseLookup();
    }

    private void InitializePlayerReference()
    {
        if (_playerMovement == null)
            _playerMovement = FindFirstObjectByType<PlayerMovement>();
            
        if (_playerMovement == null)
            Debug.LogWarning("FishingManager: No PlayerMovement found in scene.");
    }

    private void BuildDatabaseLookup()
    {
        _databaseLookup = new Dictionary<WaterType, FishingDatabase>();
        foreach (var db in _waterDatabases)
        {
            if (db != null && !_databaseLookup.ContainsKey(db.WaterType))
            {
                _databaseLookup.Add(db.WaterType, db);
            }
        }
    }

    private void OnEnable() => PlayerInputs.OnInteractPressed += OnInteractPerformed;
    private void OnDisable() => PlayerInputs.OnInteractPressed -= OnInteractPerformed;

    private void OnInteractPerformed()
    {
        if (_playerMovement == null || _isFishing) return;

        WaterType? currentWater = _playerMovement.CurrentWaterZone;
        FishingHotspot currentHotspot = _playerMovement.CurrentHotspot; // or CurrentHotspot property

        if (currentWater.HasValue)
        {
            StartCoroutine(FishRoutine(currentWater.Value, currentHotspot));
        }
    }

    private IEnumerator FishRoutine(WaterType waterZone, FishingHotspot hotspot)
    {
        Debug.Log($"Started fishing in {waterZone} water. Please wait...");
        _isFishing = true;
        
        _playerMovement.SetCanMove(false);

        float waitTime = _baseWaitTime;
        if (hotspot != null)
            waitTime *= hotspot.WaitTimeMultiplier;

        yield return new WaitForSeconds(waitTime);

        CatchFish(waterZone, hotspot);
        
        _playerMovement.SetCanMove(true);
        _isFishing = false;
    }

    private void CatchFish(WaterType zoneType, FishingHotspot hotspot)
    {
        if (!_databaseLookup.TryGetValue(zoneType, out FishingDatabase activeDb))
        {
            Debug.LogWarning($"No FishingDatabase assigned for water type: {zoneType}");
            return;
        }

        List<FishData> eligibleFish = activeDb.FishList;
        if (eligibleFish == null || eligibleFish.Count == 0)
        {
            Debug.Log("No fish found in this database area");
            return;
        }

        // Group by Rarity
        Dictionary<Rarity, List<FishData>> rarityGroups = new Dictionary<Rarity, List<FishData>>();
        foreach (var fish in eligibleFish)
        {
            if (fish == null) continue;
            if (!rarityGroups.ContainsKey(fish.rarity))
                rarityGroups[fish.rarity] = new List<FishData>();
            
            rarityGroups[fish.rarity].Add(fish);
        }

        Rarity chosenRarity = RollRarity(rarityGroups.Keys, activeDb, hotspot);

        if (!rarityGroups.TryGetValue(chosenRarity, out List<FishData> rarityFishList) || rarityFishList.Count == 0)
        {
            Debug.Log("No fish available for the rolled rarity!");
            return;
        }

        FishData caughtFish = RollFishFromRarity(rarityFishList);
        if (caughtFish != null)
        {
            string spotText = hotspot != null ? " (Hotspot Bonus Active!)" : "";
            Debug.Log($"Success! You caught a {caughtFish.rarity} {caughtFish.fishName} from the {zoneType}!{spotText}");
        }
    }

    private Rarity RollRarity(Dictionary<Rarity, List<FishData>>.KeyCollection availableRarities, FishingDatabase db, FishingHotspot hotspot)
    {
        float totalWeight = 0f;
        Dictionary<Rarity, float> calculatedWeights = new Dictionary<Rarity, float>();

        foreach (var rarity in availableRarities)
        {
            float weight = db.GetRarityWeight(rarity);
            
            if (hotspot != null && rarity >= Rarity.Rare)
            {
                weight *= hotspot.RareDropMultiplier;
            }

            calculatedWeights[rarity] = weight;
            totalWeight += weight;
        }

        if (totalWeight <= 0f) return Rarity.Common;

        float roll = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        foreach (var rarity in availableRarities)
        {
            currentSum += calculatedWeights[rarity];
            if (roll <= currentSum) return rarity;
        }

        return Rarity.Common;
    }

    private FishData RollFishFromRarity(List<FishData> rarityFishList)
    {
        float totalWeight = 0f;
        foreach (var fish in rarityFishList) 
        {
            if (fish != null) totalWeight += fish.dropChance;
        }

        if (totalWeight <= 0f) return rarityFishList[0];

        float rolledWeight = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        foreach (var fish in rarityFishList)
        {
            if (fish == null) continue;
            currentSum += fish.dropChance;
            if (rolledWeight <= currentSum)
            {
                return fish;
            }
        }

        return rarityFishList[0];
    }

    public FishingDatabase GetDatabaseForWaterType(WaterType waterType)
    {
        if (_waterDatabases == null) return null;
        return _waterDatabases.Find(db => db != null && db.WaterType == waterType);
    }
}