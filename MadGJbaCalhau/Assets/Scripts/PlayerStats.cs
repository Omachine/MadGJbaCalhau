using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private static PlayerStats _instance;
    public static PlayerStats Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PlayerStats");
                _instance = go.AddComponent<PlayerStats>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public int WorkPoints { get; private set; }

    // 0 = none beaten, 1 = easy beaten, 2 = medium beaten, 3 = hard beaten
    public int HighestPingPongDifficulty { get; private set; }

    public event System.Action<int> OnWorkPointsChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddWorkPoints(int amount)
    {
        WorkPoints += amount;
        OnWorkPointsChanged?.Invoke(WorkPoints);
        Debug.Log("[PlayerStats] Work points: " + WorkPoints + " (+" + amount + ")");
    }

    public void SetPingPongDifficultyBeaten(int difficulty)
    {
        if (difficulty > HighestPingPongDifficulty)
        {
            HighestPingPongDifficulty = difficulty;
            Debug.Log("[PlayerStats] Ping pong difficulty beaten: " + difficulty);
        }
    }
}



