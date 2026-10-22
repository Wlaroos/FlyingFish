using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Image))]
public class FishingCrosshair : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FishingManager _fishingManager;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private RectTransform _canvasRectTransform;

    [Header("Crosshair Visuals & Sprites")]
    [SerializeField] private Image _crosshairImage;
    [SerializeField] private Sprite _defaultSprite;     // Not over water
    [SerializeField] private Sprite _outOfRangeSprite;  // Over water, but too far
    [SerializeField] private Sprite _inRangeSprite;     // Valid water & in range

    [Header("Scaling & Alignment")]
    [SerializeField, Tooltip("Multiplier applied to the sprite's actual pixel size.")]
    private float _crosshairScale = 1.0f;

    [Header("Optional Colors (If using color tints instead of sprites)")]
    [SerializeField] private Color _defaultColor = Color.white;
    [SerializeField] private Color _outOfRangeColor = Color.yellow;
    [SerializeField] private Color _inRangeColor = Color.green;

    private Canvas _rootCanvas;
    private Camera _mainCamera;
    private Vector2 _spritePivotOffset;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _rootCanvas = GetComponentInParent<Canvas>();

        Cursor.visible = false;

        if (_crosshairImage == null)
        {
            _crosshairImage = GetComponent<Image>();
        }

        if (_canvasRectTransform == null && _rootCanvas != null)
        {
            _canvasRectTransform = _rootCanvas.GetComponent<RectTransform>();
        }

        if (_fishingManager == null)
        {
            _fishingManager = FindFirstObjectByType<FishingManager>();
        }
    }

    private void Update()
    {
        UpdateCrosshairState();
        UpdateCrosshairPosition();
    }

    private void UpdateCrosshairPosition()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // Handle different Canvas render modes (Screen Space - Overlay vs Camera)
        if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay || 
           (_rootCanvas.renderMode == RenderMode.ScreenSpaceCamera && _rootCanvas.worldCamera == null))
        {
            transform.position = mouseScreenPos + _spritePivotOffset;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform, 
                mouseScreenPos, 
                _rootCanvas.worldCamera, 
                out Vector2 localPoint
            );
            transform.localPosition = localPoint + _spritePivotOffset;
        }
    }

    private void UpdateCrosshairState()
    {
        if (_fishingManager == null) return;

        // Get mouse world position
        Vector2 mouseScreenPos = Mouse.current != null ? (Vector2)Mouse.current.position.ReadValue() : Input.mousePosition;
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -_mainCamera.transform.position.z));
        mouseWorldPos.z = 0f;

        // Get player position from PlayerMovement component
        Vector2 originPos = _playerMovement.transform.position;

        // Check if there is a water zone at the mouse position
        WaterType? detectedWater = GetWaterAtPosition(mouseWorldPos);
        bool isOverWater = detectedWater.HasValue;

        if (!isOverWater)
        {
            SetStateVisuals(_defaultSprite, _defaultColor);
            return;
        }

        // Check distance range
        float distance = Vector2.Distance(originPos, mouseWorldPos);
        float maxRange = _fishingManager.MaxCastRange;

        if (distance > maxRange)
        {
            SetStateVisuals(_outOfRangeSprite, _outOfRangeColor);
        }
        else
        {
            SetStateVisuals(_inRangeSprite, _inRangeColor);
        }
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

    private void SetStateVisuals(Sprite targetSprite, Color targetColor)
    {
        if (targetSprite != null)
        {
            _crosshairImage.sprite = targetSprite;
            _crosshairImage.color = Color.white; // Reset tint if using unique sprites

            RectTransform rectTransform = _crosshairImage.rectTransform;
            Vector2 spriteSize = targetSprite.rect.size;

            // Scale relative to actual sprite size and scaled
            rectTransform.sizeDelta = spriteSize * _crosshairScale;

            // Calculate the sprite's custom pivot offset so it aligns correctly with the mouse
            Vector2 normalizedPivot = new Vector2(
                targetSprite.pivot.x / spriteSize.x,
                targetSprite.pivot.y / spriteSize.y
            );

            // Offset from UI center (0.5, 0.5) to the sprite's pivot point, factored by size
            _spritePivotOffset = new Vector2(
                (0.5f - normalizedPivot.x) * rectTransform.sizeDelta.x,
                (0.5f - normalizedPivot.y) * rectTransform.sizeDelta.y
            );
        }
        else
        {
            _crosshairImage.color = targetColor; // Use color tint if no unique sprites assigned
            _spritePivotOffset = Vector2.zero;
        }
    }
}