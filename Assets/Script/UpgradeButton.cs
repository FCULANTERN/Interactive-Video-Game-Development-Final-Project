using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private TextMeshProUGUI upgradeName;
    [SerializeField] private TextMeshProUGUI upgradeDescription;
    [SerializeField] private TextMeshProUGUI upgradeLevelText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button upgradeButton;

    [Header("Icon")]
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private Image lockOverlay;      // 半透明黑色遮罩
    [SerializeField] private GameObject lockLabel;   // 顯示鎖住原因的 GameObject

    [Header("Colors")]
    [SerializeField] private Color canAffordColor   = Color.green;
    [SerializeField] private Color cannotAffordColor = Color.red;
    [SerializeField] private Color maxLevelColor    = Color.yellow;
    [SerializeField] private Color lockedColor      = new Color(0.4f, 0.4f, 0.4f);

    void Start()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(TryUpgrade);

        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.OnUpgradeChanged += OnUpgradeChanged;
            UpgradeSystem.Instance.OnGoldChanged    += _ => Refresh(); // 金幣變化時也刷新顏色
        }

        Refresh();
    }

    void OnUpgradeChanged(UpgradeType type)
    {
        if (type == upgradeType) Refresh();
    }

    public void Refresh()
    {
        if (UpgradeSystem.Instance == null) return;

        var data = UpgradeSystem.Instance.GetData(upgradeType);
        if (data == null) return;

        if (upgradeName != null)        upgradeName.text        = data.name;
        if (upgradeDescription != null) upgradeDescription.text = data.description;
        if (upgradeLevelText != null)   upgradeLevelText.text   = $"Lv. {data.currentLevel}/{data.maxLevel}";

        // ── 鎖定狀態 ──
        if (data.isLocked)
        {
            SetLocked(data.lockedReason);
            return;
        }

        // 解除鎖定遮罩
        if (lockOverlay != null) lockOverlay.gameObject.SetActive(false);
        if (lockLabel != null)   lockLabel.SetActive(false);
        if (upgradeButton != null) upgradeButton.interactable = true;

        // ── 最高等級 ──
        if (data.currentLevel >= data.maxLevel)
        {
            if (upgradeCostText != null)
            {
                upgradeCostText.text  = "滿等";
                upgradeCostText.color = maxLevelColor;
            }
            if (upgradeButton != null) upgradeButton.interactable = false;
            return;
        }

        // ── 正常狀態：顯示費用 ──
        int cost = data.UpgradeCost;
        bool canAfford = UpgradeSystem.Instance.CurrentGold >= cost;

        if (upgradeCostText != null)
        {
            upgradeCostText.text  = $"Cost: {cost}";
            upgradeCostText.color = canAfford ? canAffordColor : cannotAffordColor;
        }
        if (upgradeButton != null) upgradeButton.interactable = canAfford;
    }

    void SetLocked(string reason)
    {
        if (lockOverlay != null) lockOverlay.gameObject.SetActive(true);
        if (lockLabel != null)
        {
            lockLabel.SetActive(true);
            var txt = lockLabel.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = reason;
        }
        if (upgradeCostText != null)
        {
            upgradeCostText.text  = "🔒 未解鎖";
            upgradeCostText.color = lockedColor;
        }
        if (upgradeButton != null) upgradeButton.interactable = false;
    }

    public void TryUpgrade()
    {
        UpgradeSystem.Instance?.TryUpgrade(upgradeType);
    }

    void OnDestroy()
    {
        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.OnUpgradeChanged -= OnUpgradeChanged;
            UpgradeSystem.Instance.OnGoldChanged    -= _ => Refresh();
        }
    }
}

