using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float     interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private float      promptYOffset = 1.8f;
    [SerializeField] private float      promptXOffset = -0.5f;

    private IInteractable       _currentInteractable;
    private Transform           _currentInteractableTransform;
    private InputSystem_Actions _input;
    private bool                _interactionEnabled = true;

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }

    private void Start()
    {
        if (interactPrompt == null)
            interactPrompt = GameObject.Find("InteractPrompt");

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
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

        // Always move prompt to sit above the current interactable
        if (_currentInteractableTransform != null && interactPrompt != null && interactPrompt.activeSelf)
            interactPrompt.transform.position = new Vector3(
                _currentInteractableTransform.position.x + promptXOffset,
                _currentInteractableTransform.position.y + promptYOffset,
                _currentInteractableTransform.position.z);
    }

    public void SetInteractionEnabled(bool state)
    {
        _interactionEnabled = state;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!_interactionEnabled) return;
        _currentInteractable?.Interact();
    }

    private void DetectInteractable()
    {
        int mask = interactableLayer.value != 0 ? interactableLayer.value : ~0;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, mask);

        IInteractable found = null;
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.transform.IsChildOf(transform)) continue;

            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null) hit.TryGetComponent(out interactable);
            if (interactable != null) { found = interactable; break; }
        }

        if (found != null)
        {
            if (_currentInteractable != found)
            {
                _currentInteractable?.OnPlayerExit();
                _currentInteractable          = found;
                _currentInteractableTransform = (found as MonoBehaviour)?.transform;
                _currentInteractable.OnPlayerEnter();

                // Only show the world prompt for PingPongTable
                if (interactPrompt != null)
                    interactPrompt.SetActive(found is PingPongTable);
            }
        }
        else if (_currentInteractable != null)
        {
            _currentInteractable.OnPlayerExit();
            _currentInteractable          = null;
            _currentInteractableTransform = null;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
#endif
}
