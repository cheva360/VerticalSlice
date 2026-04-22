using UnityEngine;

public class hand : MonoBehaviour
{
    [Header("Distance Constraint")]
    public Transform anchor;
    public float maxDistance = 3f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (anchor == null) return;

        Vector3 offset = transform.position - anchor.position;
        float distance = offset.magnitude;

        if (distance > maxDistance)
        {
            // Push the object back to the max distance boundary
            Vector3 constrainedPosition = anchor.position + offset.normalized * maxDistance;

            // Cancel any velocity going further away from anchor
            Vector3 radialVelocity = Vector3.Dot(rb.velocity, offset.normalized) * offset.normalized;
            if (Vector3.Dot(radialVelocity, offset) > 0)
            {
                rb.velocity -= radialVelocity;
            }

            rb.MovePosition(constrainedPosition);
        }
    }
}
