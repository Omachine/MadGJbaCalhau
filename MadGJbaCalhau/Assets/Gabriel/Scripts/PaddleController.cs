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
    public float baseVerticalForce = 8f;       // Salto normal (Z)
    public float maxChargeMultiplier = 2.5f;   // Quanto a força é multiplicada no máximo
    private float chargeTimer = 0f;

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

        // --- LÓGICA DE CARREGAR O ATAQUE ---
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            chargeTimer += Time.deltaTime;
            // Limita o charge máximo a 1.5 segundos
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, 1.5f);
        }
        else
        {
            // Se largar o botão, perde a força acumulada
            chargeTimer = 0f;
        }
    }

    // Método chamado pela Bola quando colide connosco
    public void CalculateHitParameters(out float finalHorizontalForce, out float finalJumpForce)
    {
        // O multiplicador vai de 1.0 (sem charge) até maxChargeMultiplier (charge no máximo)
        float multiplier = 1f + (chargeTimer / 1.5f) * (maxChargeMultiplier - 1f);

        // Verifica se a tecla "1" está pressionada para o tiro alto
        bool isHighShot = Keyboard.current != null && Keyboard.current.digit1Key.isPressed;

        if (isHighShot)
        {
            // Tiro Alto: Muita força vertical (Z), mas a velocidade horizontal é a normal
            finalJumpForce = baseVerticalForce * multiplier;
            finalHorizontalForce = baseHorizontalForce;
        }
        else
        {
            // Tiro Poderoso Frontal: Força vertical normal, mas muita velocidade horizontal
            finalJumpForce = baseVerticalForce;
            finalHorizontalForce = baseHorizontalForce * multiplier;
        }

        // Dá reset ao charge depois de bater na bola
        chargeTimer = 0f;
    }
}