using UnityEngine;

/// <summary>
/// A work table the player can interact with to open a work UI panel.
/// Attach to the WorkTable GameObject and set it to the Interactable layer.
/// </summary>
public class WorkTable : MonoBehaviour, IInteractable
{
    [Header("Work Table Settings")]
    [SerializeField] private WorkTableUI workTableUI; // drag the WorkTableUI component here

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
        if (workTableUI != null)
            workTableUI.Open();
        else
            Debug.LogWarning($"[WorkTable] No WorkTableUI assigned on {gameObject.name}.");
    }
}

