using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class MesaPingPongMapa : MonoBehaviour
{
    [Header("Configurações de Transição")]
    public string nomeCenaPingPong = "PongPing";
    public string returnScene = "GonScene";

    [Header("Interface Visual")]
    public GameObject avisoInteracaoUI;
    public TextMeshProUGUI avisoText;

    // Work points required per difficulty level (index = difficulty 1-5)
    private static readonly int[] WorkPointsRequired = { 0, 100, 200, 300, 400, 500 };
    private static readonly string[] DiffNames = { "", "Easy", "Medium", "Hard", "Expert", "Master" };
    private const int MaxDifficulty = 5;

    private bool _playerNearby;

    // The difficulty the player should play next (beaten + 1, clamped to MaxDifficulty)
    private int NextDifficulty => Mathf.Clamp(PlayerStats.Instance.HighestPingPongDifficulty + 1, 1, MaxDifficulty);
    private bool AllBeaten => PlayerStats.Instance.HighestPingPongDifficulty >= MaxDifficulty;

    private void Awake()
    {
        if (avisoInteracaoUI != null) avisoInteracaoUI.SetActive(false);
        if (avisoText != null) avisoText.gameObject.SetActive(true);
    }

    private void Start()
    {
        UpdateAvisoText();
    }

    private void Update()
    {
        if (_playerNearby)
        {
            UpdateAvisoText();
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                EntrarNoTorneio();
        }
    }

    private void OnDisable()
    {
        _playerNearby = false;
        if (avisoInteracaoUI != null) avisoInteracaoUI.SetActive(false);
    }

    private bool IsUnlocked()
    {
        if (AllBeaten) return false; // already beat everything
        int next = NextDifficulty;
        int required = next < WorkPointsRequired.Length ? WorkPointsRequired[next] : 999;
        return PlayerStats.Instance.WorkPoints >= required;
    }

    private void EntrarNoTorneio()
    {
        if (!IsUnlocked())
        {
            UpdateAvisoText();
            return;
        }

        int next = NextDifficulty;

        // Save player position for return
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            PingPongReturnData.hasReturnPosition = true;
            PingPongReturnData.returnPositionX   = playerGO.transform.position.x;
            PingPongReturnData.returnPositionY   = playerGO.transform.position.y;
        }

        BouncingBall2D.nivelTorneioAtual     = next;
        PingPongReturnData.returnScene       = returnScene;
        PingPongReturnData.playedDifficulty  = next;
        SceneManager.LoadScene(nomeCenaPingPong);
    }

    private void UpdateAvisoText()
    {
        if (avisoText == null) return;

        if (AllBeaten)
        {
            avisoText.text = "All difficulties beaten!";
            return;
        }

        int next = NextDifficulty;
        int required = next < WorkPointsRequired.Length ? WorkPointsRequired[next] : 999;
        string diffName = next < DiffNames.Length ? DiffNames[next] : next.ToString();
        int currentPoints = PlayerStats.Instance.WorkPoints;

        if (currentPoints < required)
            avisoText.text = diffName + ": Need " + required + " work pts (have " + currentPoints + ")";
        else
            avisoText.text = "[E] Play — " + diffName + " (" + (next) + "/" + MaxDifficulty + ")";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        _playerNearby = true;
        if (avisoInteracaoUI != null) avisoInteracaoUI.SetActive(true);
        UpdateAvisoText();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        _playerNearby = false;
        if (avisoInteracaoUI != null) avisoInteracaoUI.SetActive(false);
    }
}
