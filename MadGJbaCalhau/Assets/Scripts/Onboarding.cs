using UnityEngine;
using UnityEngine.Video;

public class Onboarding : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    GameObject canvas;
    GameObject onboardingCanvas;
    VideoPlayer player;
    AudioSource music;
    GameObject OnboardPlayer;

    void Start()
    {
        canvas = GameObject.Find("Canvas");
        onboardingCanvas = GameObject.Find("OnboardCanvas");
        music = GameObject.Find("Music").GetComponent<AudioSource>();
        canvas.SetActive(false);
        player = GameObject.Find("OnboardPlayer").GetComponent<VideoPlayer>();
        player.loopPointReached += OnVideoEnd;
        music.enabled = false;
        OnboardPlayer = GameObject.Find("OnboardPlayer");


    }

    void OnVideoEnd(VideoPlayer vp)
    {
        canvas.SetActive(true);
        this.gameObject.SetActive(false);
        music.enabled = true;
        OnboardPlayer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            onboardingCanvas.SetActive(false);
            canvas.SetActive(true);
            music.enabled = true;
            OnboardPlayer.SetActive(false);
        }
    }
}
