using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// A ping pong table the player can interact with to start a ping pong mini-game.
/// Attach to the PingPongTable GameObject and set it to the Interactable layer.
/// </summary>
public class PingPongTable : MonoBehaviour, IInteractable
{
    [Header("Difficulty")]
    [Tooltip("1 = Easy, 2 = Medium, 3 = Hard (maps directly to AIPaddle aiDifficulty)")]
    [SerializeField] private int difficulty = 1;
    [Tooltip("Work points needed to unlock this table.")]
    [SerializeField] private int requiredWorkPoints = 100;
    [Tooltip("Previous difficulty that must be beaten first (0 = none required).")]
    [SerializeField] private int requiredPreviousDifficulty = 0;

    // Called by Unity when component is first added — sets safe defaults
    private void Reset()
    {
        requiredWorkPoints = 100;
    }

    private bool IsUnlocked()
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats.WorkPoints < requiredWorkPoints) return false;
        if (requiredPreviousDifficulty > 0 && stats.HighestPingPongDifficulty < requiredPreviousDifficulty) return false;
        return true;
    }

    [Header("Scene")]
    [Tooltip("Name of the ping pong scene to load.")]
    [SerializeField] private string pingPongScene = "PongPing";
    [Tooltip("Name of the scene to return to after the match.")]
    [SerializeField] private string returnScene = "GonScene";

    [Header("Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI promptText;

    private bool _playerNearby;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    private void Awake()
    {
        // interactPrompt (the "press E" hint) starts hidden
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // clear text on start — the nearby table will write it when player arrives
        if (promptText != null)
            promptText.text = "";
    }

    private void Update()
    {
        // Only update text when this is the active table the player is near
        if (_playerNearby)
            UpdatePromptText();
    }

    private void OnDisable()
    {
        _playerNearby = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (promptText != null) promptText.text = "";
    }

    // ── IInteractable ──────────────────────────────────────────────────────

    public void OnPlayerEnter()
    {
        _playerNearby = true;
        if (interactPrompt != null) interactPrompt.SetActive(true);
        UpdatePromptText();
    }

    public void OnPlayerExit()
    {
        _playerNearby = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (promptText != null) promptText.text = "";
    }

    // ── Self-detection fallback (works even if not on Interactable layer) ──

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            OnPlayerEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            OnPlayerExit();
    }

    public void Interact()
    {
        Debug.Log("[PingPongTable] Interact called. WorkPoints=" + PlayerStats.Instance.WorkPoints + " Required=" + requiredWorkPoints + " Unlocked=" + IsUnlocked());

        if (!IsUnlocked())
        {
            Debug.Log("[PingPongTable] LOCKED — not loading scene.");
            UpdatePromptText();
            return;
        }

        Debug.Log("[PingPongTable] UNLOCKED — loading " + pingPongScene);
        BouncingBall2D.nivelTorneioAtual     = difficulty;
        PingPongReturnData.returnScene       = returnScene;
        PingPongReturnData.playedDifficulty  = difficulty;

        // Save player position so we can restore it on return
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            PingPongReturnData.hasReturnPosition = true;
            PingPongReturnData.returnPositionX   = playerGO.transform.position.x;
            PingPongReturnData.returnPositionY   = playerGO.transform.position.y;
        }

        SceneManager.LoadScene(pingPongScene);
    }

    private void UpdatePromptText()
    {
        if (promptText == null) return;

        PlayerStats stats = PlayerStats.Instance;
        string[] names = { "", "Easy", "Medium", "Hard", "Expert", "Master" };

        if (stats.WorkPoints < requiredWorkPoints)
        {
            promptText.text = "Need " + requiredWorkPoints + " work points\n(have " + stats.WorkPoints + ")";
            return;
        }
        if (requiredPreviousDifficulty > 0 && stats.HighestPingPongDifficulty < requiredPreviousDifficulty)
        {
            string prev = requiredPreviousDifficulty < names.Length ? names[requiredPreviousDifficulty] : requiredPreviousDifficulty.ToString();
            promptText.text = "Beat " + prev + " first";
            return;
        }

        string diffName = difficulty < names.Length ? names[difficulty] : difficulty.ToString();
        promptText.text = "[E] Play — " + diffName;
    }
}
