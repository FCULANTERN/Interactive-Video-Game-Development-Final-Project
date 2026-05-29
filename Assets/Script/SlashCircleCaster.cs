using UnityEngine;
using UnityEngine.InputSystem;

public class SlashCircleCaster : MonoBehaviour
{
    [Header("References")]
    public GameObject slashCirclePrefab;
    public Transform spawnPoint;
    public SpellCooldown spellCooldownUI;

    [Header("Cast Settings")]
    public Key castKey = Key.O;
    public int cooldown = 1;
    public float destroyAfter = 3f;
    public float manaCost = 20f;

    [Header("Rotation Offset")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    private float cooldownTimer = 0f;

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[castKey].wasPressedThisFrame && cooldownTimer <= 0f)
        {
            if (HealthSystem.Instance != null && HealthSystem.Instance.manaPoint < manaCost)
            {
                Debug.Log("魔力不足，無法使用 Slash！");
                return;
            }

            CastSlashCircle();
            spellCooldownUI?.StartCooldown(cooldown);

            if (HealthSystem.Instance != null)
                HealthSystem.Instance.UseMana(manaCost);

            cooldownTimer = cooldown;
        }
    }

    void CastSlashCircle()
    {
        if (slashCirclePrefab == null)
        {
            Debug.LogWarning("SlashCircleCaster: slashCirclePrefab �S�����w�C");
            return;
        }

        Transform pointToUse = spawnPoint != null ? spawnPoint : transform;

        Quaternion finalRotation =
            pointToUse.rotation * Quaternion.Euler(rotationOffset);

        GameObject fx = Instantiate(
            slashCirclePrefab,
            pointToUse.position,
            finalRotation
        );

        if (destroyAfter > 0f)
        {
            Destroy(fx, destroyAfter);
        }
    }
}