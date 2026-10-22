using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _currentMovementInput;
    private bool _canMove = true;

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
}