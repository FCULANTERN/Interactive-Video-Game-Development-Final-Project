using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private float attackCooldown = 0.6f;
    private float cooldownTimer = 0f;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackRadius = 0.75f;
    public int attackDamage = 1;
    public LayerMask enemyLayers = ~0;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            animator.Play("Attack01");
            DealDamage();
            cooldownTimer = attackCooldown;
        }
    }

    void DealDamage()
    {
        Vector3 attackCenter = transform.position + transform.forward * attackRange;
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRadius, enemyLayers, QueryTriggerInteraction.Collide);

        HashSet<Damageable> damagedTargets = new HashSet<Damageable>();

        foreach (Collider hit in hits)
        {
            Damageable damageable = hit.GetComponentInParent<Damageable>();
            if (damageable == null || damagedTargets.Contains(damageable))
                continue;

            damageable.TakeDamage(attackDamage);
            damagedTargets.Add(damageable);
        }
    }
}
