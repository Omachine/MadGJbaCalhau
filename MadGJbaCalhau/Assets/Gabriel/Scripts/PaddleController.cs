using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
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
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                moveY = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                moveY = -1f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                moveX = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                moveX = 1f;
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

        if (Mouse.current != null)
        {
            bool isHolding = Mouse.current.leftButton.isPressed;
            bool wasReleased = Mouse.current.leftButton.wasReleasedThisFrame;

            if (isHolding && !wasReleased)
            {
                chargeTimer += Time.deltaTime;
                chargeTimer = Mathf.Clamp(chargeTimer, 0f, 1.5f);

                // EFEITO VISUAL: Encolhe o Scale no X consoante o tempo que seguras
                // Lerp vai do Scale original (100%) até metade (50%)
                float squashX = Mathf.Lerp(initialScale.x, initialScale.x * 0.5f, chargeTimer / 1.5f);
                transform.localScale = new Vector3(squashX, initialScale.y, initialScale.z);
            }

            if (wasReleased)
            {
                // DISPARO! Larga a força guardada.
                savedMultiplier = 1f + (chargeTimer / 1.5f) * (maxChargeMultiplier - 1f);

                // MUDANÇA: Verifica o botão "E"
                savedIsHighShot = Keyboard.current != null && Keyboard.current.eKey.isPressed;

                // Abre a janela de timing de 0.2s para a bola bater em nós
                swingTimer = 0.2f;
                chargeTimer = 0f;

                // Reseta o visual para o tamanho normal de forma instantânea ("Snap"!)
                transform.localScale = initialScale;
            }
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
            // O jogador falhou o timing (ou estava só a segurar o botão). Rebate com a força base.
            finalJumpForce = baseVerticalForce;
            finalHorizontalForce = baseHorizontalForce;
        }
    }
}