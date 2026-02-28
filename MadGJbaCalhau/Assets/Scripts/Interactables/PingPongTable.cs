using UnityEngine;

/// <summary>
/// A ping pong table the player can interact with to start a ping pong mini-game.
/// Attach to the PingPongTable GameObject and set it to the Interactable layer.
/// </summary>
public class PingPongTable : MonoBehaviour, IInteractable
{
    [Header("Ping Pong Settings")]
    [SerializeField] private PingPongGameUI pingPongUI; // drag the PingPongGameUI component here

    [Header("Prompt")]
    [SerializeField] private GameObject interactPrompt;

    // ── IInteractable ──────────────────────────────────────────────────────

    public void OnPlayerEnter()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    public void OnPlayerExit()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    public void Interact()
    {
        if (pingPongUI != null)
            pingPongUI.StartGame();
        else
            Debug.LogWarning($"[PingPongTable] No PingPongGameUI assigned on {gameObject.name}.");
    }
}

