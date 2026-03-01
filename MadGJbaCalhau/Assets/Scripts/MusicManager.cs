using UnityEngine;

/// <summary>
/// Singleton music manager. Attach to a GameObject with an AudioSource.
/// Survives all scene loads — music continues from the exact same position.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   musicClip;
    [SerializeField] private float       volume = 1f;

    private void Awake()
    {
        // Singleton — destroy duplicate if one already exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null && musicClip != null)
        {
            audioSource.clip   = musicClip;
            audioSource.loop   = true;
            audioSource.volume = volume;

            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    /// <summary>Change the volume at runtime.</summary>
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (audioSource != null)
            audioSource.volume = volume;
    }

    /// <summary>Pause the music.</summary>
    public void Pause()
    {
        if (audioSource != null) audioSource.Pause();
    }

    /// <summary>Resume the music from where it was paused.</summary>
    public void Resume()
    {
        if (audioSource != null) audioSource.UnPause();
    }
}

