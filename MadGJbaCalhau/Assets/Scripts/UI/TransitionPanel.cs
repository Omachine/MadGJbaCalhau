using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton that manages a full-screen black panel used for door transitions.
/// Fades to black → calls a callback (teleport) → fades back to clear.
///
/// Setup:
///   1. Create a Canvas (Screen Space - Overlay, Sort Order high like 99).
///   2. Add a child Image that fills the canvas – set its Color to black (alpha 0).
///   3. Attach this script to that Image GameObject.
/// </summary>
public class TransitionPanel : MonoBehaviour
{
    public static TransitionPanel Instance { get; private set; }

    [Tooltip("Seconds to fade to black.")]
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Tooltip("Seconds to fade back to clear after teleport.")]
    [SerializeField] private float fadeInDuration  = 0.4f;

    private Image _panel;
    private bool  _busy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject); // keep across scenes if needed

        _panel = GetComponent<Image>();
        if (_panel == null)
            Debug.LogError("[TransitionPanel] No Image component found! Attach this script to an Image.");

        // Start fully transparent
        SetAlpha(0f);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Fade to black, invoke <paramref name="onBlack"/>, then fade back to clear.
    /// Safe to call from any MonoBehaviour.
    /// </summary>
    public void DoTransition(Action onBlack)
    {
        if (_busy) return;
        StartCoroutine(TransitionRoutine(onBlack));
    }

    private IEnumerator TransitionRoutine(Action onBlack)
    {
        _busy = true;
        gameObject.SetActive(true);

        // ── Fade OUT (to black) ──────────────────────────────────────────
        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));

        // ── Teleport / scene work ────────────────────────────────────────
        onBlack?.Invoke();

        // One frame so the engine moves the player before we fade in
        yield return null;

        // ── Fade IN (to clear) ───────────────────────────────────────────
        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));

        gameObject.SetActive(false);
        _busy = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        SetAlpha(from);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        if (_panel == null) return;
        Color c = _panel.color;
        c.a = a;
        _panel.color = c;
    }
}

