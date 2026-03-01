using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject optionCanvas;
    [SerializeField] private GameObject howToPlayCanvas;
    [SerializeField] private GameObject howToPlayCanvas2;
    [SerializeField] private GameObject howToPlayCanvas3;
    [SerializeField] private GameObject cam;

    private void Start()
    {
        optionCanvas.SetActive(false);
    }
    public void PlayLevel1()
    {
        SceneManager.LoadScene("GonScene");
    }

    public void Options()
    {
        canvas.SetActive(false);
        optionCanvas.SetActive(true);
    }
    public void HowToPlay()
    {
        canvas.SetActive(false);
        howToPlayCanvas.SetActive(true);
        howToPlayCanvas2.SetActive(false);
        howToPlayCanvas3.SetActive(false);
    }
    public void HowToPlay2()
    {
        canvas.SetActive(false);
        howToPlayCanvas.SetActive(false);
        howToPlayCanvas2.SetActive(true);
        howToPlayCanvas3.SetActive(false);
    }
    public void HowToPlay3()
    {
        canvas.SetActive(false);
        howToPlayCanvas.SetActive(false);
        howToPlayCanvas2.SetActive(false);
        howToPlayCanvas3.SetActive(true);
    }

    public void ReturnFromOptions()
    {
        canvas.SetActive(true);
        optionCanvas.SetActive(false);
    }
    public void ReturnFromHowToPlay()
    {
        canvas.SetActive(true);
        howToPlayCanvas.SetActive(false);
        howToPlayCanvas2.SetActive(false);
        howToPlayCanvas3.SetActive(false);
    }
    public void Exit()
    {
        Application.Quit();
    }
}
