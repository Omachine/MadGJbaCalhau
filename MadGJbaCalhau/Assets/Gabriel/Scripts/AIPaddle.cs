using UnityEngine;

public class AIPaddle : MonoBehaviour
{
    [Header("AI Identity")]
    public bool isPlayerTwo = true;

    [Header("AI Difficulty")]
    [Range(1, 5)]
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

    private float chargeTimer = 0f;
    private float swingTimer = 0f;
    private float savedMultiplier = 1f;
    private bool savedIsHighShot = false;
    private bool savedIsCurveShot = false;
    private float lastMoveY = 0f;

    private Vector3 initialScale;
    private BouncingBall2D ballReference;
    private bool aiIsHolding = false;

    // O cérebro da IA para evitar tremores:
    private int plannedShot = 0; // 0 = Normal, 1 = Curve, 2 = HighShot

    void Start()
    {
        initialScale = transform.localScale;
        ballReference = FindObjectOfType<BouncingBall2D>();
    }

    void Update()
    {
        // NOVA LÓGICA: Se a bola ainda não foi encontrada (estava escondida pela cutscene), procura-a agora!
        if (ballReference == null)
        {
            ballReference = FindObjectOfType<BouncingBall2D>();
            if (ballReference == null) return;
        }

        float moveY = 0f;
        float moveX = 0f;

        bool isServingMode = ballReference.isServing;
        bool ballOnMySide = (isPlayerTwo && ballReference.transform.position.x > 0) || (!isPlayerTwo && ballReference.transform.position.x < 0);
        bool ballComingTowards = ballOnMySide && ((isPlayerTwo && ballReference.planeVelocity.x > 0) || (!isPlayerTwo && ballReference.planeVelocity.x < 0));

        float distX = Mathf.Abs(transform.position.x - ballReference.transform.position.x);

        float targetY = transform.position.y;
        float targetX = transform.position.x;

        // --- LÓGICA DE ALVOS DE MOVIMENTO ---
        if (isServingMode && ballOnMySide)
        {
            targetY = ballReference.transform.position.y;

            // Se a IA está a fazer o movimento de bater (swingTimer ativo), atira-se CONTRA a bola!
            if (swingTimer > 0f)
            {
                targetX = ballReference.transform.position.x + (isPlayerTwo ? -1.5f : 1.5f);
            }
            else
            {
                // Se ainda está a preparar, fica atrás da bola flutuante
                targetX = ballReference.transform.position.x + (isPlayerTwo ? 1.5f : -1.5f);
            }
        }
        else if (ballComingTowards && ballReference.isPointActive)
        {
            float timeToReach = distX / Mathf.Max(Mathf.Abs(ballReference.planeVelocity.x), 1f);
            targetY = ballReference.transform.position.y + (ballReference.planeVelocity.y * timeToReach);

            float defaultX = isPlayerTwo ? rightLimit : leftLimit;
            targetX = defaultX + (isPlayerTwo ? -(aiDifficulty * 0.5f) : (aiDifficulty * 0.5f));
        }
        else
        {
            targetY = 0f;
            targetX = isPlayerTwo ? (rightLimit + leftLimit) / 2f + 2f : (rightLimit + leftLimit) / 2f - 2f;
        }

        targetY = Mathf.Clamp(targetY, bottomLimit, topLimit);
        targetX = Mathf.Clamp(targetX, leftLimit, rightLimit);

        // --- MOVIMENTO FLUIDO ---
        float currentSpeed = baseSpeed + (aiDifficulty * 2.5f);
        float errorMargin = 0.2f;

        float diffY = targetY - transform.position.y;
        if (Mathf.Abs(diffY) > errorMargin) moveY = Mathf.Sign(diffY);

        float diffX = targetX - transform.position.x;
        // Durante o serviço OU em níveis difíceis, a IA avança no eixo X
        if (Mathf.Abs(diffX) > errorMargin && (aiDifficulty >= 2 || isServingMode)) moveX = Mathf.Sign(diffX);

        if (moveY != 0) lastMoveY = moveY;

        Vector3 movement = new Vector3(moveX, moveY, 0) * currentSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;
        newPos.x = Mathf.Clamp(newPos.x, leftLimit, rightLimit);
        newPos.y = Mathf.Clamp(newPos.y, bottomLimit, topLimit);
        transform.position = newPos;

        if (swingTimer > 0) swingTimer -= Time.deltaTime;

        // --- LÓGICA DE ATAQUE DA IA ---
        float chargeDist = 4f + aiDifficulty;
        float hitDist = 1.5f;

        bool wantsToHold = false;
        bool wantsToRelease = false;

        if (isServingMode && ballOnMySide)
        {
            // Espera chegar à posição atrás da bola para começar a carregar
            if (distX < 2.5f && Mathf.Abs(diffY) < 1f)
            {
                wantsToHold = true;
                if (chargeTimer > 0.6f)
                {
                    wantsToHold = false;
                    wantsToRelease = true;
                }
            }
        }
        else if (ballComingTowards && ballReference.isPointActive)
        {
            if (distX <= chargeDist && aiDifficulty >= 2) wantsToHold = true;

            if (distX <= hitDist)
            {
                if (ballReference.zHeight <= 2.5f)
                {
                    wantsToHold = false;
                    if (aiIsHolding) wantsToRelease = true;
                }
                else wantsToHold = true;
            }
        }

        // --- EXECUTAR O ATAQUE ---
        if (wantsToHold && !aiIsHolding)
        {
            aiIsHolding = true;
            plannedShot = 0;
            if (aiDifficulty >= 4 && Random.value > 0.6f) plannedShot = 1;
            else if (aiDifficulty >= 3 && Random.value > 0.8f) plannedShot = 2;
        }

        if (aiIsHolding)
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, 1.5f);

            if (plannedShot == 1)
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
        else if (!aiIsHolding)
        {
            chargeTimer = 0f;
            transform.localScale = initialScale;
        }

        if (wantsToRelease)
        {
            savedMultiplier = 1f + (chargeTimer / 1.5f) * (maxChargeMultiplier - 1f);
            savedIsHighShot = (plannedShot == 2);
            savedIsCurveShot = (plannedShot == 1);

            // Tempo suficiente para a IA conseguir "dar o salto" até bater na bola
            swingTimer = 0.4f;
            chargeTimer = 0f;
            aiIsHolding = false;
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