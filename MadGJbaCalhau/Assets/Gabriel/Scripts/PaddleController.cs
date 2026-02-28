using UnityEngine;
using UnityEngine.InputSystem; // Necessário para o novo Input System

public class PaddleController : MonoBehaviour
{
    [Header("Paddle Settings")]
    public float speed = 12f;
    public float topLimit = 4f;
    public float bottomLimit = -4f;

    void Update()
    {
        float move = 0f;

        // Ler as teclas através do novo Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                move = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                move = -1f;
        }

        // Calcular a nova posição
        Vector3 newPos = transform.position + new Vector3(0, move * speed * Time.deltaTime, 0);

        // Limitar para que a raquete não saia do ecrã
        newPos.y = Mathf.Clamp(newPos.y, bottomLimit, topLimit);

        transform.position = newPos;
    }
}