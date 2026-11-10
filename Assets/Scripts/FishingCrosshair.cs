using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Image))]
public class FishingCrosshair : MonoBehaviour
{
    public enum CrosshairState
    {
        Default,
        OutOfRange,
        InRange,
        HoverSolidIce
    }

    [Header("References")]
    [SerializeField] private FishingManager _fishingManager;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private RectTransform _canvasRectTransform;

    [Header("Tilemap & Tiles")]
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private TileBase _solidIceTile;
    [SerializeField] private TileBase _iceHoleTile;
    [SerializeField] private TileBase _iceHole2Tile;
    [SerializeField] private TileBase _iceHole3Tile;
    [SerializeField] private IceRefreeze _refreezeManager;

    [Header("Ice Hole Rules")]
    [SerializeField] private bool _preventAdjacentIceHoles = true;

    [Header("Crosshair Visuals (Stays on Mouse)")]
    [SerializeField] private Image _crosshairImage;
    [SerializeField] private Sprite _defaultSprite;      // Not over valid target
    [SerializeField] private Sprite _outOfRangeSprite;   // Over valid target, but too far
    [SerializeField] private Sprite _inRangeSprite;      // Valid target & in range

    [Header("Tile Overlay Visuals (Appears on Solid Ice in World)")]
    [SerializeField] private SpriteRenderer _tileHighlightRenderer; // World-space SpriteRenderer
    [SerializeField] private Sprite _tileOverlaySprite;             // Hover overlay sprite

    [Header("Scaling & Alignment")]
    [SerializeField] private float _crosshairScale = 1.0f;

    private Canvas _rootCanvas;
    private Camera _mainCamera;
    private Vector2 _spritePivotOffset;
    
    private Vector3Int _currentHoveredCell;
    private CrosshairState _currentState;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _rootCanvas = GetComponentInParent<Canvas>();
        Cursor.visible = false;

        if (_crosshairImage == null) _crosshairImage = GetComponent<Image>();
        if (_canvasRectTransform == null && _rootCanvas != null) _canvasRectTransform = _rootCanvas.GetComponent<RectTransform>();
        if (_fishingManager == null) _fishingManager = FindFirstObjectByType<FishingManager>();

        if (_tileHighlightRenderer != null && _tileOverlaySprite != null)
        {
            _tileHighlightRenderer.sprite = _tileOverlaySprite;
        }
    }

    private void Update()
    {
        UpdateCrosshairState();
        UpdateCrosshairPosition();
        HandleInputActions();
    }

    private void UpdateCrosshairPosition()
    {
        if (Mouse.current == null) return;
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay || 
           (_rootCanvas.renderMode == RenderMode.ScreenSpaceCamera && _rootCanvas.worldCamera == null))
        {
            transform.position = mouseScreenPos + _spritePivotOffset;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform, mouseScreenPos, _rootCanvas.worldCamera, out Vector2 localPoint
            );
            transform.localPosition = localPoint + _spritePivotOffset;
        }
    }

    private void UpdateCrosshairState()
    {
        // Hide the tile overlay by default each frame
        if (_tileHighlightRenderer != null) _tileHighlightRenderer.gameObject.SetActive(false);

        // If references are missing, fallback to default state
        if (_fishingManager == null || _playerMovement == null || _tilemap == null)
        {
            SetCrosshairVisuals(_defaultSprite);
            _currentState = CrosshairState.Default;
            return;
        }

        Vector2 mouseScreenPos = Mouse.current != null ? (Vector2)Mouse.current.position.ReadValue() : Input.mousePosition;
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -_mainCamera.transform.position.z));
        mouseWorldPos.z = 0f;

        Vector2 originPos = _playerMovement.transform.position;
        float distance = Vector2.Distance(originPos, mouseWorldPos);
        bool isInRange = distance <= _fishingManager.MaxCastRange;

        Vector3Int cellPos = _tilemap.WorldToCell(mouseWorldPos);
        TileBase tile = _tilemap.GetTile(cellPos);

        // If hovering over a valid tile (Solid Ice or Ice Hole)
        if (tile != null)
        {
            _currentHoveredCell = cellPos;

            if (tile == _solidIceTile)
            {
                bool hasAdjacentHole = _preventAdjacentIceHoles && HasAdjacentIceHole(cellPos);

                if (!isInRange || hasAdjacentHole)
                {
                    SetCrosshairVisuals(_outOfRangeSprite);
                    _currentState = CrosshairState.OutOfRange;
                }
                else
                {
                    // Show tile overlay only when hovering over solid ice in range and valid
                    if (_tileHighlightRenderer != null)
                    {
                        _tileHighlightRenderer.gameObject.SetActive(true);
                        _tileHighlightRenderer.transform.position = _tilemap.GetCellCenterWorld(cellPos);
                    }

                    SetCrosshairVisuals(_inRangeSprite);
                    _currentState = CrosshairState.HoverSolidIce;
                }
                return;
            }
            else // Existing ice hole or other designated tiles
            {
                WaterType? detectedWater = GetWaterAtPosition(mouseWorldPos);
                bool isOverWater = detectedWater.HasValue;

                if (!isOverWater)
                {
                    SetCrosshairVisuals(_defaultSprite);
                    _currentState = CrosshairState.Default;
                    return;
                }
                else
                {
                    if (!isInRange)
                    {
                        SetCrosshairVisuals(_outOfRangeSprite);
                        _currentState = CrosshairState.OutOfRange;
                        return;
                    }
                    else
                    {
                        SetCrosshairVisuals(_inRangeSprite);
                        _currentState = CrosshairState.InRange;
                        return;
                    }
                }
            }
        }

        //Default State (Not over any tile / Empty tilemap space)
        SetCrosshairVisuals(_defaultSprite);
        _currentState = CrosshairState.Default;
    }

    private WaterType? GetWaterAtPosition(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(position);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out WaterZone waterZone))
            {
                return waterZone.WaterType;
            }
        }
        return null;
    }

    private bool HasAdjacentIceHole(Vector3Int centerCell)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (var dir in directions)
        {
            if (_tilemap.GetTile(centerCell + dir) == _iceHoleTile || _tilemap.GetTile(centerCell + dir) == _iceHole2Tile || _tilemap.GetTile(centerCell + dir) == _iceHole3Tile)
            {
                return true;
            }
        }
        return false;
    }

    private void HandleInputActions()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // If hovering over solid ice in range, click to break it into an ice hole
            if (_currentState == CrosshairState.HoverSolidIce && _tilemap != null)
            {
                if (_preventAdjacentIceHoles && HasAdjacentIceHole(_currentHoveredCell))
                {
                    Debug.LogWarning($"Cannot place an ice hole adjacent to another ice hole at cell {_currentHoveredCell}");
                    return;
                }

                _tilemap.SetTile(_currentHoveredCell, _iceHoleTile);
                _refreezeManager.StartRefreezingTile(_currentHoveredCell);

                Debug.Log($"Broke solid ice at cell {_currentHoveredCell} into an ice hole");
            }
        }
    }

    private void SetCrosshairVisuals(Sprite targetSprite)
    {
        if (targetSprite != null)
        {
            _crosshairImage.sprite = targetSprite;
            _crosshairImage.color = Color.white; 

            RectTransform rectTransform = _crosshairImage.rectTransform;
            Vector2 spriteSize = targetSprite.rect.size;
            rectTransform.sizeDelta = spriteSize * _crosshairScale;

            Vector2 normalizedPivot = new Vector2(
                targetSprite.pivot.x / spriteSize.x,
                targetSprite.pivot.y / spriteSize.y
            );

            _spritePivotOffset = new Vector2(
                (0.5f - normalizedPivot.x) * rectTransform.sizeDelta.x,
                (0.5f - normalizedPivot.y) * rectTransform.sizeDelta.y
            );
        }
        else
        {
            _spritePivotOffset = Vector2.zero;
        }
    }
}