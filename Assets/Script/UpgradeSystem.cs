using UnityEngine;
using System;
using System.Collections.Generic;

// 所有升級類型，往後新增技能只需在此加入新 enum 值
public enum UpgradeType
{
    AttackDamage    = 0,  // 普攻傷害
    MoveSpeed       = 1,  // 移動速度
    HPRegen         = 2,  // HP 回復速度
    ManaRegen       = 3,  // Mana 回復速度
    SkillProjectile = 4,  // E 鍵技能傷害
    SkillSlash      = 5,  // Q 鍵技能傷害
    // 往後新增技能在此繼續加 ↓
    // SkillFireball = 6,
}

public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem Instance { get; private set; }

    [System.Serializable]
    public class UpgradeData
    {
        public UpgradeType type;
        public string name;
        public string description;
        public int currentLevel = 1;
        public int maxLevel = 5;
        public int goldCostPerLevel = 20;
        public float valuePerLevel = 1f;

        [Header("Lock Settings")]
        public bool isLocked = false;
        [Tooltip("顯示給玩家的解鎖條件說明")]
        public string lockedReason = "";

        public float CurrentValue => valuePerLevel * currentLevel;
        public int UpgradeCost => goldCostPerLevel * currentLevel;
    }

    [SerializeField] private int currentGold = 0;
    [SerializeField] private UpgradeData[] upgrades;

    // 用 Dictionary 快速查找
    private Dictionary<UpgradeType, UpgradeData> upgradeMap = new Dictionary<UpgradeType, UpgradeData>();

    public int CurrentGold => currentGold;
    public UpgradeData[] Upgrades => upgrades;

    public event Action<int> OnGoldChanged;
    public event Action<UpgradeType> OnUpgradeChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUpgrades();
        LoadProgress();
    }

    private const string GoldKey = "save_gold";
    private const string LevelKeyPrefix = "save_upgrade_level_";

    private void LoadProgress()
    {
        currentGold = PlayerPrefs.GetInt(GoldKey, currentGold);
        foreach (var u in upgrades)
            u.currentLevel = PlayerPrefs.GetInt(LevelKeyPrefix + (int)u.type, u.currentLevel);
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(GoldKey, currentGold);
        foreach (var u in upgrades)
            PlayerPrefs.SetInt(LevelKeyPrefix + (int)u.type, u.currentLevel);
        PlayerPrefs.Save();
    }

    void InitializeUpgrades()
    {
        if (upgrades == null || upgrades.Length == 0)
        {
            upgrades = new UpgradeData[]
            {
                new UpgradeData
                {
                    type           = UpgradeType.AttackDamage,
                    name           = "Attack Damage",
                    description    = "普攻傷害 +1 / 等級",
                    currentLevel   = 1, maxLevel = 10,
                    goldCostPerLevel = 20, valuePerLevel = 1f,
                    isLocked = false
                },
                new UpgradeData
                {
                    type           = UpgradeType.MoveSpeed,
                    name           = "冰凍",
                    description    = "R 鍵冰凍時間 +0.5s / 等級",
                    currentLevel   = 1, maxLevel = 10,
                    goldCostPerLevel = 25, valuePerLevel = 5f,
                    isLocked = false
                },
                new UpgradeData
                {
                    type           = UpgradeType.HPRegen,
                    name           = "HP Regen",
                    description    = "HP 回復速度 +0.5 / 等級",
                    currentLevel   = 1, maxLevel = 10,
                    goldCostPerLevel = 30, valuePerLevel = 0.5f,
                    isLocked = false
                },
                new UpgradeData
                {
                    type           = UpgradeType.ManaRegen,
                    name           = "Mana Regen",
                    description    = "Mana 回復速度 +1 / 等級",
                    currentLevel   = 1, maxLevel = 10,
                    goldCostPerLevel = 30, valuePerLevel = 1f,
                    isLocked = false
                },
                new UpgradeData
                {
                    type           = UpgradeType.SkillProjectile,
                    name           = "Skill: Projectile",
                    description    = "投擲技能傷害 +5 / 等級",
                    currentLevel   = 1, maxLevel = 10,
                    goldCostPerLevel = 40, valuePerLevel = 5f,
                    isLocked = false
                },
                new UpgradeData
                {
                    type           = UpgradeType.SkillSlash,
                    name           = "Skill: Slash",
                    description    = "Q 鍵技能傷害 +3 / 等級",
                    currentLevel   = 1, maxLevel = 10,
                    goldCostPerLevel = 40, valuePerLevel = 3f,
                    isLocked = false,
                    lockedReason = "累積擊殺 20 個敵人後解鎖"
                },
            };
        }

        // 建立 Dictionary 快速查找
        upgradeMap.Clear();
        foreach (var u in upgrades)
            upgradeMap[u.type] = u;
    }

    // ── 主要 API ──────────────────────────────────────────────

    /// <summary>嘗試升級指定類型，回傳是否成功</summary>
    public bool TryUpgrade(UpgradeType type)
    {
        if (!upgradeMap.TryGetValue(type, out var upgrade))
            return false;

        if (upgrade.isLocked)
        {
            Debug.Log($"{upgrade.name} 尚未解鎖！{upgrade.lockedReason}");
            return false;
        }

        if (upgrade.currentLevel >= upgrade.maxLevel)
        {
            Debug.Log($"{upgrade.name} 已達最高等級！");
            return false;
        }

        int cost = upgrade.UpgradeCost;
        if (currentGold < cost)
        {
            Debug.Log($"金幣不足！需要 {cost}，擁有 {currentGold}");
            return false;
        }

        currentGold -= cost;
        upgrade.currentLevel++;

        SaveProgress();

        OnGoldChanged?.Invoke(currentGold);
        OnUpgradeChanged?.Invoke(type);

        Debug.Log($"{upgrade.name} 升至 Lv.{upgrade.currentLevel}！");
        return true;
    }

    /// <summary>解鎖指定技能升級</summary>
    public void Unlock(UpgradeType type)
    {
        if (upgradeMap.TryGetValue(type, out var upgrade))
        {
            upgrade.isLocked = false;
            OnUpgradeChanged?.Invoke(type);
            Debug.Log($"{upgrade.name} 已解鎖！");
        }
    }

    /// <summary>取得指定類型的當前數值</summary>
    public float GetValue(UpgradeType type)
    {
        return upgradeMap.TryGetValue(type, out var u) ? u.CurrentValue : 0f;
    }

    /// <summary>取得技能傷害（int）</summary>
    public int GetSkillDamage(UpgradeType type) => (int)GetValue(type);

    /// <summary>取得升級花費</summary>
    public int GetCost(UpgradeType type)
    {
        return upgradeMap.TryGetValue(type, out var u) ? u.UpgradeCost : 0;
    }

    /// <summary>取得升級資料（唯讀）</summary>
    public UpgradeData GetData(UpgradeType type)
    {
        upgradeMap.TryGetValue(type, out var u);
        return u;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        SaveProgress();
        OnGoldChanged?.Invoke(currentGold);
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
        {
            currentGold = 100;
            SaveProgress();
            OnGoldChanged?.Invoke(currentGold);
            Debug.Log("金幣已重置為 100");
        }
    }

    [ContextMenu("Reset All Upgrades")]
    public void ResetAllUpgrades()
    {
        foreach (var u in upgrades)
        {
            u.currentLevel = 1;
            PlayerPrefs.DeleteKey(LevelKeyPrefix + (int)u.type);
            OnUpgradeChanged?.Invoke(u.type);
        }

        PlayerPrefs.Save();
        Debug.Log("升級系統已重置");
    }
}
