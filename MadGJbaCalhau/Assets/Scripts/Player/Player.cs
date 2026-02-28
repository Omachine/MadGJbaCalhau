using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    // Components
    private Rigidbody2D _rb;
    private Animator    _animator;

    // Input
    private InputSystem_Actions _input;
    private float _horizontalInput;
    private bool  _facingRight = true;

    // Animator parameter hash
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        _rb    = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _input = new InputSystem_Actions();
    }

    private void OnEnable()  => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    private void Update()
    {
        _horizontalInput = _input.Player.Move.ReadValue<Vector2>().x;
        HandleFlip();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_horizontalInput * moveSpeed, _rb.linearVelocity.y);
    }

    // ── Flip ───────────────────────────────────────────────────────────────

    private void HandleFlip()
    {
        if (_facingRight  && _horizontalInput < 0f) Flip();
        else if (!_facingRight && _horizontalInput > 0f) Flip();
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    // ── Animator ───────────────────────────────────────────────────────────

    private void UpdateAnimator()
    {
        _animator.SetFloat(SpeedHash, Mathf.Abs(_horizontalInput));
    }
}
