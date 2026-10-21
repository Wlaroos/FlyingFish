using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FishingHotspot : MonoBehaviour
{
    [Header("Hotspot Settings")]
    [Tooltip("Multiplier for wait time. 0.5 means it takes half the time to get a bite.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _waitTimeMultiplier = 0.5f;

    [Tooltip("Multiplier applied to rare fish drop chances while in this spot.")]
    [SerializeField] private float _rareDropMultiplier = 1.5f;

    public float WaitTimeMultiplier => _waitTimeMultiplier;
    public float RareDropMultiplier => _rareDropMultiplier;

    private void Reset()
    {
        // Automatically make sure the collider is set as a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }
}