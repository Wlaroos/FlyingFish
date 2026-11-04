using System;
using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[Serializable]

[CreateAssetMenu(fileName = "NewFish", menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    public string fishName;
    public Sprite fishImage;
    public WaterType requiredWaterType;
    public Rarity rarity;
    [Range(0f, 100f)] public float dropChance = 50f;
    [MinMaxRange(20f, 80f)] public Vector2 minMaxWeight;
}