using UnityEngine;
using UnityEngine.InputSystem;

public class Magic_Manager : MonoBehaviour
{
    [Header("Cast Settings")]
    public float castCooldown = 1f;
    public float castDelayBeforeProjectile = 0.25f;
    public Key castKey = Key.J;

    [Header("References")]
    public Transform spawnOffSet;
    public Transform target;

    [Header("FX Lists")]
    public GameObject[] FXList_Cast;
    public GameObject[] FXList_Projectile;
    public GameObject[] FXList_Hit;

    private float cooldownTimer = 0f;
    private float projectileDelayTimer = 0f;
    private bool isCasting = false;

    private int currentFXIndex = 0;
    private int nextFXIndex = 0;

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        HandleEffectSwitchInput();
        HandleCastInput();

        if (isCasting)
        {
            ShootProjectile();
        }
    }

    void HandleCastInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[castKey].wasPressedThisFrame && cooldownTimer <= 0f && !isCasting)
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
            Debug.LogWarning("Magic_Manager: spawnOffSet �S�����w�C");
            return;
        }

        if (HasValidIndex(FXList_Cast, currentFXIndex))
        {
            GameObject castFX = Instantiate(
                FXList_Cast[currentFXIndex],
                spawnOffSet.position,
                spawnOffSet.rotation
            );
            Destroy(castFX, 3f);
        }

        projectileDelayTimer = castDelayBeforeProjectile;
        isCasting = true;
    }

    void ShootProjectile()
    {
        projectileDelayTimer -= Time.deltaTime;

        if (projectileDelayTimer > 0f)
            return;

        if (spawnOffSet == null)
        {
            Debug.LogWarning("Magic_Manager: spawnOffSet �S�����w�C");
            isCasting = false;
            return;
        }

        if (!HasValidIndex(FXList_Projectile, currentFXIndex))
        {
            Debug.LogWarning("Magic_Manager: FXList_Projectile �S���������ު���g���C");
            isCasting = false;
            return;
        }

        GameObject projectile = Instantiate(
            FXList_Projectile[currentFXIndex],
            spawnOffSet.position,
            spawnOffSet.rotation
        );

        Vector3 projectileDir;
        if (target != null)
            projectileDir = (target.position - spawnOffSet.position).normalized;
        else
            projectileDir = spawnOffSet.forward;

        MagicAttacks_Projectile projectileScript = projectile.GetComponent<MagicAttacks_Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Setup(projectileDir);

            if (HasValidIndex(FXList_Hit, currentFXIndex))
            {
                projectileScript.FX_Hit = FXList_Hit[currentFXIndex];
            }
            else
            {
                Debug.LogWarning("Magic_Manager: FXList_Hit �S���������ު��R���S�ġC");
            }
        }
        else
        {
            Debug.LogWarning("Magic_Manager: ��g�� prefab �W�䤣�� MagicAttacks_Projectile�C");
        }

        Destroy(projectile, 4f);
        isCasting = false;
    }

    bool HasValidIndex(GameObject[] array, int index)
    {
        return array != null && index >= 0 && index < array.Length && array[index] != null;
    }
}