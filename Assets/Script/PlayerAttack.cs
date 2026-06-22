using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;

    public float attackCooldown = 0.6f;
    private float cooldownTimer = 0f;

    public InputActionReference attackAction;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackRadius = 0.75f;
    public int attackDamage = 1;
    public float criticalChance = 0f;
    public LayerMask enemyLayers = ~0;
    public string idleAnimation = "Idle";

    [Header("Spin Attack")]
    public float spinDuration = 0.3f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (attackAction.action.triggered && cooldownTimer <= 0f)
        {
            string attackAnim = Random.value < 0.5f ? "Attack01" : "Attack02";
            animator.Play(attackAnim);
            DealDamage();
            StartCoroutine(ReturnToIdle());
            StartCoroutine(SpinAttack());
            cooldownTimer = attackCooldown;
        }
    }

    void OnEnable()
    {
        attackAction.action.Enable();
    }

    void OnDisable()
    {
        attackAction.action.Disable();
    }


    IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(attackCooldown);
        animator.Play(idleAnimation);
    }

    IEnumerator SpinAttack()
    {
        if (playerMovement != null)
            playerMovement.rotationLocked = true;

        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Lerp(0f, 360f, elapsed / spinDuration);
            transform.rotation = startRotation * Quaternion.Euler(0f, angle, 0f);
            yield return null;
        }

        transform.rotation = startRotation;

        if (playerMovement != null)
            playerMovement.rotationLocked = false;
    }

    void DealDamage()
    {
        Vector3 attackCenter = transform.position + transform.forward * attackRange;
        Collider[] hits = Physics.OverlapSphere(
            attackCenter,
            attackRadius,
            enemyLayers,
            QueryTriggerInteraction.Collide
        );

        HashSet<Damageable> damagedTargets = new HashSet<Damageable>();

        foreach (Collider hit in hits)
        {
            Damageable damageable = hit.GetComponentInParent<Damageable>();
            if (damageable == null || damagedTargets.Contains(damageable))
                continue;

            Vector3 knockDir = (hit.transform.position - transform.position).normalized;

            int finalDamage = attackDamage;
            bool isCritical = Random.value < criticalChance;

            if (isCritical)
            {
                finalDamage = (int)(attackDamage * 2);
                Debug.Log("CRITICAL HIT!");
            }

            damageable.TakeDamage(finalDamage, knockDir);
            damagedTargets.Add(damageable);
        }
    }
}