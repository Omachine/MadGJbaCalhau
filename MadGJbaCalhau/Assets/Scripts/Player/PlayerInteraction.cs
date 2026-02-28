using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles detecting nearby IInteractable objects and triggering them.
/// Attach this to the Player GameObject alongside Player.cs.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float     interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Prompt UI (optional)")]
    [SerializeField] private GameObject interactPrompt;

    private IInteractable       _currentInteractable;
    private InputSystem_Actions _input;

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Interact.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        _input.Player.Interact.performed -= OnInteractPerformed;
        _input.Player.Disable();
    }

    private void Update()
    {
        DetectInteractable();
    }

    // ── Input callback ─────────────────────────────────────────────────────

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        _currentInteractable?.Interact();
    }

    // ── Detection ──────────────────────────────────────────────────────────

    private void DetectInteractable()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactionRadius, interactableLayer);

        if (hit != null && hit.TryGetComponent(out IInteractable interactable))
        {
            if (_currentInteractable != interactable)
            {
                _currentInteractable?.OnPlayerExit();
                _currentInteractable = interactable;
                _currentInteractable.OnPlayerEnter();
                ShowPrompt(true);
            }
        }
        else
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnPlayerExit();
                _currentInteractable = null;
                ShowPrompt(false);
            }
        }
    }

    private void ShowPrompt(bool show)
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(show);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
#endif
}
