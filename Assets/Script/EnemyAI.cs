using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Move")]
    public float moveSpeed = 2.5f;
    public float stopDistance = 1.5f;

    [Header("Attack")]
    public int attackDamage = 1;
    public float attackInterval = 1f;

    private float attackTimer;

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            MoveToPlayer();
        }
        else
        {
            AttackPlayer();
        }
    }

    void MoveToPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;

        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void AttackPlayer()
    {
        attackTimer -= Time.deltaTime;

        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0f;

        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        if (attackTimer <= 0f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            attackTimer = attackInterval;
        }
    }
}