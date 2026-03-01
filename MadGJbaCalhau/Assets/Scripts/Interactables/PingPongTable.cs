using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Single ping pong table. Reads the next difficulty from PlayerStats automatically.
/// After the player wins a match, HighestPingPongDifficulty increases and this table
/// advances to the next difficulty on return.
/// </summary>
public class PingPongTable : MonoBehaviour, IInteractable
{
    [Header("Scene")]
    [SerializeField] private string pingPongScene = "PongPing";
    [SerializeField] private string returnScene   = "GonScene";

    [Header("Prompt")]
    [SerializeField] private GameObject      interactPrompt;
    [SerializeField] private TextMeshProUGUI promptText;

    // Work points required per difficulty (index 1-5)
    private static readonly int[]    WorkPointsRequired = { 0, 100, 200, 300, 400, 500 };
    private static readonly string[] DiffNames          = { "", "Easy", "Medium", "Hard", "Expert", "Master" };
    private const int MaxDifficulty = 5;

    private bool _playerNearby;

    private int  NextDifficulty => Mathf.Clamp(PlayerStats.Instance.HighestPingPongDifficulty + 1, 1, MaxDifficulty);
    private bool AllBeaten      => PlayerStats.Instance.HighestPingPongDifficulty >= MaxDifficulty;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (promptText != null)     promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_playerNearby) UpdatePromptText();
    }

    private void OnDisable()
    {
        _playerNearby = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (promptText != null)     promptText.gameObject.SetActive(false);
    }

    // ── IInteractable ──────────────────────────────────────────────────────

    public void OnPlayerEnter()
    {
        _playerNearby = true;
        if (interactPrompt != null) interactPrompt.SetActive(true);
        if (promptText != null)     promptText.gameObject.SetActive(true);
        UpdatePromptText();
    }

    public void OnPlayerExit()
    {
        _playerNearby = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (promptText != null)     promptText.gameObject.SetActive(false);
    }

    // ── Self-detection (works even without Interactable layer) ────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) OnPlayerEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) OnPlayerExit();
    }

    public void Interact()
    {
        if (AllBeaten)
        {
            Debug.Log("[PingPongTable] All difficulties beaten!");
            return;
        }

        int next     = NextDifficulty;
        int required = next < WorkPointsRequired.Length ? WorkPointsRequired[next] : 999;

        if (PlayerStats.Instance.WorkPoints < required)
        {
            Debug.Log("[PingPongTable] LOCKED — need " + required + " pts, have " + PlayerStats.Instance.WorkPoints);
            return;
        }

        Debug.Log("[PingPongTable] Loading " + pingPongScene + " at difficulty " + next);

        BouncingBall2D.nivelTorneioAtual    = next;
        PingPongReturnData.returnScene      = returnScene;
        PingPongReturnData.playedDifficulty = next;

        // Save player position for return
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            PingPongReturnData.hasReturnPosition = true;
            PingPongReturnData.returnPositionX   = playerGO.transform.position.x;
            PingPongReturnData.returnPositionY   = playerGO.transform.position.y;
        }

        SceneManager.LoadScene(pingPongScene);
    }

    // ── Text ───────────────────────────────────────────────────────────────

    private void UpdatePromptText()
    {
        if (promptText == null) return;

        if (AllBeaten)
        {
            promptText.text = "All difficulties beaten!";
            return;
        }

        int    next      = NextDifficulty;
        int    required  = next < WorkPointsRequired.Length ? WorkPointsRequired[next] : 999;
        string diffName  = next < DiffNames.Length ? DiffNames[next] : next.ToString();
        int    current   = PlayerStats.Instance.WorkPoints;

        if (current < required)
            promptText.text = diffName + ": Need " + required + " work pts\n(have " + current + ")";
        else
            promptText.text = "[E] Play — " + diffName + " (" + next + "/" + MaxDifficulty + ")";
    }
}
