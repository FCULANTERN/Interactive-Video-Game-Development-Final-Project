using UnityEngine;

/// <summary>
/// 掛在玩家身上，監聽 UpgradeSystem 事件並將升級效果套用到各組件。
/// </summary>
public class PlayerUpgradable : MonoBehaviour
{
    // 基礎數值（升級前的原始值）
    private float baseMoveSpeed;
    private float baseHPRegen;

    private PlayerAttack playerAttack;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerAttack   = GetComponent<PlayerAttack>();
        playerMovement = GetComponent<PlayerMovement>();

        // 儲存基礎值
        baseMoveSpeed  = playerMovement != null ? playerMovement.moveSpeed : 6f;
        baseHPRegen    = HealthSystem.Instance != null ? HealthSystem.Instance.hpRegenBonus : 0f;

        if (UpgradeSystem.Instance == null)
        {
            Debug.LogError("PlayerUpgradable: 場景中找不到 UpgradeSystem！");
            return;
        }

        UpgradeSystem.Instance.OnUpgradeChanged += ApplyUpgrade;

        // 套用所有初始數值
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
            ApplyUpgrade(type);
    }

    void ApplyUpgrade(UpgradeType type)
    {
        if (UpgradeSystem.Instance == null) return;
        float value = UpgradeSystem.Instance.GetValue(type);

        switch (type)
        {
            case UpgradeType.AttackDamage:
                if (playerAttack != null)
                    playerAttack.attackDamage = (int)value;
                break;

            case UpgradeType.HPRegen:
                if (HealthSystem.Instance != null)
                    HealthSystem.Instance.hpRegenBonus = value;
                break;
        }
    }

    void OnDestroy()
    {
        if (UpgradeSystem.Instance != null)
            UpgradeSystem.Instance.OnUpgradeChanged -= ApplyUpgrade;
    }
}

