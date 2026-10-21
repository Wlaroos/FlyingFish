using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _currentMovementInput;
    private bool _canMove = true;

    public WaterType? CurrentWaterZone { get; private set; } = null;
    public FishingHotspot CurrentHotspot { get; private set; } = null;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GetInput()
    {
        _currentMovementInput = _canMove ? PlayerInputs.MovementValue : Vector2.zero;
    }

    private void Move()
    {
        _rb.linearVelocity = _currentMovementInput * _moveSpeed;
    }

    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;
        if (!_canMove)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out WaterZone waterZone))
        {
            CurrentWaterZone = waterZone.WaterType;
            Debug.Log($"Entered {CurrentWaterZone} water. Press 'E' to fish");
        }

        if (collision.TryGetComponent(out FishingHotspot hotspot))
        {
            CurrentHotspot = hotspot;
            Debug.Log("Entered a fishing hotspot");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out WaterZone waterZone))
        {
            if (CurrentWaterZone == waterZone.WaterType)
            {
                CurrentWaterZone = null;
                CurrentHotspot = null;
                Debug.Log("Left water zone.");
            }
        }

        if (collision.TryGetComponent(out FishingHotspot hotspot))
        {
            if (CurrentHotspot == hotspot)
            {
                CurrentHotspot = null;
                Debug.Log("Left fishing hotspot.");
            }
        }
    }
}