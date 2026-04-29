using UnityEngine;

public class DoubleCircularConstraint : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField] private Rigidbody handArb;
    [SerializeField] private Rigidbody handBrb;
    
    private Transform handA;
    private Transform handB;

    [Header("Settings")]
    public float radiusA = 0.75f;
    public float radiusB = 0.75f;

    [Tooltip("How strictly the velocity is killed when hitting the edge.")]
    public float frictionOnEdge = 1f;

    [Tooltip("Small buffer to prevent jittering (e.g., 0.01f)")]
    public float edgeTolerance = 0.01f;

    [SerializeField] private Rigidbody rb;

    void Awake()
    {
        // Create anchor transforms dynamically
        GameObject anchorA = new GameObject("HandA_Anchor");
        handA = anchorA.transform;
        handA.SetParent(transform);

        GameObject anchorB = new GameObject("HandB_Anchor");
        handB = anchorB.transform;
        handB.SetParent(transform);
    }

    public void ArmUpdate()
    {
        if (!handArb || !handBrb || !rb) return;

        handA.position = handArb.position + new Vector3(0.2f, -0.3f, 0);
        handB.position = handBrb.position - new Vector3(0.2f, 0.3f, 0);

        Vector3 currentPos = rb.position;
        Vector3 currentVelocity = rb.velocity;

        Vector3 posCorrectionA = Vector3.zero;
        Vector3 velCorrectionA = Vector3.zero;

        Vector3 posCorrectionB = Vector3.zero;
        Vector3 velCorrectionB = Vector3.zero;

        bool outsideA = CalculateCorrection(currentPos, currentVelocity, handA.position, radiusA, out posCorrectionA, out velCorrectionA);
        bool outsideB = CalculateCorrection(currentPos, currentVelocity, handB.position, radiusB, out posCorrectionB, out velCorrectionB);

        if (outsideA || outsideB)
        {
            Vector3 finalPos = currentPos + posCorrectionA + posCorrectionB;
            rb.MovePosition(new Vector3(finalPos.x, finalPos.y, currentPos.z));
            rb.velocity -= (velCorrectionA + velCorrectionB);
        }
    }

    public bool CalculateCorrection(Vector3 pos, Vector3 vel, Vector3 center, float radius, out Vector3 posCorr, out Vector3 velCorr)
    {
        posCorr = Vector3.zero;
        velCorr = Vector3.zero;

        Vector2 offset = new Vector2(pos.x - center.x, pos.y - center.y);
        float dist = offset.magnitude;

        if (dist > radius + edgeTolerance)
        {
            Vector2 normal = offset.normalized;
            Vector2 targetPos = (Vector2)center + (normal * radius);
            posCorr = new Vector3(targetPos.x - pos.x, targetPos.y - pos.y, 0);

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
