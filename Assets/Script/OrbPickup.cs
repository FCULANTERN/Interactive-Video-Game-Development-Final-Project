using UnityEngine;

public class OrbPickup : MonoBehaviour
{
    public enum OrbType { Health, Mana }

    [Header("Orb")]
    public OrbType type = OrbType.Health;
    public float amount = 20f;

    [Header("Pickup FX")]
    public GameObject pickupEffectPrefab;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (HealthSystem.Instance != null)
        {
            if (type == OrbType.Health)
                HealthSystem.Instance.HealDamage(amount);
            else
                HealthSystem.Instance.RestoreMana(amount);
        }

        if (pickupEffectPrefab != null)
        {
            GameObject fx = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        Destroy(gameObject);
    }
}
