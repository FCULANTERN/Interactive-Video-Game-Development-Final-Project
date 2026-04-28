using UnityEngine;
using UnityEngine.InputSystem;

public class Magic_Manager : MonoBehaviour
{
    [Header("Cast Settings")]
    public float castCooldown = 1f;
    public Key castKey = Key.J;

    [Header("References")]
    public Transform spawnOffSet;
    public Transform target;

    [Header("Rotation Offset")]
    public Vector3 castRotationOffset;
    public Vector3 projectileRotationOffset;

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

        if (Keyboard.current[castKey].wasPressedThisFrame && cooldownTimer <= 0f)
        {
            currentFXIndex = nextFXIndex;
            CastProjectile();
            cooldownTimer = castCooldown;
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
            Debug.LogWarning("Magic_Manager: spawnOffSet 沒有指定。");
            return;
        }

        Vector3 projectileDir = GetProjectileDirection();
        Quaternion baseRotation = Quaternion.LookRotation(projectileDir, Vector3.up);

        Quaternion castRotation = baseRotation * Quaternion.Euler(castRotationOffset);

        if (HasValidIndex(FXList_Cast, currentFXIndex))
        {
            Instantiate(
                FXList_Cast[currentFXIndex],
                spawnOffSet.position,
                castRotation
            );
        }

        ShootProjectile();
    }

    void ShootProjectile()
    {
        if (spawnOffSet == null)
        {
            Debug.LogWarning("Magic_Manager: spawnOffSet 沒有指定。");
            return;
        }

        if (!HasValidIndex(FXList_Projectile, currentFXIndex))
        {
            Debug.LogWarning("Magic_Manager: FXList_Projectile 沒有對應索引的投射物。");
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
}