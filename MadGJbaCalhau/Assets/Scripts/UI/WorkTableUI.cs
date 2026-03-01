using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hosts the Work Minigame panel.
/// Open() is called by WorkTable when the player interacts with the computer.
/// </summary>
public class WorkTableUI : MonoBehaviour
{
    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Minigame")]
    [SerializeField] private WorkMinigame workMinigame;

    private Player            _player;
    private PlayerInteraction _playerInteraction;

    // panel is always THIS GameObject — never use the inspector field
    private GameObject Panel => gameObject;

    private void Start()
    {
        // Use Start instead of Awake so it runs even if object starts inactive
        // (Start runs on first frame the object becomes active)
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        Panel.SetActive(false);
    }

    public void Open()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            _player            = playerGO.GetComponent<Player>();
            _playerInteraction = playerGO.GetComponent<PlayerInteraction>();
        }
        if (_player != null)            _player.SetInputEnabled(false);
        if (_playerInteraction != null) _playerInteraction.SetInteractionEnabled(false);

        Panel.SetActive(true);
        if (workMinigame != null) workMinigame.StartMinigame();
    }

    public void Close()
    {
        if (workMinigame != null) workMinigame.StopMinigame();
        Panel.SetActive(false);
        if (_player != null)            _player.SetInputEnabled(true);
        if (_playerInteraction != null) _playerInteraction.SetInteractionEnabled(true);
    }
}
