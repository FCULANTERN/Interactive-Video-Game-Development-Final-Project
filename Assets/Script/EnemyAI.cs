using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Move")]
    public float moveSpeed = 2.5f;
    public float stopDistance = 1.5f;
    public float rotateSpeed = 8f;

    [Header("Attack")]
    public int attackDamage = 1;
    public float attackInterval = 1f;

    private Rigidbody rb;
    private float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        FindPlayer();
        attackTimer = attackInterval;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            FindPlayer();
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        if (distance > stopDistance)
        {
            Vector3 dir = toTarget.normalized;

            Vector3 newPosition = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dir);
                Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(newRotation);
            }

            attackTimer = attackInterval;
        }
        else
        {
            attackTimer -= Time.fixedDeltaTime;

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