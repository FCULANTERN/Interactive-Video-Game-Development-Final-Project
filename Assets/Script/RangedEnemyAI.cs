using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RangedEnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Move")]
    public float moveSpeed = 2.5f;
    public float attackRange = 8f;
    public float rotateSpeed = 8f;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform muzzle;
    public int projectileDamage = 1;
    public float projectileSpeed = 12f;
    public float attackInterval = 1.5f;

    [Header("Aim Height (used when no muzzle is set)")]
    public float muzzleHeight = 0.6f;
    public float aimHeight = 0.5f;

    private Rigidbody rb;
    private float attackTimer;
    private float stunTimer;
    private PlayerHealth playerHealth;
    private EnemyMouthAnimator mouthAnimator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        FindPlayer();
        attackTimer = attackInterval;
        mouthAnimator = GetComponentInChildren<EnemyMouthAnimator>();
    }

    public void Stun(float duration)
    {
        stunTimer = duration;
    }

    void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        Damageable self = GetComponent<Damageable>();
        if (self != null && self.isDead)
        {
            enabled = false;
            return;
        }

        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            return;
        }

        if (target == null)
        {
            FindPlayer();
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        Vector3 dir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.forward;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));
        }

        if (distance > attackRange)
        {
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
            attackTimer = attackInterval;
        }
        else
        {
            attackTimer -= Time.fixedDeltaTime;
            if (attackTimer <= 0f)
            {
                Fire();
                attackTimer = attackInterval;
            }
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    void Fire()
    {
        if (projectilePrefab == null || target == null)
            return;

        Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position + Vector3.up * muzzleHeight;
        Vector3 aimDir = (target.position + Vector3.up * aimHeight - spawnPos).normalized;
        Quaternion rot = Quaternion.LookRotation(aimDir);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, rot);
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
            ep.Launch(aimDir, projectileDamage, projectileSpeed);

        if (mouthAnimator != null)
            mouthAnimator.PlayAttackAnimation();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
