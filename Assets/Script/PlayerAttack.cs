using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private float attackCooldown = 0.6f;
    private float cooldownTimer = 0f;

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
            cooldownTimer = attackCooldown;
        }
    }
}
