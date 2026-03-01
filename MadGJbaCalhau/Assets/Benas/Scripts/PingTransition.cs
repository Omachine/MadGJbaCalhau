using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PingTransition : MonoBehaviour
{
    public Material material;
    public bool isTransitioning = false;
    private float timer = 0;
    private float r;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject stencilImg = GameObject.FindWithTag("Stencil");
        Image img = stencilImg.GetComponent<Image>();
        material = img.material; // agora EXISTE
        r = 0;
        material.SetFloat("_Radius", r);
        isTransitioning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTransitioning)
        {
            Transition();
            timer += Time.deltaTime;

        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            isTransitioning = true;
        }
    }

    private void Transition()
    {
        r += 1.8f * Time.deltaTime;
        material.SetFloat("_Radius", r);
        if (r >= 1)
        {
            Time.timeScale = 0f;
            isTransitioning = false;
        }
    }

    public void ResumePlay()
    {
        GameObject.FindWithTag("HowCanvas").SetActive(false);
        Time.timeScale = 1f;

    }
}
