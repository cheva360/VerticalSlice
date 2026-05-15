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
    [SerializeField] private Image BackingImage;


    [Header("Stamina Settings")]
    [Tooltip("Maximum stamina value.")]
    public float maxStamina = 100f;

    [Tooltip("How many units of velocity are considered 'fast' for max drain rate.")]
    public float velocityDrainScale = 5f;

    [Tooltip("Max stamina drain per second (at full speed).")]
    public float maxDrainRate = 20f;

    [Tooltip("Velocity below this threshold counts as 'not moving' and won't drain stamina.")]
    public float velocityDeadzone = 0.1f;

    [Tooltip("If the player's Y velocity is below this (negative) value, stamina will not drain (e.g. -3 means falling faster than -3 stops drain).")]
    public float fallDrainCutoff = -3f;

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

    [Header("Visibility Fade")]
    [Tooltip("Seconds to fade OUT when stamina reaches max.")]
    public float fadeOutDuration = 1f;

    [Tooltip("Seconds to fade IN when stamina is not at max.")]
    public float fadeInDuration = 0.5f;

    [Tooltip("Easing curve for fade in/out (X = normalized time, Y = alpha).")]
    public AnimationCurve fadeCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f));

    // ── Runtime state ──────────────────────────────────────────────────────────
    private float currentStamina;
    private float trailStamina;
    private float noGrabTimer = 0f;
    private bool isDepleted = false;
    private bool waitingForGrabAfterDepletion = false; // blocks drain until first grab after full depletion

    // Fade state: 0 = fully hidden, 1 = fully visible
    private float fadeProgress = 1f;

    // Replenish-over-time state
    private bool isReplenishing = false;
    private float replenishStart    = 0f;   // stamina value when replenish began
    private float replenishTarget   = 0f;   // stamina value to reach
    private float replenishProgress = 0f;   // normalized [0, 1]

    private void Start()
    {
        currentStamina = maxStamina;
        trailStamina   = maxStamina;
        fadeProgress   = 0f;
        UpdateUI(false);
    }

    private void Update()
    {
        if (visualScriptingTarget == null || playerRb == null) return;

        bool rGrab      = GetVSBool("rGrab");
        bool lGrab      = GetVSBool("lGrab");
        bool isGrabbing = rGrab || lGrab;

        // ── Forced replenish overrides drain ──────────────────────────────────
        if (isReplenishing)
        {
            // Advance normalized progress at recoveryRate*2 per second,
            // mapped over the total replenish range so the speed is consistent.
            float range = replenishTarget - replenishStart;
            float progressPerSecond = (range > 0f) ? (recoveryRate * 2f) / range : 1f;
            replenishProgress += progressPerSecond * Time.deltaTime;
            replenishProgress  = Mathf.Clamp01(replenishProgress);

            // x*x*x ease-out curve: fast start, decelerates into target
            float inv = 1f - replenishProgress;
            float t = 1f - (inv * inv * inv);
            currentStamina = Mathf.Lerp(replenishStart, replenishTarget, t);

            if (replenishProgress >= 1f)
            {
                currentStamina    = replenishTarget;
                isReplenishing    = false;
                replenishProgress = 0f;
            }

            // Clear depletion if we've recovered enough
            if (isDepleted && currentStamina >= maxStamina)
            {
                isDepleted   = false;
                trailStamina = maxStamina;
                SetVSBool("IsStaminaDepleted", false);
            }

            // Skip normal drain/recovery this frame
            goto SkipDrainRecovery;
        }

        // ── Normal drain / recovery ───────────────────────────────────────────
        if (isGrabbing)
        {
            // Reset the no-grab timer while grabbing
            noGrabTimer = 0f;

            // Player grabbed — unlock drain if we were waiting after a depletion recovery
            if (waitingForGrabAfterDepletion)
                waitingForGrabAfterDepletion = false;
        }
        else
        {
            noGrabTimer += Time.deltaTime;
        }

        float currentSpeed = playerRb.velocity.magnitude;
        float yVelocity    = playerRb.velocity.y;

        if (!isDepleted)
        {
            if (!waitingForGrabAfterDepletion && currentSpeed > velocityDeadzone
                && yVelocity >= fallDrainCutoff)
            {
                float drainFactor = Mathf.Clamp01(currentSpeed / velocityDrainScale);
                float drain = drainFactor * maxDrainRate * Time.deltaTime;

                if (rGrab && lGrab)
                    drain *= bothGrabsMultiplier;

                currentStamina -= drain;

                if (currentStamina <= 0f)
                {
                    currentStamina = 0f;
                    isDepleted = true;
                    SetVSBool("IsStaminaDepleted", true);
                }
            }
            else if (!waitingForGrabAfterDepletion && noGrabTimer >= recoveryDelay
                     && currentStamina < maxStamina && currentSpeed <= velocityDeadzone)
            {
                // Recover stamina when not grabbing, after delay, and only when nearly still
                currentStamina += recoveryRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }
        else
        {
            // Only recover from depletion if the player is below the velocity threshold
            if (noGrabTimer >= recoveryDelay && currentSpeed <= velocityDeadzone)
            {
                currentStamina += recoveryRate * Time.deltaTime;

                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                    isDepleted     = false;
                    trailStamina   = maxStamina;
                    waitingForGrabAfterDepletion = true; // block drain until next grab
                    SetVSBool("IsStaminaDepleted", false);
                }
            }
        }

        SkipDrainRecovery:

        bool isRecovering = !isGrabbing && currentStamina < maxStamina;

        if (!isDepleted)
        {
            bool isActivelyRecovering = !isGrabbing && currentStamina < maxStamina && noGrabTimer >= recoveryDelay && currentStamina > trailStamina;

            if (isActivelyRecovering || isReplenishing)
            {
                trailStamina = currentStamina;
            }
            else
            {
                trailStamina = Mathf.MoveTowards(trailStamina, currentStamina, trailDecaySpeed * Time.deltaTime);
                trailStamina = Mathf.Min(trailStamina, currentStamina + maxTrailLag);
            }
        }

        // ── Fade logic ────────────────────────────────────────────────────────
        bool isAtMax = Mathf.Approximately(currentStamina, maxStamina);

        if (isAtMax)
            fadeProgress -= Time.deltaTime / fadeOutDuration;
        else
            fadeProgress += Time.deltaTime / fadeInDuration;

        fadeProgress = Mathf.Clamp01(fadeProgress);

        UpdateUI(isRecovering);
    }

    private void UpdateUI(bool isRecovering)
    {
        bool depletedRecovering = isDepleted && isRecovering;

        float alpha = fadeCurve.Evaluate(fadeProgress);

        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = currentStamina / maxStamina;

            Color c;
            if (depletedRecovering)
            {
                float t = Mathf.Abs(Mathf.Sin(Time.time * flashFrequency * Mathf.PI));
                Color orange = new Color(1f, 0.5f, 0f, 1f);
                c = Color.Lerp(recoveryColor, orange, t);
            }
            else
            {
                c = barColor;
            }

            c.a = alpha;
            staminaFillImage.color = c;
        }

        if (staminaTrailImage != null)
        {
            trailStamina = Mathf.Max(trailStamina, 0f);
            staminaTrailImage.fillAmount = trailStamina / maxStamina;

            Color c;
            if (depletedRecovering)
            {
                float t = Mathf.Abs(Mathf.Sin(Time.time * flashFrequency * Mathf.PI));
                Color orange = new Color(1f, 0.5f, 0f, 1f);
                c = Color.Lerp(recoveryColor, orange, t);
            }
            else
            {
                c = trailColor;
            }

            c.a = alpha;
            staminaTrailImage.color = c;
        }

        if (BackingImage != null)
        {
            Color c = BackingImage.color;
            c.a = alpha;
            BackingImage.color = c;
        }
    }

    /// <summary>
    /// Restores the given amount of stamina over time at recoveryRate*2 per second
    /// using a cubic (x³) ease-in curve, overriding any active drain.
    /// </summary>
    public void ReplenishStamina(float amount)
    {
        float target = Mathf.Min(currentStamina + amount, maxStamina);

        if (isReplenishing)
        {
            // Extend the target if a replenish is already in progress,
            // restarting progress from the current stamina.
            replenishStart    = currentStamina;
            replenishTarget   = Mathf.Max(replenishTarget, target);
            replenishProgress = 0f;
        }
        else
        {
            isReplenishing    = true;
            replenishStart    = currentStamina;
            replenishTarget   = target;
            replenishProgress = 0f;
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