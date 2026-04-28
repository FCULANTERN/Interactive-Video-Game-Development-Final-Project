using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Move")]
    public float moveSpeed = 2.5f;
    public float stopDistance = 1.5f;

    [Header("Attack")]
    public int attackDamage = 1;
    public float attackInterval = 1f;

    private float attackTimer;

    void Start()
    {
        FindPlayer();
        attackTimer = attackInterval;
    }

    void Update()
    {
        if (target == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > stopDistance)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0f;

            transform.position += dir * moveSpeed * Time.deltaTime;

            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
        else
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                Attack();
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
        }
    }

    void Attack()
    {
        Debug.Log(name + " attacks player for " + attackDamage + " damage.");
    }
}