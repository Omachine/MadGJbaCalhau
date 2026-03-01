using UnityEngine;

/// <summary>
/// Follows the player with a velocity-based offset so the camera
/// lags slightly behind and looks ahead in the direction of movement.
/// Attach this to the Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static CameraFollow Instance { get; private set; }

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Lead")]
    [Tooltip("How far ahead of the player the camera tries to look (in world units).")]
    [SerializeField] private float leadAmount = 3f;
    [Tooltip("How quickly the lead offset builds up / fades out (higher = snappier).")]
    [SerializeField] private float leadSpeed = 4f;

    [Header("Follow")]
    [Tooltip("How smoothly the camera catches up to the target. Lower = lazier.")]
    [SerializeField] private float followSmoothing = 5f;

    [Header("Vertical Offset")]
    [Tooltip("How many world units above the player the camera is centred.")]
    [SerializeField] private float verticalOffset = 2f;

    [Header("Vertical Lock (optional)")]
    [Tooltip("Lock the Y axis so the camera only follows horizontally.")]
    [SerializeField] private bool  lockY;
    [SerializeField] private float lockedY;

    // ── Active bounds (set by CameraBounds trigger volumes) ────────────────
    private bool  _boundsXActive;
    private float _minX, _maxX;
    private bool  _boundsYActive;
    private float _minY, _maxY;

    // Public read-only access for Player clamping
    public bool  BoundsXActive => _boundsXActive;
    public bool  BoundsYActive => _boundsYActive;
    public float MinX => _minX;
    public float MaxX => _maxX;
    public float MinY => _minY;
    public float MaxY => _maxY;

    // Runtime
    private Vector3 _currentVelocity;
    private float   _currentLeadX;
    private Camera  _cam;

    private void Awake()
    {
        Instance = this;
        _cam = GetComponent<Camera>();
    }

    /// <summary>Called by Door after teleporting — instantly snaps the camera to the player's new position so it doesn't pan through empty space.</summary>
    public void SnapToTarget()
    {
        if (target == null) return;

        _currentLeadX    = 0f;
        _currentVelocity = Vector3.zero;

        float targetY = (lockY ? lockedY : target.position.y) + verticalOffset;
        Vector3 snappedPos = new Vector3(target.position.x, targetY, transform.position.z);

        float halfH = _cam != null ? _cam.orthographicSize   : 0f;
        float halfW = _cam != null ? halfH * _cam.aspect     : 0f;

        if (_boundsXActive)
            snappedPos.x = Mathf.Clamp(snappedPos.x, _minX + halfW, _maxX - halfW);
        if (_boundsYActive)
            snappedPos.y = Mathf.Clamp(snappedPos.y, _minY + halfH, _maxY - halfH);

        transform.position = snappedPos;
    }

    /// <summary>Called automatically by CameraBounds when the player enters a room.</summary>
    public void ApplyBounds(CameraBounds bounds)
    {
        _boundsXActive = bounds.clampX;
        _minX = bounds.minX;
        _maxX = bounds.maxX;

        _boundsYActive = bounds.clampY;
        _minY = bounds.minY;
        _maxY = bounds.maxY;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Work out the player's horizontal velocity to drive the lead
        float playerVelX = 0f;
        if (target.TryGetComponent(out Rigidbody2D rb))
            playerVelX = rb.linearVelocity.x;

        // Smoothly move the lead offset toward the desired direction
        float desiredLeadX = Mathf.Sign(playerVelX) * leadAmount * (Mathf.Abs(playerVelX) > 0.1f ? 1f : 0f);
        _currentLeadX = Mathf.Lerp(_currentLeadX, desiredLeadX, Time.deltaTime * leadSpeed);

        // Build the desired camera position — vertical offset always applies
        float targetY = (lockY ? lockedY : target.position.y) + verticalOffset;
        Vector3 desiredPos = new Vector3(
            target.position.x + _currentLeadX,
            targetY,
            transform.position.z
        );

        // Clamp to active room bounds
        float halfH = _cam != null ? _cam.orthographicSize      : 0f;
        float halfW = _cam != null ? halfH * _cam.aspect        : 0f;

        if (_boundsXActive)
            desiredPos.x = Mathf.Clamp(desiredPos.x, _minX + halfW, _maxX - halfW);
        if (_boundsYActive)
            desiredPos.y = Mathf.Clamp(desiredPos.y, _minY + halfH, _maxY - halfH);

        // Smoothly move the camera toward the desired position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _currentVelocity,
            1f / followSmoothing
        );
    }
}
