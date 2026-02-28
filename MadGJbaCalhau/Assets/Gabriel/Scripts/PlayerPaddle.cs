using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPaddle : MonoBehaviour
{
    [Header("Player Settings")]
    public bool isPlayerTwo = false;

    [Header("Paddle Movement")]
    public float speed = 12f;
    public float topLimit = 4f;
    public float bottomLimit = -4f;
    public float leftLimit = -8f;
    public float rightLimit = -1f;

    [Header("Attack Mechanics")]
    public float baseHorizontalForce = 12f;
    public float baseVerticalForce = 8f;
    public float baseCurveForce = 25f; // Força base da curva da bola
    public float maxChargeMultiplier = 2.5f;

    // Variáveis de estado
    private float chargeTimer = 0f;
    private float swingTimer = 0f;
    private float savedMultiplier = 1f;
    private bool savedIsHighShot = false;
    private bool savedIsCurveShot = false;
    private float lastMoveY = 0f; // Guarda a última direção Y para saber para onde curvar

    // Variável visual
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float moveY = 0f;
        float moveX = 0f;

        if (Keyboard.current != null)
        {
            if (!isPlayerTwo)
            {
                if (Keyboard.current.wKey.isPressed) moveY = 1f;
                else if (Keyboard.current.sKey.isPressed) moveY = -1f;

                if (Keyboard.current.aKey.isPressed) moveX = -1f;
                else if (Keyboard.current.dKey.isPressed) moveX = 1f;
            }
            else
            {
                if (Keyboard.current.upArrowKey.isPressed) moveY = 1f;
                else if (Keyboard.current.downArrowKey.isPressed) moveY = -1f;

                if (Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
                else if (Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
            }
        }

        // Atualiza a direção vertical (necessário para o botão direito poder curvar para cima também)
        if (moveY != 0) lastMoveY = moveY;

        Vector3 movement = new Vector3(moveX, moveY, 0) * speed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;

        newPos.x = Mathf.Clamp(newPos.x, leftLimit, rightLimit);
        newPos.y = Mathf.Clamp(newPos.y, bottomLimit, topLimit);

        transform.position = newPos;

        if (swingTimer > 0)
        {
            swingTimer -= Time.deltaTime;
        }

        bool isHoldingPower = false;
        bool wasReleasedPower = false;
        bool isHoldingCurve = false;
        bool wasReleasedCurve = false;
        bool isHighShotPressed = false;

        // --- SISTEMA DE INPUTS COMPLETO ---
        if (!isPlayerTwo)
        {
            if (Mouse.current != null)
            {
                isHoldingPower = Mouse.current.leftButton.isPressed;
                wasReleasedPower = Mouse.current.leftButton.wasReleasedThisFrame;

                // Botão direito mantido como pedido
                isHoldingCurve = Mouse.current.rightButton.isPressed;
                wasReleasedCurve = Mouse.current.rightButton.wasReleasedThisFrame;
            }
            if (Keyboard.current != null)
            {
                isHighShotPressed = Keyboard.current.eKey.isPressed;
            }
        }
        else
        {
            if (Keyboard.current != null)
            {
                isHoldingPower = Keyboard.current.numpad0Key.isPressed;
                wasReleasedPower = Keyboard.current.numpad0Key.wasReleasedThisFrame;

                isHoldingCurve = Keyboard.current.numpad2Key.isPressed;
                wasReleasedCurve = Keyboard.current.numpad2Key.wasReleasedThisFrame;

                isHighShotPressed = Keyboard.current.numpad1Key.isPressed;
            }
        }

        // --- LÓGICA DE CARREGAR (CHARGE) E ANIMAÇÃO ---
        if ((isHoldingPower && !wasReleasedPower) || (isHoldingCurve && !wasReleasedCurve))
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, 1.5f);

            // EFEITO VISUAL: Esmaga no eixo Y se estiver a segurar o botão da curva OU se estiver a ir para baixo com o ataque normal
            if (isHoldingCurve || (isHoldingPower && moveY < 0f))
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

        // --- LÓGICA DE LARGAR (RELEASE) ---
        if (wasReleasedPower || wasReleasedCurve)
        {
            savedMultiplier = 1f + (chargeTimer / 1.5f) * (maxChargeMultiplier - 1f);
            savedIsHighShot = isHighShotPressed;

            // A MAGIA DA CURVA: Ativa se soltarmos o botão de curva OU se soltarmos o botão normal enquanto nos movemos para baixo (independentemente do X)
            savedIsCurveShot = wasReleasedCurve || (wasReleasedPower && moveY < 0f);

            swingTimer = 0.2f;
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
                // TIRO ALTO (Balão)
                finalJumpForce = baseVerticalForce * savedMultiplier * 1.5f;
                finalHorizontalForce = baseHorizontalForce * 0.4f;
            }
            else if (savedIsCurveShot)
            {
                // TIRO COM CURVA (Slice)
                finalJumpForce = baseVerticalForce;
                finalHorizontalForce = baseHorizontalForce;

                // Se foi o botão direito, pode curvar na direção em que se move. Se foi o botão esquerdo para baixo, será negativo.
                float curveDir = lastMoveY != 0 ? lastMoveY : -1f;
                finalCurve = curveDir * baseCurveForce * savedMultiplier;
            }
            else
            {
                // TIRO PODEROSO (Normal/Smash)
                finalJumpForce = baseVerticalForce;
                finalHorizontalForce = baseHorizontalForce * savedMultiplier;
            }

            swingTimer = 0f;
        }
        else
        {
            // O jogador falhou o timing da janela
            finalJumpForce = baseVerticalForce;
            finalHorizontalForce = baseHorizontalForce;
        }
    }
}