using UnityEngine;
using UnityEngine.InputSystem;

public class SlashCircleCaster : MonoBehaviour
{
    [Header("References")]
    public GameObject slashCirclePrefab;
    public Transform spawnPoint;
    public SpellCooldown spellCooldownUI;

    [Header("Cast Settings")]
    public InputActionReference castAction;
    public int cooldown = 1;
    public float destroyAfter = 3f;
    public float manaCost = 20f;


    [Header("Rotation Offset")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    private float cooldownTimer = 0f;
    private bool isCasting = false;


    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                isCasting = false;
            }
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

    void OnEnable()
    {
        castAction.action.Enable();
        castAction.action.started += OnCast;
    }

    void OnDisable()
    {
        castAction.action.started -= OnCast;
        castAction.action.Disable();
    }

    void OnCast(InputAction.CallbackContext ctx)
    {
        if (isCasting) return;

        if (HealthSystem.Instance == null) return;

        if (!HealthSystem.Instance.UseMana(manaCost)) return;

        isCasting = true;
        cooldownTimer = cooldown;

        CastSlashCircle();
        spellCooldownUI?.StartCooldown(cooldown);
    }
}