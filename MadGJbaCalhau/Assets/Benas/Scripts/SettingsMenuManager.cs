using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("Audio")]
    public Slider masterVol, musicVol, sfxVol;
    public AudioMixer mainAudioMixer;

    [Header("Graphics")]

    const string MASTER_KEY = "MasterVolume";
    const string MUSIC_KEY = "MusicVolume";
    const string SFX_KEY = "SfxVolume";

    [Header("Toggle Sound")]
    public AudioSource audioSource; // seu AudioSource
    public AudioClip clip;          // clip que quer tocar
    private bool isPlaying = false; // flag para toggle

    [Header("SFX Preview")]
    [SerializeField] private AudioSource sfxPreviewSource;

    public GameObject canvas;
    public GameObject optionCanvas;

    void Awake()
    {

    }

    private void Start()
    {
        LoadAudioSettings();
    }

    // ---------- QUALITY ----------
    public void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("Quality", index);
        PlayerPrefs.Save();
    }

    // ---------- AUDIO ----------
    public void ChangeMasterVolume()
    {
        SetVolume(MASTER_KEY, "MasterVol", masterVol.value);
    }

    public void ChangeMusicVolume()
    {
        SetVolume(MUSIC_KEY, "MusicVol", musicVol.value);
    }

    public void ChangeSfxVolume()
    {
        StartSfxPreview();
        SetVolume(SFX_KEY, "SfxVol", sfxVol.value);
    }

    void SetVolume(string prefKey, string mixerParam, float sliderValue)
    {
        // Evita log(0)
        float dB = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;

        mainAudioMixer.SetFloat(mixerParam, dB);
        PlayerPrefs.SetFloat(prefKey, sliderValue); // guarda o valor do slider (0–1)
        PlayerPrefs.Save();
    }

    void LoadAudioSettings()
    {
        float master = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        masterVol.SetValueWithoutNotify(master);
        musicVol.SetValueWithoutNotify(music);
        sfxVol.SetValueWithoutNotify(sfx);

        mainAudioMixer.SetFloat("MasterVol", Mathf.Log10(master) * 20f);
        mainAudioMixer.SetFloat("MusicVol", Mathf.Log10(music) * 20f);
        mainAudioMixer.SetFloat("SfxVol", Mathf.Log10(sfx) * 20f);
    }


    // ---------- TOGGLE AUDIO ----------
    public void TogglePlay()
    {
        if (isPlaying)
        {
            audioSource.Stop();
            isPlaying = false;
        }
        else
        {
            audioSource.clip = clip;
            audioSource.Play();
            isPlaying = true;
        }
    }

    public void ReturnFromOptions()
    {
        canvas.SetActive(true);
        optionCanvas.SetActive(false);
       
    }

    public void StartSfxPreview()
    {
        if (!sfxPreviewSource.isPlaying)
            sfxPreviewSource.Play();
    }

    public void StopSfxPreview()
    {
        if (sfxPreviewSource.isPlaying)
            sfxPreviewSource.Stop();
    }
}

