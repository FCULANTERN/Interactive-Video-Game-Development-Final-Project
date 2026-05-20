using UnityEngine;

/// <summary>
/// 技能傷害基底類別。
/// 每個技能腳本繼承此類，並實作 SkillType 屬性即可自動從升級系統取得傷害。
/// 往後新增技能：
///   1. 在 UpgradeType 加入新 enum 值
///   2. 建立新類別繼承 SkillBase，覆寫 SkillType
///   3. 在 UpgradeSystem.InitializeUpgrades() 加入對應 UpgradeData
/// </summary>
public abstract class SkillBase : MonoBehaviour
{
    [Header("Base Damage (before upgrade)")]
    [SerializeField] protected int baseDamage = 5;

    /// <summary>子類別必須指定此技能對應的升級類型</summary>
    public abstract UpgradeType SkillType { get; }

    /// <summary>取得升級後的最終傷害</summary>
    public int GetDamage()
    {
        if (UpgradeSystem.Instance != null)
            return UpgradeSystem.Instance.GetSkillDamage(SkillType);
        return baseDamage;
    }
}
