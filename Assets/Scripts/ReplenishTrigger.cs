using UnityEngine;

public class ReplenishTrigger : MonoBehaviour
{
    [Tooltip("Amount of stamina to restore on contact.")]
    public float replenishAmount = 100f;
    [SerializeField] private bool hasBeenUsed = false;

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
                rend.material.SetFloat("_IsFade", 1f);

                rend.material.SetFloat("_StartTime", Time.time);
            }
            //wait for 0.5fs then destroy
            Destroy(gameObject, 0.5f);
        }
    }
}