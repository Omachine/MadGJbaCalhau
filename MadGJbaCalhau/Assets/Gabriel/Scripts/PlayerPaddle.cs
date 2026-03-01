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
    public float baseCurveForce = 25f; 
    public float maxChargeMultiplier = 2.5f;

    private float chargeTimer = 0f;
    private float swingTimer = 0f;
    private float savedMultiplier = 1f;
    private bool savedIsHighShot = false;
    private bool savedIsCurveShot = false;
    private float lastMoveY = 0f;


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

        if (!isPlayerTwo)
        {
            if (Mouse.current != null)
            {
                isHoldingPower = Mouse.current.leftButton.isPressed;
                wasReleasedPower = Mouse.current.leftButton.wasReleasedThisFrame;

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

        if ((isHoldingPower && !wasReleasedPower) || (isHoldingCurve && !wasReleasedCurve))
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, 1.5f);

            if (isHoldingCurve)
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

        if (wasReleasedPower || wasReleasedCurve)
        {
            savedMultiplier = 1f + (chargeTimer / 1.5f) * (maxChargeMultiplier - 1f);
            savedIsHighShot = isHighShotPressed;

            savedIsCurveShot = wasReleasedCurve;

            swingTimer = 0.4f;
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


                float curveDir = lastMoveY != 0 ? lastMoveY : 1f;
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