using UnityEngine;

/// <summary>
/// 掛在 Q 鍵 Slash 技能預製物上。
/// 生成時自動對範圍內敵人造成傷害。
/// </summary>
public class SkillAreaDamage : SkillBase
{
    public override UpgradeType SkillType => UpgradeType.SkillSlash;

    [Header("Area Settings")]
    [SerializeField] private float damageRadius = 2.5f;
    [SerializeField] private LayerMask enemyLayers = ~0;
    [SerializeField] private bool damageOnStart = true;

    void Start()
    {
        if (damageOnStart)
            DealAreaDamage();
    }

    public void DealAreaDamage()
    {
        int finalDamage = GetDamage();
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, enemyLayers);

        foreach (Collider hit in hits)
        {
            Damageable target = hit.GetComponentInParent<Damageable>();
            if (target != null && !target.isDead)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                target.TakeDamage(finalDamage, dir);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
