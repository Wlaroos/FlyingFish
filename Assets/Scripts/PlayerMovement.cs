using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Animator _anim;
    private SpriteRenderer _sr;
    private Vector2 _currentMovementInput;
    private bool _canMove = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.freezeRotation = true;

        _sr = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        Move();
        UpdateAnimation();
    }

    private void GetInput()
    {
        _currentMovementInput = _canMove ? PlayerInputs.MovementValue : Vector2.zero;
    }

    private void Move()
    {
        _rb.linearVelocity = _currentMovementInput * _moveSpeed;
        FlipSpriteBasedOnMovement();
    }

    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;
        if (!_canMove)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdateAnimation()
    {
        // Update movement animation
        _anim.SetBool("isMoving", _currentMovementInput != Vector2.zero);
    }

    private void FlipSpriteBasedOnMovement()
    {
        if (_currentMovementInput.x > 0)
            _sr.gameObject.transform.localScale = new Vector3(-1, 1, 1);
        else if (_currentMovementInput.x < 0)
            _sr.gameObject.transform.localScale = new Vector3(1, 1, 1);
    }
}