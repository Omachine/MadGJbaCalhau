using UnityEngine;

/// <summary>
/// Place this on a trigger collider that covers a room.
/// When the player walks in, CameraFollow automatically switches to these bounds.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CameraBounds : MonoBehaviour
{
    [Header("Bounds for this room")]
    public float minX = -10f;
    public float maxX =  10f;
    public float minY =  -5f;
    public float maxY =   5f;

    [Header("Axes to clamp")]
    public bool clampX = true;
    public bool clampY = false;

    private void Awake()
    {
        // Make sure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraFollow.Instance?.ApplyBounds(this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.25f);
        Vector3 centre = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size   = new Vector3(maxX - minX, maxY - minY, 1f);
        Gizmos.DrawCube(centre, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(centre, size);
    }
#endif
}

