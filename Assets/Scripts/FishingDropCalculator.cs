using System.Collections.Generic;
using UnityEngine;

public static class FishingDropCalculator
{
    public static FishData GetRandomCatch(FishingDatabase db, FishingHotspot hotspot)
    {
        if (db == null || db.FishList == null || db.FishList.Count == 0) return null;

        Dictionary<Rarity, List<FishData>> rarityGroups = GroupFishByRarity(db.FishList);
        Rarity chosenRarity = RollRarity(rarityGroups.Keys, db, hotspot);

        if (!rarityGroups.TryGetValue(chosenRarity, out List<FishData> rarityFishList) || rarityFishList.Count == 0)
            return null;

        return RollFishFromRarity(rarityFishList);
    }

    private static Dictionary<Rarity, List<FishData>> GroupFishByRarity(List<FishData> fishList)
    {
        var groups = new Dictionary<Rarity, List<FishData>>();
        foreach (var fish in fishList)
        {
            if (fish == null) continue;
            if (!groups.ContainsKey(fish.rarity))
                groups[fish.rarity] = new List<FishData>();
            groups[fish.rarity].Add(fish);
        }
        return groups;
    }

    private static Rarity RollRarity(IEnumerable<Rarity> availableRarities, FishingDatabase db, FishingHotspot hotspot)
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

    private static FishData RollFishFromRarity(List<FishData> rarityFishList)
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
            if (rolledWeight <= currentSum) return fish;
        }

        return rarityFishList[0];
    }
}