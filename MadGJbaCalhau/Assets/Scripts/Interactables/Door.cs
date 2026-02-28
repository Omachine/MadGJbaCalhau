using UnityEngine;

/// <summary>
/// A door that teleports the player to a linked destination door in the same scene.
/// Attach to any door GameObject and set it to the Interactable layer.
/// Assign the linked destination door in the inspector.
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [Tooltip("The door the player will be teleported to.")]
    [SerializeField] private Door destinationDoor;

    [Tooltip("Spawn offset from the destination door (e.g. slightly in front of it).")]
    [SerializeField] private Vector2 spawnOffset = new Vector2(1f, 0f);

    [Header("Transition (optional)")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private float    delayBeforeTeleport = 0.3f;

    [Header("Prompt")]
    [SerializeField] private GameObject interactPrompt;

    // Brief cooldown so the player doesn't immediately re-trigger the destination door
    private static float _teleportCooldown;
    private const  float CooldownDuration = 1f;

    private Transform _playerTransform;

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
        // Block interaction during cooldown (e.g. just arrived from another door)
        if (Time.time < _teleportCooldown)
            return;

        if (destinationDoor == null)
        {
            Debug.LogWarning($"[Door] No destination door assigned on {gameObject.name}.");
            return;
        }

        // Cache the player before the delay
        _playerTransform = FindPlayerTransform();
        if (_playerTransform == null) return;

        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        if (destinationDoor.doorAnimator != null)
            destinationDoor.doorAnimator.SetTrigger("Open");

        if (delayBeforeTeleport > 0f)
            Invoke(nameof(TeleportPlayer), delayBeforeTeleport);
        else
            TeleportPlayer();
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void TeleportPlayer()
    {
        if (_playerTransform == null || destinationDoor == null) return;

        // Calculate world-space spawn offset, respecting door's orientation
        Vector3 destination = destinationDoor.transform.position
                              + destinationDoor.transform.TransformDirection(new Vector3(spawnOffset.x, spawnOffset.y, 0f));

        // Move the player — also zero out rigidbody velocity so it doesn't slide
        _playerTransform.position = destination;

        Rigidbody2D rb = _playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Set cooldown so the destination door doesn't trigger immediately
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
