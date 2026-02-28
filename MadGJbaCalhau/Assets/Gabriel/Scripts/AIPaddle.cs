using UnityEngine;

public class AIPaddle : MonoBehaviour
{
    [Header("AI Identity")]
    [Tooltip("Normalmente a IA é o Player Two (Direita)")]
    public bool isPlayerTwo = true;

    [Header("AI Difficulty")]
    [Range(1, 5)]
    [Tooltip("1 = Iniciante, 2 = Amador, 3 = Profissional, 4 = Campeão, 5 = BOSS FINAL")]
    public int aiDifficulty = 1;

    [Header("Paddle Movement")]
    public float baseSpeed = 10f;
    public float topLimit = 4f;
    public float bottomLimit = -4f;
    public float leftLimit = -8f;
    public float rightLimit = -1f;

    [Header("Attack Mechanics")]
    public float baseHorizontalForce = 12f;
    public float baseVerticalForce = 8f;
    public float baseCurveForce = 25f;
    public float maxChargeMultiplier = 2.5f;

    // Variáveis de estado
    private float chargeTimer = 0f;
    private float swingTimer = 0f;
    private float savedMultiplier = 1f;
    private bool savedIsHighShot = false;
    private bool savedIsCurveShot = false;
    private float lastMoveY = 0f;

    // Variáveis Visuais e IA
    private Vector3 initialScale;
    private BouncingBall2D ballReference;
    private bool aiIsHolding = false;

    void Start()
    {
        initialScale = transform.localScale;
        ballReference = FindObjectOfType<BouncingBall2D>();
    }

    void Update()
    {
        if (ballReference == null || !ballReference.isPointActive) return;

        float moveY = 0f;
        float moveX = 0f;

        bool ballComingTowards = (isPlayerTwo && ballReference.planeVelocity.x > 0) || (!isPlayerTwo && ballReference.planeVelocity.x < 0);
        float distX = Mathf.Abs(transform.position.x - ballReference.transform.position.x);

        // --- LÓGICA DE MOVIMENTO (Agora com previsão de trajetória!) ---
        float targetY = 0f;

        if (ballComingTowards)
        {
            // Calcula quanto tempo a bola vai demorar a chegar até à raquete
            float timeToReach = distX / Mathf.Max(Mathf.Abs(ballReference.planeVelocity.x), 1f);

            // Prevê onde a bola vai estar nesse tempo
            float predictedY = ballReference.transform.position.y + (ballReference.planeVelocity.y * timeToReach);

            // Mantém a previsão dentro dos limites da mesa
            targetY = Mathf.Clamp(predictedY, bottomLimit, topLimit);
        }

        // Margem de erro e velocidade escalam melhor com a dificuldade
        float errorMargin = (5 - aiDifficulty) * 0.5f;
        float currentSpeed = baseSpeed + (aiDifficulty * 2.5f); // Ficam bem mais rápidas nas dificuldades altas!

        if (transform.position.y < targetY - errorMargin) moveY = 1f;
        else if (transform.position.y > targetY + errorMargin) moveY = -1f;

        // O Boss (Nível 5) move-se também no eixo X para antecipar
        if (aiDifficulty == 5 && ballComingTowards)
        {
            float targetX = isPlayerTwo ? rightLimit - 1f : leftLimit + 1f;
            if (transform.position.x < targetX) moveX = 1f;
            else if (transform.position.x > targetX) moveX = -1f;
        }

        if (moveY != 0) lastMoveY = moveY;

        Vector3 movement = new Vector3(moveX, moveY, 0) * currentSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;
        newPos.x = Mathf.Clamp(newPos.x, leftLimit, rightLimit);
        newPos.y = Mathf.Clamp(newPos.y, bottomLimit, topLimit);
        transform.position = newPos;

        if (swingTimer > 0) swingTimer -= Time.deltaTime;

        // --- LÓGICA DE ATAQUE DA IA ---
        float chargeDist = 4f + aiDifficulty;
        // Quanto mais difícil, mais perto a bola tem de estar para a IA largar o botão (timing perfeito)
        float hitDist = 1.8f - (aiDifficulty * 0.2f);

        bool wantsToHold = false;
        bool wantsToRelease = false;

        if (ballComingTowards)
        {
            if (distX <= chargeDist && aiDifficulty >= 2)
            {
                wantsToHold = true;
            }

            // A IA verifica se a bola está perto o suficiente E se não está alta demais
            if (distX <= hitDist)
            {
                if (ballReference.zHeight <= 2.5f) // maxPaddleReach
                {
                    wantsToHold = false; // Larga o botão!
                    if (aiIsHolding) wantsToRelease = true;
                }
                else
                {
                    // Se a bola está demasiado alta, a IA é inteligente e continua a segurar à espera que caia
                    wantsToHold = true;
                }
            }
        }

        // Executar as ações simuladas
        if (wantsToHold)
        {
            aiIsHolding = true;
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, 1.5f);

            bool curveRandom = (aiDifficulty >= 4 && Random.value > 0.5f);
            if (curveRandom)
            {
                float squashY = Mathf.Lerp(initialScale.y, initialScale.y * 0.5f, chargeTimer / 1.5f);
                transform.localScale = new Vector3(initialScale.x, squashY, initialScale.z);
            }
            else
            {
                float squashX = Mathf.Lerp(initialScale.x, initialScale.x * 0.5f, chargeTimer / 1.5f);
                transform.localScale = new Vector3(squashX, initialScale.y, initialScale.z);
            }
        }
        else if (aiIsHolding && wantsToRelease)
        {
            savedMultiplier = 1f + (chargeTimer / 1.5f) * (maxChargeMultiplier - 1f);

            // Seleção de tiro mais inteligente
            savedIsHighShot = (aiDifficulty >= 3 && Random.value > 0.85f && ballReference.zHeight < 1.0f);
            savedIsCurveShot = (aiDifficulty >= 4 && Random.value > 0.5f);

            swingTimer = 0.2f;
            chargeTimer = 0f;
            aiIsHolding = false;
            transform.localScale = initialScale;
        }
        else if (!ballComingTowards)
        {
            aiIsHolding = false;
            chargeTimer = 0f;
            transform.localScale = initialScale;
        }
    }

    public void CalculateHitParameters(out float finalHorizontalForce, out float finalJumpForce, out float finalCurve)
    {
        finalCurve = 0f;

        if (swingTimer > 0f)
        {
            if (savedIsHighShot)
            {
                finalJumpForce = baseVerticalForce * savedMultiplier * 1.5f;
                finalHorizontalForce = baseHorizontalForce * 0.4f;
            }
            else if (savedIsCurveShot)
            {
                finalJumpForce = baseVerticalForce;
                finalHorizontalForce = baseHorizontalForce;
                float curveDir = lastMoveY != 0 ? lastMoveY : -1f;
                finalCurve = curveDir * baseCurveForce * savedMultiplier;
            }
            else
            {
                finalJumpForce = baseVerticalForce;
                finalHorizontalForce = baseHorizontalForce * savedMultiplier;
            }
            swingTimer = 0f;
        }
        else
        {
            finalJumpForce = baseVerticalForce;
            finalHorizontalForce = baseHorizontalForce;
        }
    }
}