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
    private bool  _facingRight   = true;
    private bool  _inputEnabled  = true;

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
        _horizontalInput = _inputEnabled ? _input.Player.Move.ReadValue<Vector2>().x : 0f;
        HandleFlip();
        UpdateAnimator();
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Enable or disable player input (used during door transitions, cutscenes, etc.).</summary>
    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        if (!enabled)
            _horizontalInput = 0f;
    }

    /// <summary>Run a coroutine on the player object so it survives scene/door transitions.</summary>
    public void RunCoroutine(System.Collections.IEnumerator routine) => StartCoroutine(routine);

    private void FixedUpdate()
    {
        if (!_inputEnabled)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }
        float tirednessMultiplier = DayManager.Instance != null ? DayManager.Instance.SpeedMultiplier : 1f;
        _rb.linearVelocity = new Vector2(_horizontalInput * moveSpeed * tirednessMultiplier, _rb.linearVelocity.y);

        ClampToBounds();
    }

    private void ClampToBounds()
    {
        if (CameraFollow.Instance == null) return;
        Vector2 pos = _rb.position;
        bool changed = false;

        if (CameraFollow.Instance.BoundsXActive)
        {
            float clamped = Mathf.Clamp(pos.x, CameraFollow.Instance.MinX, CameraFollow.Instance.MaxX);
            if (clamped != pos.x) { pos.x = clamped; changed = true; }
        }
        if (CameraFollow.Instance.BoundsYActive)
        {
            float clamped = Mathf.Clamp(pos.y, CameraFollow.Instance.MinY, CameraFollow.Instance.MaxY);
            if (clamped != pos.y) { pos.y = clamped; changed = true; }
        }

        if (changed)
        {
            _rb.position = pos;
            _rb.linearVelocity = new Vector2(
                CameraFollow.Instance.BoundsXActive && (pos.x <= CameraFollow.Instance.MinX || pos.x >= CameraFollow.Instance.MaxX) ? 0f : _rb.linearVelocity.x,
                _rb.linearVelocity.y
            );
        }
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
