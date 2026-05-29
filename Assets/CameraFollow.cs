using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public enum LerpCurve
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Exponential
    }

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Lerp Settings")]
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private LerpCurve curveType = LerpCurve.EaseOut;

    [Header("Offset")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector2 desiredXY = (Vector2)target.position + offset;
        float t = ApplyCurve(Time.deltaTime * lerpSpeed);
        Vector2 newXY = Vector2.Lerp(transform.position, desiredXY, t);

        transform.position = new Vector3(newXY.x, newXY.y, transform.position.z);
    }

    private float ApplyCurve(float t)
    {
        t = Mathf.Clamp01(t);
        switch (curveType)
        {
            case LerpCurve.Linear:      return t;
            case LerpCurve.EaseIn:      return t * t;
            case LerpCurve.EaseOut:     return 1f - (1f - t) * (1f - t);
            case LerpCurve.EaseInOut:   return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            case LerpCurve.Exponential: return t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
            default:                    return t;
        }
    }
}
