using UnityEngine.VFX;
using UnityEngine;
using System.Collections.Generic;

public class MagicAttacks_Projectile : MonoBehaviour
{
    private Vector3 projectileDir;
    public GameObject FX_Hit;
    public int damage = 1;
    public float moveSpeed = 60f;

    // Sweep radius for continuous hit detection (prevents passing through enemies at close range / high speed)
    public float hitRadius = 0.5f;

    // 從哪個升級類型取得傷害（由 Magic_Manager 設定）
    [HideInInspector] public UpgradeType skillUpgradeType = UpgradeType.SkillProjectile;
    [HideInInspector] public bool useUpgradeSystem = false;

    // Freeze effect (set by Magic_Manager, only enabled for the ice spell)
    [HideInInspector] public bool applyFreeze = false;
    [HideInInspector] public float freezeDuration = 2f;

    // Area-of-effect radius (0 = single target, >0 = affects all enemies in range on hit)
    [HideInInspector] public float areaRadius = 0f;

    VisualEffect FX_Projectile;
    VisualEffect FX_ProjectileTail;
    AudioSource SFX_Projectile;
    private bool hasImpacted = false;
    private bool firstUpdate = true;

    private void Start()
    {
        FX_Projectile = gameObject.transform.GetChild(0).GetComponent<VisualEffect>();
        FX_ProjectileTail = gameObject.transform.GetChild(1).GetComponent<VisualEffect>();
        SFX_Projectile = gameObject.GetComponent<AudioSource>();

        // 從升級系統取得傷害
        if (useUpgradeSystem && UpgradeSystem.Instance != null)
            damage = UpgradeSystem.Instance.GetSkillDamage(skillUpgradeType);

        Destroy(gameObject, 5f);
    }

    public void Setup(Vector3 projectileDir)
    {
        this.projectileDir = projectileDir;
    }

    private void Update()
    {
        if (hasImpacted) return;

        // Point-blank case: an enemy may already be overlapping the spawn point, so OnTriggerEnter
        // never fires. Catch it explicitly on the first frame.
        if (firstUpdate)
        {
            firstUpdate = false;
            foreach (Collider c in Physics.OverlapSphere(transform.position, hitRadius, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!c.CompareTag("Player") && c.GetComponentInParent<Damageable>() != null)
                {
                    Impact(c, transform.position);
                    return;
                }
            }
        }

        float step = moveSpeed * Time.deltaTime;

        // Continuous collision: sweep a sphere along the movement so fast shots can't tunnel through.
        RaycastHit hit;
        if (Physics.SphereCast(transform.position - projectileDir * hitRadius, hitRadius,
                               projectileDir, out hit, step + hitRadius, ~0, QueryTriggerInteraction.Ignore)
            && !hit.collider.CompareTag("Player"))
        {
            transform.position = hit.point;
            Impact(hit.collider, hit.point);
            return;
        }

        transform.position += projectileDir * step;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
            return;

        Impact(col, transform.position);
    }

    // Resolve a hit: deal damage / freeze (single target or area), spawn the hit FX, then despawn.
    void Impact(Collider col, Vector3 point)
    {
        if (hasImpacted) return;
        hasImpacted = true;

        if (areaRadius > 0f)
        {
            HashSet<Damageable> affected = new HashSet<Damageable>();
            foreach (Collider c in Physics.OverlapSphere(point, areaRadius))
            {
                Damageable d = c.GetComponentInParent<Damageable>();
                if (d == null || affected.Contains(d)) continue;
                affected.Add(d);
                ApplyHit(d);
            }
        }
        else
        {
            Damageable d = col.GetComponentInParent<Damageable>();
            if (d != null)
                ApplyHit(d);
        }

        if (FX_Hit != null)
        {
            GameObject hitFX = Instantiate(FX_Hit, point, Quaternion.identity);
            Destroy(hitFX, 3f);
        }

        if (FX_Projectile != null)
            Destroy(FX_Projectile);

        if (FX_ProjectileTail != null)
            FX_ProjectileTail.Stop();

        if (SFX_Projectile != null)
            SFX_Projectile.Stop();

        Destroy(gameObject, 0.05f);
    }

    // Apply damage (and freeze) to a single enemy
    void ApplyHit(Damageable d)
    {
        d.TakeDamage(damage);
        if (applyFreeze)
            FrozenEffect.Apply(d.gameObject, freezeDuration);
    }
}
