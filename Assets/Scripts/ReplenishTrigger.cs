using UnityEngine;

public class ReplenishTrigger : MonoBehaviour
{
    [Tooltip("Amount of stamina to restore on contact.")]
    public float replenishAmount = 100f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameController.Instance.StaminaBar.ReplenishStamina(replenishAmount);
            Destroy(gameObject);
        }
    }
}