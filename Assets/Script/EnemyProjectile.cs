using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 12f;
    public int damage = 1;
    public float lifetime = 5f;

    [Header("Hit VFX")]
    public GameObject hitVFX;
    public float vfxScale = 1f;

    private Vector3 direction;
    private bool launched;

    public void Launch(Vector3 dir, int dmg, float spd)
    {
        direction = dir.normalized;
        damage = dmg;
        speed = spd;
        launched = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!launched) return;
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph == null) ph = other.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
            HitAndDestroy();
            return;
        }

        if (other.isTrigger) return;
        if (other.GetComponentInParent<Damageable>() != null) return;

        HitAndDestroy();
    }

    void HitAndDestroy()
    {
        if (hitVFX != null)
        {
            GameObject fx = Instantiate(hitVFX, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * vfxScale;
            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            float dur = (ps != null) ? ps.main.duration + ps.main.startLifetime.constantMax : 2f;
            Destroy(fx, dur);
        }
        Destroy(gameObject);
    }
}
