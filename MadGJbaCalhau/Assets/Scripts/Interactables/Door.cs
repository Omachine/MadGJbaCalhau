using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A door that teleports the player to a linked destination door in the same scene.
/// Attach a full-screen black Image (alpha 0) as the transitionPanel — the door
/// activates it, waits, teleports, then deactivates it (same pattern as Bed/SleepRoutine).
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [Tooltip("The door the player will be teleported to.")]
    [SerializeField] private Door destinationDoor;

    [Tooltip("Spawn offset from the destination door (e.g. slightly in front of it).")]
    [SerializeField] private Vector2 spawnOffset = new Vector2(1f, 0f);

    [Header("Transition")]
    [Tooltip("Full-screen black panel UI Image — same one used for sleep. Will be activated/deactivated.")]
    [SerializeField] private GameObject transitionPanel;
    [Tooltip("Seconds to fade TO black.")]
    [SerializeField] private float fadeInDuration  = 0.3f;
    [Tooltip("Seconds to fade FROM black back to clear.")]
    [SerializeField] private float fadeOutDuration = 1.5f;

    [Header("Prompt")]
    [SerializeField] private GameObject interactPrompt;

    // Brief cooldown so the player doesn't immediately re-trigger the destination door
    private static float _teleportCooldown;
    private const  float CooldownDuration = 1.2f;

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
        if (Time.time < _teleportCooldown) return;

        if (destinationDoor == null)
        {
            Debug.LogWarning($"[Door] No destination door assigned on {gameObject.name}.");
            return;
        }

        Transform playerTransform = FindPlayerTransform();
        if (playerTransform == null) return;

        Player player = playerTransform.GetComponent<Player>();
        if (player == null) return;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Run on the player so the coroutine is never interrupted by door triggers
        player.RunCoroutine(TransitionRoutine(playerTransform));
    }

    // ── Private ────────────────────────────────────────────────────────────

    private IEnumerator TransitionRoutine(Transform playerTransform)
    {
        Player player = playerTransform.GetComponent<Player>();
        if (player != null) player.SetInputEnabled(false);

        Image panelImage = transitionPanel != null ? transitionPanel.GetComponent<Image>() : null;

        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
            if (panelImage != null) SetAlpha(panelImage, 0f);
        }

        // Fade TO black
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            if (panelImage != null)
                SetAlpha(panelImage, Mathf.Lerp(0f, 1f, elapsed / fadeInDuration));
        }
        if (panelImage != null) SetAlpha(panelImage, 1f);

        // Teleport while fully black
        TeleportPlayer(playerTransform);
        CameraFollow.Instance?.SnapToTarget();
        yield return null;

        // Fade FROM black back to clear
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            if (panelImage != null)
                SetAlpha(panelImage, Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration));
        }
        if (panelImage != null) SetAlpha(panelImage, 0f);

        if (transitionPanel != null) transitionPanel.SetActive(false);
        if (player != null) player.SetInputEnabled(true);
    }

    private static void SetAlpha(Image image, float a)
    {
        Color c = image.color;
        c.a = a;
        image.color = c;
    }

    private void TeleportPlayer(Transform playerTransform)
    {
        if (playerTransform == null || destinationDoor == null) return;

        Vector3 destination = destinationDoor.transform.position
                              + new Vector3(spawnOffset.x, spawnOffset.y, 0f);

        playerTransform.position = destination;

        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        _teleportCooldown = Time.time + CooldownDuration;
    }

    private static Transform FindPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[Door] No GameObject with tag 'Player' found in scene.");
            return null;
        }
        return player.transform;
    }
}
