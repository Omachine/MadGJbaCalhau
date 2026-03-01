using UnityEngine;

/// <summary>
/// Bed interactable: press E to sleep, which skips 3 in-game hours
/// and resets the tiredness meter via DayManager.
/// </summary>
public class Bed : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [SerializeField] private GameObject interactPrompt; // kept for backwards compat

    public void OnPlayerEnter() { }
    public void OnPlayerExit()  { }

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

