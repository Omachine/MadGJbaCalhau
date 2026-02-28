using UnityEngine;

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

    [Header("Game Logic")]
    public bool isPointActive = true;
    private int bouncesOnCurrentSide = 0;
    private float lastBounceSide = 0f; // -1 para esquerda, 1 para direita

    private float previousXPosition;
    private bool hitNet = false;
    private float scaleMultiplier = 0.15f;

    void Start()
    {
        previousXPosition = transform.position.x;
        ApplyImpulse(new Vector2(6f, 0f));
    }

    void Update()
    {
        if (!hitNet)
        {
            transform.Translate(planeVelocity * Time.deltaTime);
        }

        zVelocity -= gravity * Time.deltaTime;
        zHeight += zVelocity * Time.deltaTime;

        if (zHeight <= 0f)
        {
            zHeight = 0f;
            zVelocity = Mathf.Abs(zVelocity) * 0.75f;

            if (zVelocity < 1.5f) zVelocity = 0f;

            // Regista o quique no chão apenas se o ponto ainda estiver a decorrer
            if (isPointActive)
            {
                RegisterBounce();
            }
        }

        CheckBoundaries();
        CheckNetCollision();
        UpdateVisuals();

        previousXPosition = transform.position.x;
    }

    public void ApplyImpulse(Vector2 newDirection)
    {
        planeVelocity = newDirection;
        zVelocity = jumpForce;
        hitNet = false;
        bouncesOnCurrentSide = 0; // Reset aos quiques quando há impulso
    }

    // Método que podes chamar através de um botão ou Game Manager para recomeçar
    public void StartNewPoint(Vector2 initialDirection)
    {
        transform.position = new Vector3(0, 0, transform.position.z); // Volta ao centro
        zHeight = 3f;
        isPointActive = true;
        bouncesOnCurrentSide = 0;
        lastBounceSide = 0f;
        ApplyImpulse(initialDirection);
    }

    private void RegisterBounce()
    {
        // Descobre em que lado a bola bateu (esquerda ou direita)
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

        // Regra do Ping Pong: Bater mais de uma vez = Ponto perdido!
        if (bouncesOnCurrentSide >= 2)
        {
            isPointActive = false; // Bloqueia a contagem extra

            string loser = currentSide < 0 ? "Jogador da Esquerda" : "Jogador da Direita";
            UnityEngine.Debug.Log($"PONTO! O {loser} deixou a bola quicar {bouncesOnCurrentSide} vezes.");

            // Para a bola completamente para não continuar a saltitar pela mesa
            planeVelocity = Vector2.zero;
            zVelocity = 0f;
        }
    }

    private void CheckBoundaries()
    {
        // Verifica se bateu na parede invisível de cima
        if (transform.position.y >= topBoundary)
        {
            transform.position = new Vector3(transform.position.x, topBoundary, transform.position.z);
            planeVelocity.y = -Mathf.Abs(planeVelocity.y);
        }
        // Verifica se bateu na parede invisível de baixo
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
            // Reset aos quiques quando a bola cruza a rede com sucesso
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

        // Bateu na Raquete (Esquerda)
        if (collision.CompareTag("Paddle"))
        {
            // Verifica se a raquete tem o controlador de ataque
            PaddleController paddle = collision.GetComponent<PaddleController>();

            if (paddle != null)
            {
                float forcaHorizontal, forcaVertical;
                paddle.CalculateHitParameters(out forcaHorizontal, out forcaVertical);

                planeVelocity.x = forcaHorizontal; // Força X calculada pela raquete
                zVelocity = forcaVertical;         // Força Z calculada pela raquete
            }
            else
            {
                // Fallback normal
                planeVelocity.x = 10f;
                zVelocity = jumpForce;
            }

            // Efeito da raquete (Y)
            float hitOffset = transform.position.y - collision.transform.position.y;
            planeVelocity.y = hitOffset * 4f;

            hitNet = false;
            bouncesOnCurrentSide = 0; // Reset ao ser devolvida
        }
        // Bateu na Parede (Direita)
        else if (collision.CompareTag("Wall"))
        {
            planeVelocity.x = -10f; // Força de devolução da parede
            zVelocity = jumpForce;
            hitNet = false;
            bouncesOnCurrentSide = 0;
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