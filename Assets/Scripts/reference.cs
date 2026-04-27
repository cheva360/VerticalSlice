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

    [Tooltip("Strength of position correction (0-1). Lower values reduce jitter.")]
    [Range(0.1f, 1f)]
    public float correctionStrength = 0.8f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!handA || !handB) return;

        Vector3 currentPos = rb.position;
        Vector3 currentVelocity = rb.velocity;

        // Calculate corrections for both hands
        bool outsideA = CalculateCorrection(currentPos, currentVelocity, handA.position, radiusA, 
            out Vector3 posCorrectionA, out Vector3 velCorrectionA, out float violationA);

        bool outsideB = CalculateCorrection(currentPos, currentVelocity, handB.position, radiusB, 
            out Vector3 posCorrectionB, out Vector3 velCorrectionB, out float violationB);

        if (outsideA || outsideB)
        {
            // Apply position correction using velocity-based approach (smoother)
            Vector3 totalPosCorrection = Vector3.zero;

            if (outsideA && outsideB)
            {
                // When violating both constraints, prioritize the more violated one
                // or blend based on violation severity
                float totalViolation = violationA + violationB;
                float weightA = violationA / totalViolation;
                float weightB = violationB / totalViolation;
                
                totalPosCorrection = (posCorrectionA * weightA + posCorrectionB * weightB);
            }
            else if (outsideA)
            {
                totalPosCorrection = posCorrectionA;
            }
            else
            {
                totalPosCorrection = posCorrectionB;
            }

            // Apply correction via velocity instead of direct position for smoother movement
            Vector3 correctionVelocity = totalPosCorrection / Time.fixedDeltaTime;
            rb.velocity += correctionVelocity * correctionStrength;

            // Kill outward velocity
            rb.velocity -= (velCorrectionA + velCorrectionB);
        }
    }

    private bool CalculateCorrection(Vector3 pos, Vector3 vel, Vector3 center, float radius, 
        out Vector3 posCorr, out Vector3 velCorr, out float violation)
    {
        posCorr = Vector3.zero;
        velCorr = Vector3.zero;
        violation = 0f;

        // X-Y Distance check
        Vector2 offset = new Vector2(pos.x - center.x, pos.y - center.y);
        float dist = offset.magnitude;

        if (dist > radius)
        {
            violation = dist - radius; // How far outside the radius

            // 1. Position Correction
            Vector2 normal = offset.normalized;
            Vector2 targetPos = (Vector2)center + (normal * radius);
            posCorr = new Vector3(targetPos.x - pos.x, targetPos.y - pos.y, 0);

            // 2. Velocity Correction (Outward momentum)
            float outwardSpeed = Vector2.Dot(new Vector2(vel.x, vel.y), normal);

            if (outwardSpeed > 0)
            {
                velCorr = new Vector3(normal.x, normal.y, 0) * outwardSpeed * frictionOnEdge;
            }

            return true;
        }

        return false;
    }
}
