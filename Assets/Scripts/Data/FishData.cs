using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[System.Serializable]

[CreateAssetMenu(fileName = "NewFish", menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    public string fishName;
    public WaterType requiredWaterType;
    public Rarity rarity;
    [Range(0f, 100f)] public float dropChance = 50f;
    public Sprite fishImage;
}