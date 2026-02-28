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
        }

        CheckNetCollision();
        UpdateVisuals();

        previousXPosition = transform.position.x;
    }

    public void ApplyImpulse(Vector2 newDirection)
    {
        planeVelocity = newDirection;
        zVelocity = jumpForce;
        hitNet = false;
    }

    private void CheckNetCollision()
    {
        if (hitNet) return;

        if (Mathf.Sign(previousXPosition) != Mathf.Sign(transform.position.x) && previousXPosition != 0)
        {
            if (zHeight < minNetHeight)
            {
                UnityEngine.Debug.Log("Hit the net!");
                hitNet = true;
                planeVelocity = Vector2.zero;
                transform.Translate(new Vector3(Mathf.Sign(previousXPosition) * 0.2f, 0, 0));
            }
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