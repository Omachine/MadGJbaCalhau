using UnityEngine;

/// <summary>
/// Bed interactable: press E to sleep, which skips 3 in-game hours
/// and resets the tiredness meter via DayManager.
/// </summary>
public class Bed : MonoBehaviour, IInteractable
{
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
        if (DayManager.Instance == null)
        {
            Debug.LogWarning("[Bed] No DayManager found in scene.");
            return;
        }

        if (!DayManager.Instance.ClockRunning)
            return; // already sleeping or weekend over

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        DayManager.Instance.Sleep();
        Debug.Log("[Bed] Player went to sleep.");
    }
}

