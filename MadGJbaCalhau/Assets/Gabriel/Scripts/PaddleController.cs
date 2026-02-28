using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("Ativa isto para a raquete da direita (Jogador 2)")]
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
    public float maxChargeMultiplier = 2.5f;

    // Variáveis de estado
    private float chargeTimer = 0f;
    private float swingTimer = 0f; // Janela de tempo do "parry" (timing perfeito)
    private float savedMultiplier = 1f;
    private bool savedIsHighShot = false;

    // Variável visual
    private Vector3 initialScale;

    void Start()
    {
        // Guardamos o tamanho original para a animação
        initialScale = transform.localScale;
    }

    void Update()
    {
        // --- LÓGICA DE MOVIMENTO ---
        float moveY = 0f;
        float moveX = 0f;

        if (Keyboard.current != null)
        {
            if (!isPlayerTwo)
            {
                // Controlos do Jogador 1 (Esquerda)
                if (Keyboard.current.wKey.isPressed) moveY = 1f;
                else if (Keyboard.current.sKey.isPressed) moveY = -1f;

                if (Keyboard.current.aKey.isPressed) moveX = -1f;
                else if (Keyboard.current.dKey.isPressed) moveX = 1f;
            }
            else
            {
                // Controlos do Jogador 2 (Direita)
                if (Keyboard.current.upArrowKey.isPressed) moveY = 1f;
                else if (Keyboard.current.downArrowKey.isPressed) moveY = -1f;

                if (Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
                else if (Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
            }
        }

        Vector3 movement = new Vector3(moveX, moveY, 0) * speed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;

        newPos.x = Mathf.Clamp(newPos.x, leftLimit, rightLimit);
        newPos.y = Mathf.Clamp(newPos.y, bottomLimit, topLimit);

        transform.position = newPos;

        // --- LÓGICA DE TIMING E EFEITO VISUAL ---
        if (swingTimer > 0)
        {
            swingTimer -= Time.deltaTime;
        }

        bool isHolding = false;
        bool wasReleased = false;
        bool isHighShotPressed = false;

        if (!isPlayerTwo)
        {
            // Jogador 1 ataca com o Rato e o "E"
            if (Mouse.current != null)
            {
                isHolding = Mouse.current.leftButton.isPressed;
                wasReleased = Mouse.current.leftButton.wasReleasedThisFrame;
            }
            if (Keyboard.current != null)
            {
                isHighShotPressed = Keyboard.current.eKey.isPressed;
            }
        }
        else
        {
            // Jogador 2 ataca com o Numpad 0 e Numpad 1
            if (Keyboard.current != null)
            {
                isHolding = Keyboard.current.numpad0Key.isPressed;
                wasReleased = Keyboard.current.numpad0Key.wasReleasedThisFrame;
                isHighShotPressed = Keyboard.current.numpad1Key.isPressed;
            }
        }

        if (isHolding && !wasReleased)
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, 1.5f);

            // EFEITO VISUAL: Encolhe o Scale no X consoante o tempo que seguras
            float squashX = Mathf.Lerp(initialScale.x, initialScale.x * 0.5f, chargeTimer / 1.5f);
            transform.localScale = new Vector3(squashX, initialScale.y, initialScale.z);
        }

        if (wasReleased)
        {
            // DISPARO! Larga a força guardada.
            savedMultiplier = 1f + (chargeTimer / 1.5f) * (maxChargeMultiplier - 1f);
            savedIsHighShot = isHighShotPressed;

            // Abre a janela de timing de 0.2s para a bola bater em nós
            swingTimer = 0.2f;
            chargeTimer = 0f;

            // Reseta o visual para o tamanho normal de forma instantânea ("Snap"!)
            transform.localScale = initialScale;
        }
    }

    // Método chamado pela Bola quando colide connosco
    public void CalculateHitParameters(out float finalHorizontalForce, out float finalJumpForce)
    {
        // Só aplicamos a super força se a bola nos atingir dentro do timing do "swing"
        if (swingTimer > 0f)
        {
            if (savedIsHighShot)
            {
                // Tiro Alto
                finalJumpForce = baseVerticalForce * savedMultiplier;
                finalHorizontalForce = baseHorizontalForce;
            }
            else
            {
                // Tiro Poderoso Frontal
                finalJumpForce = baseVerticalForce;
                finalHorizontalForce = baseHorizontalForce * savedMultiplier;
            }

            // Consome o swing para não aplicar duas vezes
            swingTimer = 0f;
        }
        else
        {
            // O jogador falhou o timing. Rebate com a força base.
            finalJumpForce = baseVerticalForce;
            finalHorizontalForce = baseHorizontalForce;
        }
    }
}