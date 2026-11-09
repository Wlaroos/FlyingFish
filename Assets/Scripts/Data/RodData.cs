using UnityEngine;

[CreateAssetMenu(fileName = "NewRod", menuName = "Fishing/Gear/Rod")]
public class RodData : FishingGearData
{
    [Header("Rod Stats")]
    public float castRangeMultiplier = 1f;
    public float lineStrength = 10f;
}