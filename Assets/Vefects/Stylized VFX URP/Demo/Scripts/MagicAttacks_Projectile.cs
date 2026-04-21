using UnityEngine.VFX;
using UnityEngine;

public class MagicAttacks_Projectile : MonoBehaviour
{
    private Vector3 projectileDir;
    public GameObject FX_Hit;
    public int damage = 1;

    VisualEffect FX_Projectile;
    VisualEffect FX_ProjectileTail;
    AudioSource SFX_Projectile;

    private void Start()
    {
        FX_Projectile = gameObject.transform.GetChild(0).GetComponent<VisualEffect>();
        FX_ProjectileTail = gameObject.transform.GetChild(1).GetComponent<VisualEffect>();
        SFX_Projectile = gameObject.GetComponent<AudioSource>();
    }

    public void Setup(Vector3 projectileDir)
    {
        this.projectileDir = projectileDir;
    }

    private void Update()
    {
        float moveSpeed = 60f;
        transform.position += projectileDir * moveSpeed * Time.deltaTime;
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter(Collider col)
    {
        // 不打自己
        if (col.CompareTag("Player"))
            return;

        // 如果對方有血量腳本，就扣血
        Damageable damageable = col.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        // 生成命中特效
        if (FX_Hit != null)
        {
            Instantiate(FX_Hit, col.transform.position, Quaternion.identity);
        }

        // 停止特效和音效
        if (FX_Projectile != null)
            Destroy(FX_Projectile);

        if (FX_ProjectileTail != null)
            FX_ProjectileTail.Stop();

        if (SFX_Projectile != null)
            SFX_Projectile.Stop();

        Destroy(gameObject, 0.05f);
    }
}
