using UnityEngine;
using UnityEngine.UI; // Usamos UI para sobrepor as imagens ao jogo
using System.Collections;

public class IntroCutscene : MonoBehaviour
{
    [Header("Cutscene Timings")]
    [Tooltip("Tempo antes da animação começar")]
    public float tempoEsperaInicial = 0.5f;
    [Tooltip("Quão rápido o jogador desliza para o ecrã")]
    public float tempoDeslizeJogador = 0.3f;
    [Tooltip("Quão rápido o VS explode no meio")]
    public float tempoExplosaoVS = 0.25f;
    [Tooltip("Quão rápido o Inimigo desliza para o ecrã")]
    public float tempoDeslizeInimigo = 0.3f;
    [Tooltip("Tempo em que ficam a olhar um para o outro")]
    public float tempoTensao = 1.2f;
    [Tooltip("Quão rápido a cutscene desvanece no final")]
    public float tempoDesvanecer = 0.4f;

    [Header("UI Elements (RectTransforms)")]
    public RectTransform playerImage;
    public RectTransform vsImage;
    public RectTransform enemyImage;
    public CanvasGroup cutsceneCanvasGroup; // Para desvanecer a cutscene no final

    [Header("Game Reference")]
    [Tooltip("Arrasta o objeto da Bola para aqui. Ele será ativado quando a cutscene acabar.")]
    public GameObject ballObject;

    void Start()
    {
        // 1. Desativar a bola para o jogo não começar enquanto a cutscene decorre
        if (ballObject != null) ballObject.SetActive(false);

        // 2. Esconder tudo inicialmente (Posições fora do ecrã e Escala a 0)
        playerImage.anchoredPosition = new Vector2(-1500f, playerImage.anchoredPosition.y);
        enemyImage.anchoredPosition = new Vector2(1500f, enemyImage.anchoredPosition.y);
        vsImage.localScale = Vector3.zero;

        // 3. Iniciar a magia!
        StartCoroutine(PlayCutsceneRoutine());
    }

    private IEnumerator PlayCutsceneRoutine()
    {
        // Espera um bocadinho antes de começar
        yield return new WaitForSeconds(tempoEsperaInicial);

        // --- PASSO 1: O JOGADOR ENTRA (Desliza da Esquerda) ---
        yield return SlideUI(playerImage, new Vector2(-350f, playerImage.anchoredPosition.y), tempoDeslizeJogador);

        // --- PASSO 2: O "VS" EXPLODE NO ECRÃ ---
        yield return PopUI(vsImage, tempoExplosaoVS);

        // --- PASSO 3: O INIMIGO ENTRA (Desliza da Direita) ---
        yield return SlideUI(enemyImage, new Vector2(350f, enemyImage.anchoredPosition.y), tempoDeslizeInimigo);

        // --- PASSO 4: PAUSA PARA TENSÃO ---
        // Aqui os jogadores olham um para o outro
        yield return new WaitForSeconds(tempoTensao);

        // --- PASSO 5: DESAPARECER TUDO ---
        // Desvanece a transparência (Alpha) de tudo para 0
        float elapsed = 0f;
        while (elapsed < tempoDesvanecer)
        {
            elapsed += Time.deltaTime;
            cutsceneCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / tempoDesvanecer);
            yield return null;
        }

        // --- PASSO 6: COMEÇAR O JOGO ---
        // Ativamos a bola, o que fará o Start() dela correr e iniciar o primeiro serviço!
        if (ballObject != null) ballObject.SetActive(true);

        // Desativa a cutscene inteira para não pesar no jogo
        gameObject.SetActive(false);
    }

    // --- FUNÇÕES DE ANIMAÇÃO MATEMÁTICA ---

    // Animação de deslize suave
    private IEnumerator SlideUI(RectTransform uiElement, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = uiElement.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Usamos SmoothStep para não ser um movimento robótico (acelera e trava no fim)
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            uiElement.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        uiElement.anchoredPosition = targetPosition;
    }

    // Animação de explosão (Escala vai a 1.5 e volta a 1.0 como uma mola)
    private IEnumerator PopUI(RectTransform uiElement, float duration)
    {
        float elapsed = 0f;
        float halfDuration = duration / 2f;

        // Aumenta muito (0 até 1.5)
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            uiElement.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1.5f, 1.5f, 1.5f), t);
            yield return null;
        }

        // Encolhe para o tamanho normal (1.5 até 1.0)
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            uiElement.localScale = Vector3.Lerp(new Vector3(1.5f, 1.5f, 1.5f), Vector3.one, t);
            yield return null;
        }

        uiElement.localScale = Vector3.one;
    }
}