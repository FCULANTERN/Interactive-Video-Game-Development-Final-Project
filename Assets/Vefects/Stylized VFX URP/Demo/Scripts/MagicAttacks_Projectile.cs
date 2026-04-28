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
        // �����ۤv
        if (col.CompareTag("Player"))
            return;

        // �p�G��観��q�}���A�N����
        Damageable damageable = col.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        // �ͦ��R���S��
        if (FX_Hit != null)
        {
            Instantiate(FX_Hit, col.transform.position, Quaternion.identity);
        }

        // ����S�ĩM����
        if (FX_Projectile != null)
            Destroy(FX_Projectile);

        if (FX_ProjectileTail != null)
            FX_ProjectileTail.Stop();

        if (SFX_Projectile != null)
            SFX_Projectile.Stop();

        Destroy(gameObject, 0.05f);
    }
}
