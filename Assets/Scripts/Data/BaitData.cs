using UnityEngine;

[CreateAssetMenu(fileName = "NewBait", menuName = "Fishing/Gear/Bait")]
public class BaitData : FishingGearData
{
    [Header("Bait Stats")]
    public WaterType preferredWater;
    public float biteSpeedMultiplier = 1f;
}