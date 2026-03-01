using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the weekend game clock: Friday 16:00 → Sunday 16:00 (48 real-time hours mapped to configurable seconds).
/// Also tracks player tiredness. Sleeping skips 3 in-game hours and resets tiredness.
/// </summary>
public class DayManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static DayManager Instance { get; private set; }

    // ── Time settings ──────────────────────────────────────────────────────
    [Header("Time Settings")]
    [Tooltip("How many real seconds = 1 in-game hour.")]
    [SerializeField] private float realSecondsPerGameHour = 60f;  // 1 min real = 1 game hour

    // Game clock starts Friday 16:00, ends Sunday 16:00 = 48 game-hours total
    private const int StartDayOfWeek  = 5;   // Friday   (0=Sun … 6=Sat, but we use 5=Fri)
    private const int StartHour       = 16;
    private const int TotalGameHours  = 48;  // Fri 16:00 → Sun 16:00

    // Internal: total elapsed game-hours (0 = Fri 16:00, 48 = Sun 16:00)
    private float _elapsedGameHours = 0f;
    private bool  _clockRunning     = false;
    private bool  _gameOver         = false;

    // ── Tiredness ──────────────────────────────────────────────────────────
    [Header("Tiredness")]
    [Tooltip("Tiredness increases by this amount per game-hour automatically.")]
    [SerializeField] private float tirednessPerHour = 0.2f;       // 0→1 scale — fully tired after 5 game-hours
    [Tooltip("Speed multiplier when fully tired (tiredness = 1).")]
    [SerializeField] private float minSpeedMultiplier = 0.4f;

    private float _tiredness = 0f;   // 0 = fresh, 1 = exhausted

    // ── UI ─────────────────────────────────────────────────────────────────
    [Header("UI – Time Bar")]
    [Tooltip("Image set to Filled / Horizontal. Fills as weekend progresses.")]
    [SerializeField] private Image timeProgressBar;
    [SerializeField] private TextMeshProUGUI clockLabel;      // e.g. "Friday 18:30"
    [SerializeField] private TextMeshProUGUI hoursLeftLabel;  // e.g. "34h left"

    [Header("UI – Tiredness Bar")]
    [Tooltip("Image set to Filled / Horizontal. Fills as player gets tired.")]
    [SerializeField] private Image tirednessBar;
    [SerializeField] private TextMeshProUGUI tirednessLabel;  // e.g. "Tired 40%"

    [Header("End-of-Weekend Panel (optional)")]
    [SerializeField] private GameObject endPanel;

    [Header("Sleep Transition Panel (optional)")]
    [SerializeField] private GameObject sleepPanel;
    [SerializeField] private float      sleepFadeDuration = 1.5f;

    // ── Events ─────────────────────────────────────────────────────────────
    public event System.Action<string, int> OnHourChanged;  // (dayName, hour)
    public event System.Action              OnWeekendOver;

    // ── Public accessors ───────────────────────────────────────────────────
    public float Tiredness          => _tiredness;
    /// <summary>Speed multiplier based on tiredness (1 = full speed, minSpeedMultiplier = exhausted).</summary>
    public float SpeedMultiplier    => Mathf.Lerp(1f, minSpeedMultiplier, _tiredness);
    public float ElapsedGameHours   => _elapsedGameHours;
    public float TotalHours         => TotalGameHours;
    public bool  ClockRunning       => _clockRunning;

    // ── Internal helpers ───────────────────────────────────────────────────
    private int _lastWholeHour = -1;

    private (string dayName, int hour, int minute) GetCurrentTime()
    {
        float totalHoursFromFri16 = StartHour + _elapsedGameHours;
        int dayOffset = Mathf.FloorToInt(totalHoursFromFri16 / 24f);
        int hour      = Mathf.FloorToInt(totalHoursFromFri16 % 24f);
        int minute    = Mathf.FloorToInt((totalHoursFromFri16 % 1f) * 60f);

        string dayName = dayOffset switch
        {
            0 => "Friday",
            1 => "Saturday",
            2 => "Sunday",
            _ => "Sunday"
        };
        return (dayName, hour, minute);
    }

    // ──────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find all UI references by GameObject name in the new scene
        ReconnectUI();

        // Restore player position if returning from ping pong
        if (PingPongReturnData.hasReturnPosition)
        {
            PingPongReturnData.hasReturnPosition = false;
            StartCoroutine(RestorePlayerPosition(
                PingPongReturnData.returnPositionX,
                PingPongReturnData.returnPositionY));
        }
    }

    private System.Collections.IEnumerator RestorePlayerPosition(float x, float y)
    {
        // Wait one frame for the scene to fully initialize
        yield return null;
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerGO.transform.position = new Vector3(x, y, playerGO.transform.position.z);
            // Zero out rigidbody velocity so they don't fly off
            Rigidbody2D rb = playerGO.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    private void ReconnectUI()
    {
        // Sleep panel
        GameObject sp = GameObject.FindWithTag("SleepPanel");
        if (sp != null) { sleepPanel = sp; sleepPanel.SetActive(false); }
        else if (sleepPanel != null) sleepPanel.SetActive(false);

        // Clock label
        var clockGO = GameObject.Find("ClockLabel");
        if (clockGO != null) clockLabel = clockGO.GetComponent<TextMeshProUGUI>();

        // Hours left label
        var hoursGO = GameObject.Find("HoursLeftLabel");
        if (hoursGO != null) hoursLeftLabel = hoursGO.GetComponent<TextMeshProUGUI>();

        // Tiredness label
        var tiredGO = GameObject.Find("TirednessLabel");
        if (tiredGO != null) tirednessLabel = tiredGO.GetComponent<TextMeshProUGUI>();

        // Time progress bar
        var barGO = GameObject.Find("DayBarFill");
        if (barGO != null) timeProgressBar = barGO.GetComponent<Image>();

        // Tiredness bar
        var tiredBarGO = GameObject.Find("TirednessBarFill");
        if (tiredBarGO != null) tirednessBar = tiredBarGO.GetComponent<Image>();

        // End panel
        var endGO = GameObject.Find("EndPanel");
        if (endGO != null) endPanel = endGO;

        // Refresh UI immediately with current values
        UpdateUI();
    }

    private void Start()
    {
        if (endPanel   != null) endPanel.SetActive(false);
        if (sleepPanel != null) sleepPanel.SetActive(false);

        _clockRunning = true;
        UpdateUI();
    }

    private void Update()
    {
        if (!_clockRunning || _gameOver) return;

        float hoursThisFrame = (Time.deltaTime / realSecondsPerGameHour);
        _elapsedGameHours += hoursThisFrame;

        // Tiredness accumulates over time
        _tiredness = Mathf.Clamp01(_tiredness + tirednessPerHour * hoursThisFrame);

        // Fire event on each new whole hour
        int wholeHour = Mathf.FloorToInt(_elapsedGameHours + StartHour) % 24;
        if (wholeHour != _lastWholeHour)
        {
            _lastWholeHour = wholeHour;
            var (dayName, hour, _) = GetCurrentTime();
            OnHourChanged?.Invoke(dayName, hour);
        }

        UpdateUI();

        if (_elapsedGameHours >= TotalGameHours)
            EndWeekend();
    }

    // ── Sleep ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by the Bed interactable. Skips 3 game-hours and resets tiredness.
    /// </summary>
    public void Sleep()
    {
        if (!_clockRunning || _gameOver) return;
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        _clockRunning = false;

        if (sleepPanel != null) sleepPanel.SetActive(true);
        yield return new WaitForSeconds(sleepFadeDuration);

        _elapsedGameHours = Mathf.Min(_elapsedGameHours + 3f, TotalGameHours);
        _tiredness = 0f;

        UpdateUI();

        if (sleepPanel != null) sleepPanel.SetActive(false);

        if (_elapsedGameHours >= TotalGameHours)
            EndWeekend();
        else
            _clockRunning = true;

        Debug.Log("[DayManager] Slept 3 hours. Tiredness reset.");
    }

    // ── End ────────────────────────────────────────────────────────────────

    private void EndWeekend()
    {
        _gameOver     = true;
        _clockRunning = false;
        _elapsedGameHours = TotalGameHours;
        UpdateUI();

        if (endPanel != null) endPanel.SetActive(true);
        OnWeekendOver?.Invoke();
        Debug.Log("[DayManager] Weekend over!");
    }

    // ── UI ─────────────────────────────────────────────────────────────────

    private void UpdateUI()
    {
        var (dayName, hour, minute) = GetCurrentTime();

        if (timeProgressBar != null)
            timeProgressBar.fillAmount = Mathf.Clamp01(_elapsedGameHours / TotalGameHours);

        if (clockLabel != null)
            clockLabel.text = $"{dayName}  {hour:D2}:{minute:D2}";

        float hoursLeft = Mathf.Max(0f, TotalGameHours - _elapsedGameHours);
        if (hoursLeftLabel != null)
            hoursLeftLabel.text = $"{hoursLeft:F0}h left";

        if (tirednessBar != null)
            tirednessBar.fillAmount = _tiredness;

        if (tirednessLabel != null)
            tirednessLabel.text = $"Tired {_tiredness * 100f:F0}%";
    }

    // ── Public utilities ───────────────────────────────────────────────────

    /// <summary>Pause/resume the clock (e.g. while in a UI menu).</summary>
    public void SetClockPaused(bool paused) => _clockRunning = !paused;

    /// <summary>Add tiredness manually (e.g. from activities).</summary>
    public void AddTiredness(float amount) => _tiredness = Mathf.Clamp01(_tiredness + amount);
}
