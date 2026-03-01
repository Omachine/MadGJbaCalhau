using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class WorkMinigame : MonoBehaviour
{
    private static readonly string[] WordBank =
    {
        "unity", "godot", "shader", "prefab", "commit", "build",
        "debug", "scene", "script", "pixel", "sprite", "asset",
        "branch", "merge", "deploy", "render", "physics", "collider",
        "raycast", "coroutine", "jam", "prototype", "playtest",
        "feedback", "iterate", "polish", "crunch", "scope", "design",
        "mechanic", "feature", "deadline", "submit", "export", "patch",
        "hotfix", "version", "engine", "texture", "animation", "audio",
        "tilemap", "canvas", "button", "toggle", "slider", "panel"
    };

    [Header("Word Slots (drag 4 TMP texts in order)")]
    [SerializeField] private TextMeshProUGUI[] wordSlots;

    [Header("Colours")]
    [SerializeField] private Color greyedColor  = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color activeColor  = new Color(1f,    1f,    1f,    1f);
    [SerializeField] private Color mistypeColor = new Color(0.9f,  0.2f,  0.2f,  1f);
    [SerializeField] private Color doneColor    = new Color(0.2f,  0.7f,  0.3f,  1f);

    [Header("Points")]
    [SerializeField] private int pointsPerWord = 10;

    [Header("Cooldown")]
    [SerializeField] private float mistypeCooldown = 2f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI inputIndicatorText;
    [SerializeField] private GameObject      cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private readonly List<string> _activeWords = new List<string>();
    private int  _currentWordIndex;
    private int  _typedCount;
    private bool _onCooldown;

    private PlayerStats _playerStats; // cached reference — never auto-creates

    private void OnPointsChanged(int pts)
    {
        if (scoreText != null)
            scoreText.text = "Work Points: " + pts;
    }

    public void StartMinigame()
    {
        _onCooldown = false;
        if (cooldownOverlay != null) cooldownOverlay.SetActive(false);

        _playerStats = PlayerStats.Instance; // auto-creates once here, safe
        if (_playerStats != null)
            _playerStats.OnWorkPointsChanged += OnPointsChanged;

        BuildWordList();
        RefreshSlots();
        UpdateScoreText();
        UpdateInputIndicator();
        Debug.Log("[WorkMinigame] Started. WordSlots: " + (wordSlots != null ? wordSlots.Length : 0) + " | Words loaded: " + _activeWords.Count);
        StartCoroutine(SubscribeNextFrame());
    }

    public void StopMinigame()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleChar;
        if (_playerStats != null)
            _playerStats.OnWorkPointsChanged -= OnPointsChanged;
        StopAllCoroutines();
        // Do NOT call gameObject.SetActive(false) here — WorkTableUI controls panel visibility
    }

    private IEnumerator SubscribeNextFrame()
    {
        yield return null;
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += HandleChar;
    }

    private void BuildWordList()
    {
        _activeWords.Clear();
        List<string> pool = new List<string>(WordBank);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string t = pool[i]; pool[i] = pool[j]; pool[j] = t;
        }
        int count = Mathf.Min(wordSlots != null ? wordSlots.Length : 4, pool.Count);
        for (int i = 0; i < count; i++)
            _activeWords.Add(pool[i]);
        _currentWordIndex = 0;
        _typedCount = 0;
    }

    private void RefreshSlots()
    {
        if (wordSlots == null) return;
        for (int w = 0; w < wordSlots.Length; w++)
        {
            if (wordSlots[w] == null) continue;
            if (w < _activeWords.Count)
            {
                wordSlots[w].gameObject.SetActive(true);
                wordSlots[w].text  = _activeWords[w].ToUpper();
                wordSlots[w].color = (w == _currentWordIndex) ? activeColor : greyedColor;
            }
            else
            {
                wordSlots[w].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateActiveSlot()
    {
        if (wordSlots == null || _currentWordIndex >= wordSlots.Length) return;
        if (_currentWordIndex >= _activeWords.Count) return;
        TextMeshProUGUI slot = wordSlots[_currentWordIndex];
        if (slot == null) return;
        string word = _activeWords[_currentWordIndex].ToUpper();
        string done = word.Substring(0, _typedCount);
        string rest = word.Substring(_typedCount);
        slot.text = "<color=#33DD66>" + done + "</color><color=#FFFFFF>" + rest + "</color>";
    }

    private void HandleChar(char c)
    {
        if (!char.IsLetter(c)) return;
        if (_onCooldown) return;
        if (_currentWordIndex >= _activeWords.Count) return;

        c = char.ToLower(c);
        char expected = _activeWords[_currentWordIndex][_typedCount];

        if (c == expected)
        {
            _typedCount++;
            UpdateActiveSlot();
            UpdateInputIndicator();
            if (_typedCount >= _activeWords[_currentWordIndex].Length)
                CompleteWord();
        }
        else
        {
            StartCoroutine(MistypeCooldown());
        }
    }

    private void CompleteWord()
    {
        Debug.Log("[WorkMinigame] Word completed: " + _activeWords[_currentWordIndex]);
        if (_playerStats != null)
            _playerStats.AddWorkPoints(pointsPerWord);
        else
            Debug.LogWarning("[WorkMinigame] PlayerStats is null!");
        UpdateScoreText();

        if (wordSlots != null && _currentWordIndex < wordSlots.Length && wordSlots[_currentWordIndex] != null)
        {
            wordSlots[_currentWordIndex].text  = _activeWords[_currentWordIndex].ToUpper();
            wordSlots[_currentWordIndex].color = doneColor;
        }

        _currentWordIndex++;
        _typedCount = 0;

        if (_currentWordIndex >= _activeWords.Count)
        {
            BuildWordList();
            RefreshSlots();
        }
        else
        {
            RefreshSlots();
        }
        UpdateInputIndicator();
    }

    private IEnumerator MistypeCooldown()
    {
        _onCooldown = true;
        _typedCount = 0;
        UpdateInputIndicator();

        if (wordSlots != null && _currentWordIndex < wordSlots.Length && wordSlots[_currentWordIndex] != null)
        {
            wordSlots[_currentWordIndex].text  = _activeWords[_currentWordIndex].ToUpper();
            wordSlots[_currentWordIndex].color = mistypeColor;
        }

        if (cooldownOverlay != null) cooldownOverlay.SetActive(true);

        float remaining = mistypeCooldown;
        while (remaining > 0f)
        {
            if (cooldownText != null)
                cooldownText.text = "Mistype! Wait " + remaining.ToString("F1") + "s";
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (cooldownOverlay != null) cooldownOverlay.SetActive(false);

        if (wordSlots != null && _currentWordIndex < wordSlots.Length && wordSlots[_currentWordIndex] != null)
        {
            wordSlots[_currentWordIndex].text  = _activeWords[_currentWordIndex].ToUpper();
            wordSlots[_currentWordIndex].color = activeColor;
        }

        _onCooldown = false;
        UpdateInputIndicator();
    }

    private void UpdateScoreText()
    {
        if (scoreText == null) return;
        int pts = _playerStats != null ? _playerStats.WorkPoints : 0;
        scoreText.text = "Work Points: " + pts;
    }

    private void UpdateInputIndicator()
    {
        if (inputIndicatorText == null) return;
        if (_currentWordIndex >= _activeWords.Count) { inputIndicatorText.text = ""; return; }
        string word  = _activeWords[_currentWordIndex];
        string typed = word.Substring(0, _typedCount).ToUpper();
        string rest  = word.Substring(_typedCount).ToUpper();
        inputIndicatorText.text = "<color=#33DD66>" + typed + "</color><color=#888888>" + rest + "</color>";
    }
}
