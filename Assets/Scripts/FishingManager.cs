using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class FishingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Transform _rodTipTransform;
    [SerializeField] private FishingBobber _bobberPrefab;

    [Header("Cast Settings")]
    [SerializeField] private float _maxCastRange = 5f;
    public float MaxCastRange => _maxCastRange;
    [SerializeField] private float _castDuration = 0.6f;

    [Header("Range Visualization")]
    [SerializeField] private bool _showRangeIndicator = true;
    [SerializeField] private int _rangeSegments = 50;
    [SerializeField] private Color _rangeColor = new Color(0f, 1f, 1f, 0.4f);

    [Header("Timing & Mechanics")]
    [SerializeField] private float _baseWaitTime = 2.5f;

    [Header("All Water Databases")]
    [SerializeField] private List<FishingDatabase> _waterDatabases = new List<FishingDatabase>();

    private Dictionary<WaterType, FishingDatabase> _databaseLookup;
    private FishingBobber _activeBobber;
    private bool _isFishing = false;
    private LineRenderer _rangeLineRenderer;

    private void Awake()
    {
        InitializePlayerReference();
        BuildDatabaseLookup();
        SetupRangeIndicator();
    }

    private void Update()
    {
        // Keep the range indicator centered on the player
        if (_showRangeIndicator && _rangeLineRenderer != null && _playerMovement != null)
        {
            transform.position = _playerMovement.transform.position;
            UpdateRangeIndicator();
        }
    }

    private void InitializePlayerReference()
    {
        if (_playerMovement == null)
            _playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (_playerMovement == null)
            Debug.LogWarning("FishingManager: No PlayerMovement found in scene.");
    }

    private void BuildDatabaseLookup()
    {
        _databaseLookup = new Dictionary<WaterType, FishingDatabase>();
        foreach (var db in _waterDatabases)
        {
            if (db != null && !_databaseLookup.ContainsKey(db.WaterType))
            {
                _databaseLookup.Add(db.WaterType, db);
            }
        }
    }

    private void SetupRangeIndicator()
    {
        if (!_showRangeIndicator) return;

        _rangeLineRenderer = GetComponent<LineRenderer>();
        _rangeLineRenderer.positionCount = _rangeSegments + 1;
        _rangeLineRenderer.useWorldSpace = false;
        _rangeLineRenderer.startWidth = 0.05f;
        _rangeLineRenderer.endWidth = 0.05f;

        // Assign a basic unlit/default material if none exists
        if (_rangeLineRenderer.sharedMaterial == null)
        {
            _rangeLineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        _rangeLineRenderer.startColor = _rangeColor;
        _rangeLineRenderer.endColor = _rangeColor;

        UpdateRangeIndicator();
    }

    private void UpdateRangeIndicator()
    {
        if (!_showRangeIndicator || _rangeLineRenderer == null) return;

        float angleStep = 360f / _rangeSegments;
        for (int i = 0; i <= _rangeSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * _maxCastRange;
            float y = Mathf.Sin(angle) * _maxCastRange;
            _rangeLineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    private void OnEnable() => PlayerInputs.OnInteractPressed += OnInteractPerformed;
    private void OnDisable() => PlayerInputs.OnInteractPressed -= OnInteractPerformed;

    private void OnInteractPerformed()
    {
        if (_playerMovement == null || _isFishing) return;

        Vector3 originPos = _playerMovement.transform.position;
        
        if (TryGetTargetCastPosition(originPos, out Vector3 targetPos, out WaterType? targetWater))
        {
            if (targetWater.HasValue)
            {
                StartCoroutine(FishRoutine(targetWater.Value, targetPos));
            }
            else
            {
                Debug.Log("Cannot fish here! Aim your mouse at a water zone within range.");
            }
        }
        else
        {
            Debug.Log("Target is out of casting range!");
        }
    }

    private bool TryGetTargetCastPosition(Vector3 originPoint, out Vector3 targetPos, out WaterType? detectedWater)
    {
        targetPos = Vector3.zero;
        detectedWater = null;

        Vector3 mouseScreenPos = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3 direction = mouseWorldPos - originPoint;
        float distance = direction.magnitude;

        // Strict range check: returns false if clicked outside max range instead of clamping
        if (distance > _maxCastRange)
        {
            return false;
        }

        targetPos = mouseWorldPos;

        Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out WaterZone waterZone))
            {
                detectedWater = waterZone.WaterType;
                break;
            }
        }

        return true;
    }

    private IEnumerator FishRoutine(WaterType waterZoneType, Vector3 targetPos)
    {
        _isFishing = true;
        _playerMovement.SetCanMove(false);

        Transform rodTransform = _rodTipTransform != null ? _rodTipTransform : _playerMovement.transform;

        // Instantiate Bobber & Initialize its Line Renderer
        _activeBobber = Instantiate(_bobberPrefab, rodTransform.position, Quaternion.identity);
        _activeBobber.InitializeLine(rodTransform);

        // Perform Cast Physics Arc
        yield return StartCoroutine(_activeBobber.CastToPosition(targetPos, _castDuration));

        // Check for active FishingHotspot where the bobber landed
        FishingHotspot activeHotspot = GetHotspotAtPosition(targetPos);

        // Waiting Phase
        float waitTime = _baseWaitTime;
        if (activeHotspot != null)
            waitTime *= activeHotspot.WaitTimeMultiplier;

        yield return new WaitForSeconds(waitTime);

        // Fish Bite Tug Animation
        yield return StartCoroutine(_activeBobber.TriggerBiteAnimation());

        // Catch Processing & Visual Cleanup
        ProcessCatch(waterZoneType, activeHotspot);
        
        if (_activeBobber != null)
        {
            Destroy(_activeBobber.gameObject);
        }

        _playerMovement.SetCanMove(true);
        _isFishing = false;
    }

    private FishingHotspot GetHotspotAtPosition(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(position);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out FishingHotspot hotspot))
            {
                return hotspot;
            }
        }
        return null;
    }

    private void ProcessCatch(WaterType zoneType, FishingHotspot hotspot)
    {
        if (!_databaseLookup.TryGetValue(zoneType, out FishingDatabase activeDb))
        {
            Debug.LogWarning($"No FishingDatabase assigned for water type: {zoneType}");
            return;
        }

        FishData caughtFish = FishingDropCalculator.GetRandomCatch(activeDb, hotspot);
        if (caughtFish != null)
        {
            string spotText = hotspot != null ? " (Hotspot Bonus Active!)" : "";
            Debug.Log($"Success! You caught a {caughtFish.rarity} {caughtFish.fishName} from the {zoneType}!{spotText}");
        }
        else
        {
            Debug.Log("No fish caught.");
        }
    }

    public FishingDatabase GetDatabaseForWaterType(WaterType waterType)
    {
        if (_databaseLookup.TryGetValue(waterType, out FishingDatabase db))
        {
            return db;
        }
        return null;
    }
}