using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWaterDatabase", menuName = "Fishing/Water Fishing Database")]
public class FishingDatabase : ScriptableObject
{
    [Header("Water Type for this Database")]
    [SerializeField] private WaterType _waterType;
    public WaterType WaterType => _waterType;

    [Header("Global Rarity Roll Weights")]
    [SerializeField] private float _commonWeight = 60f;
    [SerializeField] private float _uncommonWeight = 25f;
    [SerializeField] private float _rareWeight = 10f;
    [SerializeField] private float _epicWeight = 4f;
    [SerializeField] private float _legendaryWeight = 1f;

    [Header("Fish in this Water Type")]
    [SerializeField] private List<FishData> _fishList = new List<FishData>();
    public List<FishData> FishList => _fishList;

    public float GetRarityWeight(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return _commonWeight;
            case Rarity.Uncommon: return _uncommonWeight;
            case Rarity.Rare: return _rareWeight;
            case Rarity.Epic: return _epicWeight;
            case Rarity.Legendary: return _legendaryWeight;
            default: return 10f;
        }
    }
}