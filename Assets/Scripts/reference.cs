using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DoubleCircularConstraint : MonoBehaviour
{
    [Header("Anchors")]
    public Transform handA;
    public Transform handB;

    [Header("Settings")]
    public float radiusA = 0.5f;
    public float radiusB = 0.5f;

    [Tooltip("How strictly the velocity is killed when hitting the edge.")]
    public float frictionOnEdge = 1f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!handA || !handB) return;

        Vector3 currentPos = rb.position;
        Vector3 currentVelocity = rb.velocity; // Use .velocity in older Unity versions

        // We calculate corrections for both hands
        Vector3 posCorrectionA = Vector3.zero;
        Vector3 velCorrectionA = Vector3.zero;

        Vector3 posCorrectionB = Vector3.zero;
        Vector3 velCorrectionB = Vector3.zero;

        // Process Hand A
        bool outsideA = CalculateCorrection(currentPos, currentVelocity, handA.position, radiusA, out posCorrectionA, out velCorrectionA);

        // Process Hand B
        bool outsideB = CalculateCorrection(currentPos, currentVelocity, handB.position, radiusB, out posCorrectionB, out velCorrectionB);

        // Apply Position: MovePosition is better for interpolated physics
        if (outsideA || outsideB)
        {
            // Sum the corrections and apply to the Rigidbody
            Vector3 finalPos = currentPos + posCorrectionA + posCorrectionB;
            rb.MovePosition(new Vector3(finalPos.x, finalPos.y, currentPos.z));

            // Apply Velocity: Subtract the outward momentum for each violated constraint
            rb.velocity -= (velCorrectionA + velCorrectionB);
        }
    }

    private bool CalculateCorrection(Vector3 pos, Vector3 vel, Vector3 center, float radius, out Vector3 posCorr, out Vector3 velCorr)
    {
        posCorr = Vector3.zero;
        velCorr = Vector3.zero;

        // X-Y Distance check
        Vector2 offset = new Vector2(pos.x - center.x, pos.y - center.y);
        float dist = offset.magnitude;

        if (dist > radius)
        {
            // 1. Position Correction
            Vector2 normal = offset.normalized;
            Vector2 targetPos = (Vector2)center + (normal * radius);
            posCorr = new Vector3(targetPos.x - pos.x, targetPos.y - pos.y, 0);

            // 2. Velocity Correction (Outward momentum)
            // Dot product tells us how much we are moving 'away' from the center
            float outwardSpeed = Vector2.Dot(new Vector2(vel.x, vel.y), normal);

            if (outwardSpeed > 0)
            {
                // We return the amount of velocity to subtract
                velCorr = new Vector3(normal.x, normal.y, 0) * outwardSpeed * frictionOnEdge;
            }

            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!handA || !handB) return;

        Gizmos.color = Color.green;
        DrawCircle(handA.position, radiusA);

        Gizmos.color = Color.blue;
        DrawCircle(handB.position, radiusB);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        float segments = 30;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / segments) * Mathf.PI * 2;
            float angle2 = ((i + 1) / segments) * Mathf.PI * 2;
            Gizmos.DrawLine(
                center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0),
                center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0)
            );
        }
    }
}
