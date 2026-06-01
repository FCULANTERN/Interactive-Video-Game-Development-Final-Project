using UnityEngine;
using UnityEngine.InputSystem;

public class Magic_Manager : MonoBehaviour
{
    [Header("Cast Settings")]
    public int castCooldown = 1;
    public InputActionReference castAction;
    public float manaCost = 10f;

    [Header("References")]
    public Transform spawnOffSet;
    public Transform target;
    public SpellCooldown spellCooldownUI;

    [Header("Rotation Offset")]
    public Vector3 castRotationOffset;
    public Vector3 projectileRotationOffset;

    [Header("Skill Upgrade Type")]
    public UpgradeType skillUpgradeType = UpgradeType.SkillProjectile;
    [Tooltip("這些 FX index 不受升級系統影響，使用 prefab 上原本的 damage。例如 Void Spell 在第 3 個位置（從 0 起）填 3")]
    public int[] upgradeIgnoredFXIndices;

    [Header("Freeze (Ice spell)")]
    public bool applyFreeze = false;
    public float freezeDuration = 2f;
    [Tooltip("每升一級增加的冰凍秒數（僅 applyFreeze 開啟時生效）")]
    public float freezeDurationPerLevel = 0.5f;

    [Header("Area Effect")]
    [Tooltip("Area-of-effect radius on hit (0 = only the directly hit enemy)")]
    public float areaRadius = 0f;

    [Header("Damage")]
    [Tooltip("Multiplier on top of the base/upgrade damage (1 = normal)")]
    public float damageMultiplier = 1f;

    [Header("FX Lists")]
    public GameObject[] FXList_Cast;
    public GameObject[] FXList_Projectile;
    public GameObject[] FXList_Hit;

    private float cooldownTimer = 0f;
    private int currentFXIndex = 0;
    private int nextFXIndex = 0;

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        HandleEffectSwitchInput();
        HandleCastInput();
    }

    void HandleCastInput()
    {
        if (Keyboard.current == null)
            return;

        if (castAction.action.triggered && cooldownTimer <= 0f)
        {

            if (HealthSystem.Instance == null)
                return;

            if (!HealthSystem.Instance.UseMana(manaCost))
            {
                Debug.Log("Mana insuffisant");
                return;
            }

            currentFXIndex = nextFXIndex;
            CastProjectile();
            cooldownTimer = castCooldown;
            spellCooldownUI?.StartCooldown(castCooldown);
        }
    }

    void HandleEffectSwitchInput()
    {
        if (Keyboard.current == null)
            return;

        int maxCount = GetMaxAvailableEffectCount();
        if (maxCount <= 0)
            return;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            nextFXIndex++;
            if (nextFXIndex >= maxCount)
                nextFXIndex = 0;
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            nextFXIndex--;
            if (nextFXIndex < 0)
                nextFXIndex = maxCount - 1;
        }
    }

    int GetMaxAvailableEffectCount()
    {
        int castCount = FXList_Cast != null ? FXList_Cast.Length : 0;
        int projectileCount = FXList_Projectile != null ? FXList_Projectile.Length : 0;
        int hitCount = FXList_Hit != null ? FXList_Hit.Length : 0;

        return Mathf.Max(castCount, projectileCount, hitCount);
    }

    void CastProjectile()
    {
        if (spawnOffSet == null)
        {
            Debug.LogWarning("Magic_Manager: spawnOffSet �S�����w�C");
            return;
        }

        Vector3 projectileDir = GetProjectileDirection();
        Quaternion baseRotation = Quaternion.LookRotation(projectileDir, Vector3.up);

        Quaternion castRotation = baseRotation * Quaternion.Euler(castRotationOffset);

        if (HasValidIndex(FXList_Cast, currentFXIndex))
        {
            GameObject castFX = Instantiate(
                FXList_Cast[currentFXIndex],
                spawnOffSet.position,
                castRotation
            );
            Destroy(castFX, 3f);
        }

        ShootProjectile();
    }

    void ShootProjectile()
    {
        if (spawnOffSet == null)
        {
            Debug.LogWarning("Magic_Manager: spawnOffSet �S�����w�C");
            return;
        }

        if (!HasValidIndex(FXList_Projectile, currentFXIndex))
        {
            Debug.LogWarning("Magic_Manager: FXList_Projectile �S���������ު���g���C");
            return;
        }

        Vector3 projectileDir = GetProjectileDirection();
        Quaternion baseRotation = Quaternion.LookRotation(projectileDir, Vector3.up);
        Quaternion projectileRotation = baseRotation * Quaternion.Euler(projectileRotationOffset);

        GameObject projectile = Instantiate(
            FXList_Projectile[currentFXIndex],
            spawnOffSet.position,
            projectileRotation
        );

        MagicAttacks_Projectile projectileScript = projectile.GetComponent<MagicAttacks_Projectile>();
        if (projectileScript != null)
        {
            bool ignoreUpgrade = false;
            if (upgradeIgnoredFXIndices != null)
            {
                foreach (int idx in upgradeIgnoredFXIndices)
                    if (idx == currentFXIndex) { ignoreUpgrade = true; break; }
            }

            projectileScript.skillUpgradeType = skillUpgradeType;
            // 冰凍技能不使用升級傷害（升級只影響冰凍時間），保留 prefab 上的固定 damage
            projectileScript.useUpgradeSystem = !ignoreUpgrade && !applyFreeze;
            projectileScript.applyFreeze = applyFreeze;

            // 冰凍時間隨升級等級提升
            float scaledFreeze = freezeDuration;
            if (applyFreeze && UpgradeSystem.Instance != null)
            {
                var upgradeData = UpgradeSystem.Instance.GetData(skillUpgradeType);
                if (upgradeData != null)
                    scaledFreeze += (upgradeData.currentLevel - 1) * freezeDurationPerLevel;
            }
            projectileScript.freezeDuration = scaledFreeze;
            projectileScript.areaRadius = areaRadius;
            projectileScript.damageMultiplier = damageMultiplier;
            projectileScript.Setup(projectileDir);

            if (HasValidIndex(FXList_Hit, currentFXIndex))
            {
                projectileScript.FX_Hit = FXList_Hit[currentFXIndex];
            }
        }

        Destroy(projectile, 4f);
    }

    Vector3 GetProjectileDirection()
    {
        Vector3 dir;

        if (target != null)
            dir = (target.position - spawnOffSet.position).normalized;
        else
            dir = spawnOffSet.forward;

        if (dir.sqrMagnitude < 0.0001f)
            dir = spawnOffSet.forward;

        return dir;
    }

    bool HasValidIndex(GameObject[] array, int index)
    {
        return array != null && index >= 0 && index < array.Length && array[index] != null;
    }

    void OnEnable()
    {
        if (castAction != null && castAction.action != null)
            castAction.action.Enable();
    }

    void OnDisable()
    {
        if (castAction != null && castAction.action != null)
            castAction.action.Disable();
    }
}