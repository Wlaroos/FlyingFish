using UnityEngine;

public enum WaterType
{
    Lake,
    Ice,
    Ocean
}

[RequireComponent(typeof(Collider2D))]
public class WaterZone : MonoBehaviour
{
    [Header("Zone Configuration")]
    [SerializeField] private WaterType waterType = WaterType.Lake;

    public WaterType WaterType => waterType;

    private void Reset()
    {
        // Ensure collider is set to trigger automatically when added
        GetComponent<Collider2D>().isTrigger = true;
    }
}