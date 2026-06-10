using UnityEngine;

public class ReplenishTrigger : MonoBehaviour
{
    [Tooltip("Amount of stamina to restore on contact.")]
    public float replenishAmount = 100f;
    [SerializeField] private bool hasBeenUsed = false;
    [SerializeField] private float fadeDuration = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenUsed)
        {
            hasBeenUsed = true;
            GameController.Instance.StaminaBar.ReplenishStamina(replenishAmount);
            Debug.Log($"Player stamina replenished by {replenishAmount}.");

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.SetFloat("_Duration", fadeDuration);
                rend.material.SetFloat("_IsFade", 1f);
                rend.material.SetFloat("_StartTime", Time.time);
                StartCoroutine(DisableAfterDelay(fadeDuration));
            }
        }
    }

    //disable object coroutine
    private System.Collections.IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        //disable renderer
        Renderer rend = GetComponent<Renderer>();
        rend.enabled = false;

        //wait for 5s then enable the object again
        yield return new WaitForSeconds(5f);
        hasBeenUsed = false;
        if (rend != null)
        {
            rend.enabled = true;
            rend.material.SetFloat("_IsFade", 0f);
            rend.material.SetFloat("_StartTime", 1f);
        }
    }
}