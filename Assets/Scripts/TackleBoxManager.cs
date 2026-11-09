using UnityEngine;
using System;

public class TackleBoxManager : MonoBehaviour
{
    public static TackleBoxManager Instance { get; private set; }

    [Header("Default Starter Gear")]
    [SerializeField] private RodData _defaultRod;
    [SerializeField] private BaitData _defaultBait;
    [SerializeField] private LureData _defaultLure;
    [Header("Currently Equipped Gear")]
    public RodData EquippedRod;
    public BaitData EquippedBait;
    public LureData EquippedLure;

    public event Action OnGearChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        // Only assign defaults if no gear is currently loaded (e.g., first-time play)
        if (EquippedRod == null && _defaultRod != null) EquipRod(_defaultRod);
        if (EquippedBait == null && _defaultBait != null) EquipBait(_defaultBait);
        if (EquippedLure == null && _defaultLure != null) EquipLure(_defaultLure);
    }

    public void EquipRod(RodData newRod)
    {
        EquippedRod = newRod;
        //Debug.Log($"Equipped Rod: {newRod.gearName}");
        OnGearChanged?.Invoke();
    }

    public void EquipBait(BaitData newBait)
    {
        EquippedBait = newBait;
        //Debug.Log($"Equipped Bait: {newBait.gearName}");
        OnGearChanged?.Invoke();
    }

    public void EquipLure(LureData newLure)
    {
        EquippedLure = newLure;
        //Debug.Log($"Equipped Lure: {newLure.gearName}");
        OnGearChanged?.Invoke();
    }
}