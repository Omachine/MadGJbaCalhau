using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hosts the Work Minigame panel.
/// Open() is called by WorkTable when the player interacts with the computer.
/// </summary>
public class WorkTableUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button     closeButton;

    [Header("Minigame")]
    [SerializeField] private WorkMinigame workMinigame;

    private Player            _player;
    private PlayerInteraction _playerInteraction;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (panel != null)
            panel.SetActive(false);
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

        if (panel != null) panel.SetActive(true);
        if (workMinigame != null) workMinigame.StartMinigame();
    }

    public void Close()
    {
        if (workMinigame != null) workMinigame.StopMinigame();
        if (panel != null) panel.SetActive(false);
        if (_player != null)            _player.SetInputEnabled(true);
        if (_playerInteraction != null) _playerInteraction.SetInteractionEnabled(true);
    }
}
