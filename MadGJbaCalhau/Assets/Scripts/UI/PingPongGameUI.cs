using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// A simple single-player ping pong mini-game.
/// Ball bounces left/right between player paddle and a wall.
/// Player controls the paddle with W/S (or Up/Down).
/// </summary>
public class PingPongGameUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button     closeButton;

    [Header("Game Objects")]
    [SerializeField] private RectTransform ball;
    [SerializeField] private RectTransform playerPaddle;
    [SerializeField] private RectTransform wallRight; // right wall / CPU side

    [Header("Settings")]
    [SerializeField] private float paddleSpeed  = 300f;
    [SerializeField] private float ballSpeed    = 250f;
    [SerializeField] private float paddleHalfH  = 60f;  // half paddle height in pixels
    [SerializeField] private float ballRadius   = 12f;

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    // ── Private state ──────────────────────────────────────────────────────

    private bool    _playing;
    private Vector2 _ballVelocity;
    private int     _score;

    // Bounds (set from panel rect)
    private float _minY, _maxY, _minX, _maxX;

    // Input
    private InputSystem_Actions _input;
    private float _paddleInput;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    private void Awake()
    {
        _input = new InputSystem_Actions();

        if (closeButton != null)
            closeButton.onClick.AddListener(StopGame);

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled  += OnMove;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled  -= OnMove;
        _input.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _paddleInput = ctx.ReadValue<Vector2>().y;
    }

    private void Update()
    {
        if (!_playing) return;

        MovePaddle();
        MoveBall();
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Opens the ping pong UI and starts the game.</summary>
    public void StartGame()
    {
        if (panel != null)
            panel.SetActive(true);

        _score = 0;
        UpdateScore();
        ResetBall();
        _playing = true;

        Time.timeScale = 0f; // use unscaledDeltaTime while paused
    }

    /// <summary>Closes the UI and returns to normal play.</summary>
    public void StopGame()
    {
        _playing = false;

        if (panel != null)
            panel.SetActive(false);

        Time.timeScale = 1f;
    }

    // ── Private game logic ─────────────────────────────────────────────────

    private void ResetBall()
    {
        if (ball == null) return;

        ball.anchoredPosition = Vector2.zero;

        // random angle, always going left toward player first
        float angle  = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        _ballVelocity = new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle)) * ballSpeed;

        // Cache bounds from the panel's rect
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            float hw = panelRect.rect.width  * 0.5f;
            float hh = panelRect.rect.height * 0.5f;
            _minX = -hw + ballRadius;
            _maxX =  hw - ballRadius;
            _minY = -hh + ballRadius;
            _maxY =  hh - ballRadius;
        }
    }

    private void MovePaddle()
    {
        if (playerPaddle == null) return;

        Vector2 pos = playerPaddle.anchoredPosition;
        pos.y += _paddleInput * paddleSpeed * Time.unscaledDeltaTime;
        pos.y  = Mathf.Clamp(pos.y, _minY + paddleHalfH, _maxY - paddleHalfH);
        playerPaddle.anchoredPosition = pos;
    }

    private void MoveBall()
    {
        if (ball == null) return;

        Vector2 pos = ball.anchoredPosition;
        pos += _ballVelocity * Time.unscaledDeltaTime;

        // Top / bottom bounce
        if (pos.y >= _maxY || pos.y <= _minY)
        {
            _ballVelocity.y = -_ballVelocity.y;
            pos.y = Mathf.Clamp(pos.y, _minY, _maxY);
        }

        // Right wall bounce (point scored)
        if (pos.x >= _maxX)
        {
            _ballVelocity.x = -_ballVelocity.x;
            pos.x = _maxX;
            _score++;
            UpdateScore();
        }

        // Left — check paddle hit
        if (pos.x <= _minX)
        {
            // Check if paddle overlaps ball vertically
            float paddleY = playerPaddle != null ? playerPaddle.anchoredPosition.y : 0f;
            if (Mathf.Abs(pos.y - paddleY) <= paddleHalfH + ballRadius)
            {
                _ballVelocity.x = Mathf.Abs(_ballVelocity.x); // bounce right
                pos.x = _minX;
            }
            else
            {
                // Missed — reset
                Debug.Log("[PingPong] Missed! Final score: " + _score);
                ResetBall();
            }
        }

        ball.anchoredPosition = pos;
    }

    private void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + _score;
    }
}

