using UnityEngine;

public class OrbPickup : MonoBehaviour
{
    [Header("Orb")]
    public float amount = 20f;

    [Header("Pickup FX")]
    public GameObject pickupEffectPrefab;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (HealthSystem.Instance != null)
            HealthSystem.Instance.HealDamage(amount);

        if (pickupEffectPrefab != null)
        {
            GameObject fx = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        Destroy(gameObject);
    }
}
