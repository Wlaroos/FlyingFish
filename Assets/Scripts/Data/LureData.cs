using UnityEngine;

[CreateAssetMenu(fileName = "NewLure", menuName = "Fishing/Gear/Lure")]
public class LureData : FishingGearData
{
    [Header("Lure Stats")]
    public Rarity minGuaranteedRarity;
    public float rareDropBonus = 1f;
}