using UnityEngine;

public class HipRotationLimiter : MonoBehaviour
{
    [SerializeField] private float minAngle = -25f;
    [SerializeField] private float maxAngle = 25f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Get current X rotation in the range (-180, 180]
        float currentX = NormalizeAngle(transform.eulerAngles.x);

        if (currentX < minAngle || currentX > maxAngle)
        {
            // Clamp the X rotation and preserve Y and Z
            float clampedX = Mathf.Clamp(currentX, minAngle, maxAngle);
            transform.eulerAngles = new Vector3(clampedX, transform.eulerAngles.y, transform.eulerAngles.z);

            // Kill angular velocity on X so the rigidbody doesn't fight the correction
            rb.angularVelocity = new Vector3(0f, rb.angularVelocity.y, rb.angularVelocity.z);
        }
    }

    // Converts a Unity angle (0-360) to a signed angle (-180 to 180)
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }
}
