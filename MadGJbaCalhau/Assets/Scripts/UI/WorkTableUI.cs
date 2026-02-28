using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI panel for the Work Table mini-game / interface.
/// Expand this class with your actual work logic (crafting, quests, etc.).
/// </summary>
public class WorkTableUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;   // root panel to show/hide
    [SerializeField] private Button     closeButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // Start hidden
        if (panel != null)
            panel.SetActive(false);
    }

    /// <summary>Opens the work table UI and pauses the game.</summary>
    public void Open()
    {
        if (panel != null)
            panel.SetActive(true);

        Time.timeScale = 0f; // pause while in UI
    }

    /// <summary>Closes the work table UI and resumes the game.</summary>
    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);

        Time.timeScale = 1f;
    }
}

