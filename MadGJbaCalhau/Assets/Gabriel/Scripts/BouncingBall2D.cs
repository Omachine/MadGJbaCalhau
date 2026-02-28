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

    [Header("Screen Boundaries (Out of Bounds)")]
    public float leftOutBoundary = -10f;
    public float rightOutBoundary = 10f;

    [Header("Game Logic")]
    public bool isPointActive = true;
    private int bouncesOnCurrentSide = 0;
    private float lastBounceSide = 0f;

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
        if (!hitNet && isPointActive)
        {
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

    // NOVA LÓGICA DE BOLA FORA DO ECRÃ
    private void CheckOutOdBounds()
    {
        if (transform.position.x > rightOutBoundary)
        {
            // Se foi pela direita:
            // Já quicou na mesa da direita? Se sim, a direita falhou a receção -> Esquerda Ganha
            // Se não, a esquerda atirou a bola diretamente para fora -> Direita Ganha
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
            // O mesmo para o lado esquerdo
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
            PaddleController paddle = collision.GetComponent<PaddleController>();

            if (paddle != null)
            {
                float forcaHorizontal, forcaVertical;
                paddle.CalculateHitParameters(out forcaHorizontal, out forcaVertical);

                planeVelocity.x = Mathf.Abs(forcaHorizontal); // Força sempre a bola a ir para a frente (direita) se for a raquete esquerda
                zVelocity = forcaVertical;
            }
            else
            {
                planeVelocity.x = 10f;
                zVelocity = jumpForce;
            }

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