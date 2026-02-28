using UnityEngine;
using UnityEngine.UI; // Necessário para usar elementos de UI como Text
using System.Collections; // Necessário para usar Coroutines (pausas)
using TMPro; // Adicionado: Necessário para usar o novo sistema de texto TextMeshPro

public class BouncingBall2D : MonoBehaviour
{
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
    public TextMeshProUGUI player1ScoreText; // Alterado para TextMeshProUGUI
    public TextMeshProUGUI player2ScoreText; // Alterado para TextMeshProUGUI
    private int player1Score = 0;
    private int player2Score = 0;

    [Header("Game Logic")]
    public bool isPointActive = true;
    public float maxPaddleReach = 2.5f;
    public float currentCurve = 0f; // Curva ativa no ar
    public float pendingCurve = 0f; // NOVA VARIÁVEL: Curva à espera do quique na mesa
    private int bouncesOnCurrentSide = 0;
    private float lastBounceSide = 0f;

    private float previousXPosition;
    private bool hitNet = false;
    private float scaleMultiplier = 0.15f;

    void Start()
    {
        previousXPosition = transform.position.x;
        // Inicia com os textos a 0
        if (player1ScoreText != null) player1ScoreText.text = "0";
        if (player2ScoreText != null) player2ScoreText.text = "0";

        ApplyImpulse(new Vector2(6f, 0f));
    }

    void Update()
    {
        if (!hitNet && isPointActive)
        {
            // Aplica a força de curva ao longo do tempo (só terá valor depois de bater na mesa)
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

                // NOVA LÓGICA: Ativa o efeito da curva apenas ao bater na mesa
                if (pendingCurve != 0f)
                {
                    currentCurve = pendingCurve; // Inicia a curva no ar após o quique
                    pendingCurve = 0f; // Limpa para não aplicar de novo
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

    public void ApplyImpulse(Vector2 newDirection)
    {
        planeVelocity = newDirection;
        zVelocity = jumpForce;
        hitNet = false;
        bouncesOnCurrentSide = 0;
        currentCurve = 0f; // RESET À CURVA
        pendingCurve = 0f; // RESET À CURVA PENDENTE
    }

    public void StartNewPoint(Vector2 initialDirection)
    {
        transform.position = new Vector3(0, 0, transform.position.z);
        zHeight = 3f;
        isPointActive = true;
        bouncesOnCurrentSide = 0;
        lastBounceSide = 0f;
        ApplyImpulse(initialDirection);
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

        // Bater mais de uma vez = Ponto perdido
        if (bouncesOnCurrentSide >= 2)
        {
            string winner = currentSide < 0 ? "Jogador da Direita" : "Jogador da Esquerda";
            UnityEngine.Debug.Log($"Dois quiques na mesa! Ponto para o {winner}.");
            EndPoint(winner);
        }
    }

    private void CheckOutOdBounds()
    {
        if (transform.position.x > rightOutBoundary)
        {
            if (lastBounceSide == 1 && bouncesOnCurrentSide > 0)
            {
                EndPoint("Jogador da Esquerda");
            }
            else
            {
                EndPoint("Jogador da Direita");
            }
        }
        else if (transform.position.x < leftOutBoundary)
        {
            if (lastBounceSide == -1 && bouncesOnCurrentSide > 0)
            {
                EndPoint("Jogador da Direita");
            }
            else
            {
                EndPoint("Jogador da Esquerda");
            }
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

        StartCoroutine(WaitAndReset(serveDirectionX));
    }

    private IEnumerator WaitAndReset(float serveDirectionX)
    {
        yield return new WaitForSeconds(2f);
        StartNewPoint(new Vector2(serveDirectionX * 6f, 0f));
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
                UnityEngine.Debug.Log("Hit the net!");
                hitNet = true;
                planeVelocity = Vector2.zero;
                transform.Translate(new Vector3(Mathf.Sign(previousXPosition) * 0.2f, 0, 0));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPointActive) return;

        if (collision.CompareTag("Paddle"))
        {
            if (zHeight > maxPaddleReach)
            {
                UnityEngine.Debug.Log("A bola passou por cima da raquete!");
                return;
            }

            // Tenta ir buscar o Player, se não existir, tenta ir buscar a IA
            PlayerPaddle playerPaddle = collision.GetComponent<PlayerPaddle>();
            AIPaddle aiPaddle = collision.GetComponent<AIPaddle>();

            float forcaHorizontal = 10f, forcaVertical = jumpForce, forcaCurva = 0f;
            bool isPlayerTwo = false;

            if (playerPaddle != null)
            {
                playerPaddle.CalculateHitParameters(out forcaHorizontal, out forcaVertical, out forcaCurva);
                isPlayerTwo = playerPaddle.isPlayerTwo;
            }
            else if (aiPaddle != null)
            {
                aiPaddle.CalculateHitParameters(out forcaHorizontal, out forcaVertical, out forcaCurva);
                isPlayerTwo = aiPaddle.isPlayerTwo;
            }
            else
            {
                // Fallback de segurança se esqueceres os scripts
                forcaHorizontal = 10f;
                forcaVertical = jumpForce;
                forcaCurva = 0f;
            }

            if (isPlayerTwo)
                planeVelocity.x = -Mathf.Abs(forcaHorizontal);
            else
                planeVelocity.x = Mathf.Abs(forcaHorizontal);

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