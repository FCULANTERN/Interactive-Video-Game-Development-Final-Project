using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    public float attackCooldown = 0.6f;
    private float cooldownTimer = 0f;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackRadius = 0.75f;
    public int attackDamage = 1;
    public float criticalChance = 0f; // 0-1, represents 0-100%
    public LayerMask enemyLayers = ~0;
    public string idleAnimation = "Idle";

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
            string attackAnim = Random.value < 0.5f ? "Attack01" : "Attack02";
            animator.Play(attackAnim);
            DealDamage();
            StartCoroutine(ReturnToIdle());
            cooldownTimer = attackCooldown;
        }
    }

    IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(attackCooldown);
        animator.Play(idleAnimation);
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

            Vector3 knockDir = (hit.transform.position - transform.position).normalized;
            
            // Calculate damage with critical hit
            int finalDamage = attackDamage;
            bool isCritical = Random.value < criticalChance;
            if (isCritical)
            {
                finalDamage = (int)(attackDamage * 2); // 2x damage on critical
                Debug.Log("CRITICAL HIT!");
            }
            
            damageable.TakeDamage(finalDamage, knockDir);
            damagedTargets.Add(damageable);
        }
    }
}
