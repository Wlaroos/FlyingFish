using UnityEngine;

public enum GearType { Rod, Bait, Lure }

public abstract class FishingGearData : ScriptableObject
{
    public string gearName;
    public Sprite gearIcon;
    public GearType gearType;
    [TextArea] public string description;
}