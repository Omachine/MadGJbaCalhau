using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement; // Necessário para mudar de Scene

public class BouncingBall2D : MonoBehaviour
{
    // NOVO: Variável estática que guarda o nível apenas enquanto o jogo estiver aberto!
    public static int nivelTorneioAtual = 1;

    [Header("Tournament Settings")]
    public int pontosParaVencer = 11;
    [Tooltip("Escreve aqui o nome exato da Scene do teu mapa/menu")]
    public string cenaDoMapa = "Mapa";

    [Header("2D Physics (Table Movement)")]
    public Vector2 planeVelocity;

    [Header("Z Physics (Simulated Height)")]
    public float zHeight = 0f;
    public float zVelocity = 0f;
    public float gravity = 15f;
    public float jumpForce = 8f;

    [Header("Visual References")]
    public Transform ballVisual;
    public Transform ballShadow;

    [Header("Net Settings")]
    public float minNetHeight = 1.5f;

    [Header("Arena Boundaries")]
    public float topBoundary = 4.5f;
    public float bottomBoundary = -4.5f;

    [Header("Screen Boundaries (Out of Bounds)")]
    public float leftOutBoundary = -10f;
    public float rightOutBoundary = 10f;

    [Header("Score UI")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    private int player1Score = 0;
    private int player2Score = 0;

    [Header("Game Logic")]
    public bool isPointActive = false;
    public bool isServing = false;
    public float maxPaddleReach = 2.5f;
    public float currentCurve = 0f;
    public float pendingCurve = 0f;
    private int bouncesOnCurrentSide = 0;
    private float lastBounceSide = 0f;

    private float previousXPosition;
    private bool hitNet = false;
    private float scaleMultiplier = 0.15f;

    void Start()
    {
        previousXPosition = transform.position.x;
        if (player1ScoreText != null) player1ScoreText.text = "0";
        if (player2ScoreText != null) player2ScoreText.text = "0";

        isPointActive = false;
        isServing = false;

        StartCoroutine(WaitAndServe(1f));
    }

    void Update()
    {
        if (isServing)
        {
            // Altura mais baixa (1.5f) para garantir que a raquete alcança sempre a bola durante o serviço
            zHeight = 1.5f + Mathf.Sin(Time.time * 3f) * 0.2f;
            UpdateVisuals();
            return;
        }

        if (!hitNet && isPointActive)
        {
            planeVelocity.y += currentCurve * Time.deltaTime;
            transform.Translate(planeVelocity * Time.deltaTime);
        }

        if (isPointActive)
        {
            zVelocity -= gravity * Time.deltaTime;
            zHeight += zVelocity * Time.deltaTime;

            if (zHeight <= 0f)
            {
                zHeight = 0f;
                zVelocity = Mathf.Abs(zVelocity) * 0.75f;

                if (zVelocity < 1.5f) zVelocity = 0f;

                if (pendingCurve != 0f)
                {
                    currentCurve = pendingCurve;
                    pendingCurve = 0f;
                }

                RegisterBounce();
            }

            CheckBoundaries();
            CheckOutOdBounds();
            CheckNetCollision();
        }

        UpdateVisuals();
        previousXPosition = transform.position.x;
    }

    private void RegisterBounce()
    {
        float currentSide = Mathf.Sign(transform.position.x);

        if (lastBounceSide != currentSide || bouncesOnCurrentSide == 0)
        {
            lastBounceSide = currentSide;
            bouncesOnCurrentSide = 1;
        }
        else
        {
            bouncesOnCurrentSide++;
        }

        if (bouncesOnCurrentSide >= 2)
        {
            string winner = currentSide < 0 ? "Jogador da Direita" : "Jogador da Esquerda";
            EndPoint(winner);
        }
    }

    private void CheckOutOdBounds()
    {
        if (transform.position.x > rightOutBoundary)
        {
            if (lastBounceSide == 1 && bouncesOnCurrentSide > 0) EndPoint("Jogador da Esquerda");
            else EndPoint("Jogador da Direita");
        }
        else if (transform.position.x < leftOutBoundary)
        {
            if (lastBounceSide == -1 && bouncesOnCurrentSide > 0) EndPoint("Jogador da Direita");
            else EndPoint("Jogador da Esquerda");
        }
    }

    private void EndPoint(string winner)
    {
        isPointActive = false;
        planeVelocity = Vector2.zero;
        zVelocity = 0f;
        UnityEngine.Debug.Log($"PONTO para o {winner}!");

        float serveDirectionX = 1f;

        if (winner == "Jogador da Esquerda")
        {
            player1Score++;
            if (player1ScoreText != null) player1ScoreText.text = player1Score.ToString();
            serveDirectionX = 1f;
        }
        else
        {
            player2Score++;
            if (player2ScoreText != null) player2ScoreText.text = player2Score.ToString();
            serveDirectionX = -1f;
        }

        // --- NOVA LÓGICA DE FIM DE PARTIDA (11 PONTOS) ---
        if (player1Score >= pontosParaVencer)
        {
            UnityEngine.Debug.Log("JOGADOR 1 VENCEU O JOGO!");

            // Sobe de nível no torneio (apenas na memória da sessão atual)
            nivelTorneioAtual++;

            // Volta para a cena do mapa
            SceneManager.LoadScene(cenaDoMapa);
            return; // Interrompe para não servir mais bolas
        }
        else if (player2Score >= pontosParaVencer)
        {
            UnityEngine.Debug.Log("OPONENTE VENCEU O JOGO!");

            // Volta para a cena do mapa (Sem subir de nível porque perdemos)
            SceneManager.LoadScene(cenaDoMapa);
            return; // Interrompe para não servir mais bolas
        }

        StartCoroutine(WaitAndServe(serveDirectionX));
    }

    private IEnumerator WaitAndServe(float serveSideX)
    {
        // Esconde a bola fora do ecrã durante o tempo de espera para evitar colisões acidentais
        isServing = false;
        isPointActive = false;
        transform.position = new Vector3(0, 100f, 0);

        yield return new WaitForSeconds(1.5f);

        float spawnX = serveSideX > 0 ? 6f : -6f;
        transform.position = new Vector3(spawnX, 0, transform.position.z);

        planeVelocity = Vector2.zero;
        zVelocity = 0f;
        currentCurve = 0f;
        pendingCurve = 0f;
        bouncesOnCurrentSide = 0;
        lastBounceSide = serveSideX;
        hitNet = false;

        isServing = true;
    }

    private void CheckBoundaries()
    {
        if (transform.position.y >= topBoundary)
        {
            transform.position = new Vector3(transform.position.x, topBoundary, transform.position.z);
            planeVelocity.y = -Mathf.Abs(planeVelocity.y);
        }
        else if (transform.position.y <= bottomBoundary)
        {
            transform.position = new Vector3(transform.position.x, bottomBoundary, transform.position.z);
            planeVelocity.y = Mathf.Abs(planeVelocity.y);
        }
    }

    private void CheckNetCollision()
    {
        if (hitNet) return;
        if (Mathf.Sign(previousXPosition) != Mathf.Sign(transform.position.x) && previousXPosition != 0)
        {
            bouncesOnCurrentSide = 0;
            if (zHeight < minNetHeight)
            {
                hitNet = true;
                planeVelocity = Vector2.zero;
                transform.Translate(new Vector3(Mathf.Sign(previousXPosition) * 0.2f, 0, 0));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool wasServing = false;

        if (isServing && collision.CompareTag("Paddle"))
        {
            isServing = false;
            isPointActive = true;
            wasServing = true;
        }

        if (!isPointActive && !wasServing) return;

        if (collision.CompareTag("Paddle"))
        {
            // Se for um serviço, ignoramos o limite de altura para garantir que funciona sempre!
            if (zHeight > maxPaddleReach && !wasServing) return;

            PlayerPaddle playerPaddle = collision.GetComponent<PlayerPaddle>();
            AIPaddle aiPaddle = collision.GetComponent<AIPaddle>();

            float forcaHorizontal = 10f, forcaVertical = jumpForce, forcaCurva = 0f;
            bool isPaddleTwo = false;

            if (playerPaddle != null)
            {
                playerPaddle.CalculateHitParameters(out forcaHorizontal, out forcaVertical, out forcaCurva);
                isPaddleTwo = playerPaddle.isPlayerTwo;
            }
            else if (aiPaddle != null)
            {
                aiPaddle.CalculateHitParameters(out forcaHorizontal, out forcaVertical, out forcaCurva);
                isPaddleTwo = aiPaddle.isPlayerTwo;
            }

            if (isPaddleTwo) planeVelocity.x = -Mathf.Abs(forcaHorizontal);
            else planeVelocity.x = Mathf.Abs(forcaHorizontal);

            zVelocity = forcaVertical;
            pendingCurve = forcaCurva;
            currentCurve = 0f;

            float hitOffset = transform.position.y - collision.transform.position.y;
            planeVelocity.y = hitOffset * 4f;

            hitNet = false;
            bouncesOnCurrentSide = 0;
        }
        else if (collision.CompareTag("Wall"))
        {
            planeVelocity.x = -10f;
            zVelocity = jumpForce;
            hitNet = false;
            bouncesOnCurrentSide = 0;
            pendingCurve = 0f;
            currentCurve = 0f;
        }
    }

    private void UpdateVisuals()
    {
        if (ballVisual == null || ballShadow == null) return;
        ballVisual.localPosition = new Vector3(0, zHeight, 0);
        float fakeScale = 1f + (zHeight * scaleMultiplier);
        ballVisual.localScale = new Vector3(fakeScale, fakeScale, 1f);

        float shadowOpacity = Mathf.Clamp01(1f - (zHeight * 0.15f));
        SpriteRenderer shadowSR = ballShadow.GetComponent<SpriteRenderer>();
        if (shadowSR != null)
        {
            Color color = shadowSR.color;
            color.a = shadowOpacity;
            shadowSR.color = color;
            float shadowScale = Mathf.Max(0.5f, 1f - (zHeight * 0.05f));
            ballShadow.localScale = new Vector3(shadowScale, shadowScale, 1f);
        }
    }
}