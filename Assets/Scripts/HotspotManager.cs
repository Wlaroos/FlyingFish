using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotspotManager : MonoBehaviour
{
    [Header("Hotspot Settings")]
    [SerializeField] private GameObject _hotspotPrefab; // Prefab with FishingHotspot.cs and a Collider2D (Trigger)
    [SerializeField] private List<WaterZone> _activeWaterZones = new List<WaterZone>();

    [Header("Spawn Timing")]
    [SerializeField] private float _minSpawnInterval = 10f;
    [SerializeField] private float _maxSpawnInterval = 25f;

    [Header("Active Duration")]
    [SerializeField] private float _minActiveDuration = 15f;
    [SerializeField] private float _maxActiveDuration = 40f;

    [Header("Limits")]
    [SerializeField] private int _maxConcurrentHotspots = 3;

    private List<GameObject> _spawnedHotspots = new List<GameObject>();

    private void Start()
    {
        // Auto-find water zones in the scene if the list is empty
        if (_activeWaterZones.Count == 0)
        {
            _activeWaterZones.AddRange(FindObjectsByType<WaterZone>(FindObjectsSortMode.None));
        }

        if (_activeWaterZones.Count == 0 || _hotspotPrefab == null)
        {
            Debug.LogWarning("HotspotManager is missing Water Zones or the Hotspot Prefab!");
            return;
        }

        // Start the spawning loop
        StartCoroutine(HotspotLifecycleRoutine());
    }

    private IEnumerator HotspotLifecycleRoutine()
    {
        while (true)
        {
            // Wait for a random interval before trying to spawn a new hotspot
            float waitTime = Random.Range(_minSpawnInterval, _maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            if (_spawnedHotspots.Count < _maxConcurrentHotspots)
            {
                SpawnHotspot();
            }
        }
    }

    private void SpawnHotspot()
    {
        // Pick a random water zone from our list
        WaterZone targetZone = _activeWaterZones[Random.Range(0, _activeWaterZones.Count)];
        Collider2D zoneCollider = targetZone.GetComponent<Collider2D>();

        if (zoneCollider == null) return;

        // Generate a random valid point inside the water zone's bounding box
        Vector2 randomPoint = GetRandomPointInCollider(zoneCollider);

        // Instantiate the hotspot
        GameObject newHotspot = Instantiate(_hotspotPrefab, randomPoint, Quaternion.identity);
        _spawnedHotspots.Add(newHotspot);

        // Determine how long this hotspot will stay active before despawning
        float activeDuration = Random.Range(_minActiveDuration, _maxActiveDuration);
        StartCoroutine(DespawnHotspotRoutine(newHotspot, activeDuration));
    }

    private IEnumerator DespawnHotspotRoutine(GameObject hotspot, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (hotspot != null)
        {
            _spawnedHotspots.Remove(hotspot);
            Destroy(hotspot);
        }
    }

    private Vector2 GetRandomPointInCollider(Collider2D collider)
    {
        Bounds bounds = collider.bounds;
        Vector2 randomPoint;
        int safetyCounter = 0;

        // Loop to ensure the randomly generated point actually falls inside the collider
        do
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            randomPoint = new Vector2(randomX, randomY);
            safetyCounter++;
        } 
        while (!collider.OverlapPoint(randomPoint) && safetyCounter < 30);

        return randomPoint;
    }
}