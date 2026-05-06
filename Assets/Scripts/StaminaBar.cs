using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StaminaBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Rigidbody of the player to read velocity from.")]
    [SerializeField] private Rigidbody playerRb;

    [Tooltip("The GameObject that holds the Visual Scripting variables (rgrab, lgrab, IsStaminaDepleted).")]
    [SerializeField] private GameObject visualScriptingTarget;

    [Tooltip("The circular filled Image used as the stamina bar (always green).")]
    [SerializeField] private Image staminaFillImage;

    [Tooltip("A second radial Image placed BEHIND the green fill — shows the red drain trail.")]
    [SerializeField] private Image staminaTrailImage;

    [Header("Stamina Settings")]
    [Tooltip("Maximum stamina value.")]
    public float maxStamina = 100f;

    [Tooltip("How many units of velocity are considered 'fast' for max drain rate.")]
    public float velocityDrainScale = 5f;

    [Tooltip("Max stamina drain per second (at full speed).")]
    public float maxDrainRate = 20f;

    [Tooltip("Velocity below this threshold counts as 'not moving' and won't drain stamina.")]
    public float velocityDeadzone = 0.1f;

    [Tooltip("How fast stamina recovers per second when not grabbing.")]
    public float recoveryRate = 15f;

    [Tooltip("How many seconds of not grabbing before recovery begins.")]
    public float recoveryDelay = 3f;

    [Header("Trail Settings")]
    [Tooltip("How quickly the red trail catches up to the green bar (higher = faster fade).")]
    public float trailDecaySpeed = 4f;

    [Tooltip("Color of the red drain trail.")]
    public Color trailColor = new Color(0.85f, 0.1f, 0.1f, 1f);

    [Tooltip("Color of the stamina bar (always this color).")]
    public Color barColor = new Color(0.3f, 1f, 0.4f, 1f);

    [Tooltip("Maximum stamina units the trail can lag behind the front bar.")]
    public float maxTrailLag = 25f;

    [Tooltip("Drain rate multiplier when both hands are grabbing simultaneously.")]
    public float bothGrabsMultiplier = 2f;

    [Header("Recovery Flash")]
    [Tooltip("Color of the bar while recovering (flashes between this and transparent).")]
    public Color recoveryColor = new Color(0.85f, 0.1f, 0.1f, 1f);

    [Tooltip("How many times per second the bar flashes during recovery.")]
    public float flashFrequency = 3f;

    // ── Runtime state ──────────────────────────────────────────────────────────
    private float currentStamina;
    private float trailStamina;      // lags behind currentStamina to show the red trail
    private float noGrabTimer = 0f;
    private bool isDepleted = false;

    private void Start()
    {
        currentStamina = maxStamina;
        trailStamina   = maxStamina;
        UpdateUI(false);
    }

    private void Update()
    {
        if (visualScriptingTarget == null || playerRb == null) return;

        bool rGrab      = GetVSBool("rGrab");
        bool lGrab      = GetVSBool("lGrab");
        bool isGrabbing = rGrab || lGrab;

        // Determine if we are in a "recovering" state (not grabbing and stamina not full)
        bool isRecovering = !isGrabbing && currentStamina < maxStamina;

        if (isGrabbing)
        {
            noGrabTimer = 0f;

            if (!isDepleted)
            {
                float speed = playerRb.velocity.magnitude;

                if (speed > velocityDeadzone)
                {
                    float drainFactor = Mathf.Clamp01(speed / velocityDrainScale);
                    float drain       = drainFactor * maxDrainRate * Time.deltaTime;

                    // Both hands grabbing = faster depletion
                    if (rGrab && lGrab)
                        drain *= bothGrabsMultiplier;

                    currentStamina -= drain;

                    if (currentStamina <= 0f)
                    {
                        currentStamina = 0f;
                        isDepleted     = true;
                        SetVSBool("IsStaminaDepleted", true);
                    }
                }
            }
        }
        else
        {
            noGrabTimer += Time.deltaTime;

            if (noGrabTimer >= recoveryDelay)
            {
                currentStamina += recoveryRate * Time.deltaTime;

                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;

                    if (isDepleted)
                    {
                        isDepleted    = false;
                        trailStamina  = maxStamina;   // ← reset trail to full
                        SetVSBool("IsStaminaDepleted", false);
                    }
                }
            }
        }

        // Freeze trail when depleted; let it move normally otherwise
        if (!isDepleted)
        {
            bool isActivelyRecovering = !isGrabbing && currentStamina < maxStamina && noGrabTimer >= recoveryDelay && currentStamina > trailStamina;

            if (isActivelyRecovering)
            {
                // Snap trail to fill only while stamina is actively increasing
                trailStamina = currentStamina;
            }
            else
            {
                // Lag behind during draining, or while waiting for recovery delay
                trailStamina = Mathf.MoveTowards(trailStamina, currentStamina, trailDecaySpeed * Time.deltaTime);
                trailStamina = Mathf.Min(trailStamina, currentStamina + maxTrailLag);
            }
        }

        UpdateUI(isRecovering);
    }

    private void UpdateUI(bool isRecovering)
    {
        // Flash only when stamina was fully depleted AND is now recovering
        bool depletedRecovering = isDepleted && isRecovering;

        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = currentStamina / maxStamina;

            if (depletedRecovering)
            {
                float t = Mathf.Abs(Mathf.Sin(Time.time * flashFrequency * Mathf.PI));
                Color orange = new Color(1f, 0.5f, 0f, 1f);
                staminaFillImage.color = Color.Lerp(recoveryColor, orange, t);
            }
            else
            {
                staminaFillImage.color = barColor;
            }
        }

        if (staminaTrailImage != null)
        {
            trailStamina = Mathf.Max(trailStamina, 0f);
            staminaTrailImage.fillAmount = trailStamina / maxStamina;

            if (depletedRecovering)
            {
                float t = Mathf.Abs(Mathf.Sin(Time.time * flashFrequency * Mathf.PI));
                Color orange = new Color(1f, 0.5f, 0f, 1f);
                staminaTrailImage.color = Color.Lerp(recoveryColor, orange, t);
            }
            else
            {
                staminaTrailImage.color = trailColor;
            }
        }
    }

    /// <summary>Call this to instantly restore stamina by the given amount.</summary>
    public void ReplenishStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);

        if (isDepleted && currentStamina >= maxStamina)
        {
            isDepleted   = false;
            trailStamina = maxStamina;
            SetVSBool("IsStaminaDepleted", false);
        }
    }

    // ── Visual Scripting helpers ───────────────────────────────────────────────

    private bool GetVSBool(string variableName)
    {
        try   { return Variables.Object(visualScriptingTarget).Get<bool>(variableName); }
        catch { return false; }
    }

    private void SetVSBool(string variableName, bool value)
    {
        try   { Variables.Object(visualScriptingTarget).Set(variableName, value); }
        catch (System.Exception e)
        { Debug.LogWarning($"[StaminaBar] Could not set VS variable '{variableName}': {e.Message}"); }
    }
}