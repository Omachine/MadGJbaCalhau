using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class MesaPingPongMapa : MonoBehaviour
{
    [Header("Configurações de Transição")]
    public string nomeCenaPingPong = "PingPong";
    public string returnScene = "GoncaloScene";

    [Header("Difficulty Lock")]
    [Tooltip("1 = Easy, 2 = Medium, 3 = Hard")]
    public int difficulty = 1;
    public int requiredWorkPoints = 100;
    public int requiredPreviousDifficulty = 0;

    [Header("Interface Visual")]
    public GameObject avisoInteracaoUI;
    public TextMeshProUGUI avisoText;  // optional — shows lock reason

    private bool jogadorEstaPerto = false;

    void Start()
    {
        if (avisoInteracaoUI != null)
            avisoInteracaoUI.SetActive(false);
    }

    void Update()
    {
        if (jogadorEstaPerto && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            EntrarNoTorneio();
        }
    }

    private bool IsUnlocked()
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats.WorkPoints < requiredWorkPoints) return false;
        if (requiredPreviousDifficulty > 0 && stats.HighestPingPongDifficulty < requiredPreviousDifficulty) return false;
        return true;
    }

    private void EntrarNoTorneio()
    {
        if (!IsUnlocked())
        {
            UpdateAvisoText();
            Debug.Log("[MesaPingPongMapa] LOCKED — WorkPoints=" + PlayerStats.Instance.WorkPoints + " Required=" + requiredWorkPoints);
            return;
        }

        Debug.Log("A carregar a partida de Ping Pong...");
        BouncingBall2D.nivelTorneioAtual    = difficulty;
        PingPongReturnData.returnScene      = returnScene;
        PingPongReturnData.playedDifficulty = difficulty;
        SceneManager.LoadScene(nomeCenaPingPong);
    }

    private void UpdateAvisoText()
    {
        if (avisoText == null) return;
        PlayerStats stats = PlayerStats.Instance;
        string[] names = { "", "Easy", "Medium", "Hard", "Expert", "Master" };

        if (stats.WorkPoints < requiredWorkPoints)
        {
            avisoText.text = "Need " + requiredWorkPoints + " work points (have " + stats.WorkPoints + ")";
            return;
        }
        if (requiredPreviousDifficulty > 0 && stats.HighestPingPongDifficulty < requiredPreviousDifficulty)
        {
            string prev = requiredPreviousDifficulty < names.Length ? names[requiredPreviousDifficulty] : requiredPreviousDifficulty.ToString();
            avisoText.text = "Beat " + prev + " first";
            return;
        }
        string diffName = difficulty < names.Length ? names[difficulty] : difficulty.ToString();
        avisoText.text = "[E] Play — " + diffName;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorEstaPerto = true;
            if (avisoInteracaoUI != null)
                avisoInteracaoUI.SetActive(true);
            UpdateAvisoText();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorEstaPerto = false;
            if (avisoInteracaoUI != null)
                avisoInteracaoUI.SetActive(false);
        }
    }
}

