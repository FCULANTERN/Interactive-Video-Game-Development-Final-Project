# 玩家升級系統設置指南

## 🎮 系統概述
這套升級系統包含以下核心組件：
- **UpgradeSystem.cs** - 升級邏輯管理器
- **UpgradeUI.cs** - 升級UI控制
- **UpgradeButton.cs** - 單個升級按鈕
- **PlayerUpgradable.cs** - 玩家升級效果應用
- **GoldDrop.cs** - 敵人掉落金幣

## 📋 設置步驟

### 1. 建立升級系統管理器
1. 在場景中建立一個空的 GameObject，命名為 "UpgradeSystem"
2. 將 **UpgradeSystem.cs** 腳本拖到此 GameObject
3. 確保只有一個 UpgradeSystem 在場景中（會自動持久化）

### 2. 設置 UI Canvas
1. 在場景中建立一個 Canvas（如果還沒有）
2. 在 Canvas 下建立一個 Panel，命名為 "UpgradePanel"
3. 在 UpgradePanel 內建立以下 UI 元素：

#### UpgradePanel 結構：
```
Canvas
└─ UpgradePanel (Image)
   ├─ Title (TextMeshPro) - "升級列表"
   ├─ GoldDisplay (TextMeshPro) - 顯示金幣數量
   ├─ InstructionText (TextMeshPro) - 按鍵說明
   ├─ ButtonContainer (VerticalLayoutGroup)
   │  └─ UpgradeButtonPrefab (預製物)
   └─ CloseButton (Button - 可選)
```

### 3. 建立升級按鈕預製物
1. 建立一個 Prefab，結構如下：
```
UpgradeButton (Button)
├─ UpgradeName (TextMeshProUGUI)
├─ UpgradeDescription (TextMeshProUGUI)
├─ LevelText (TextMeshProUGUI) - "Level: X/10"
├─ CostText (TextMeshProUGUI) - "Cost: X"
```

2. 將 **UpgradeButton.cs** 腳本添加到根 GameObject
3. 在 Inspector 中分配所有文本欄位

### 4. 配置 UI 管理器
1. 在 Canvas 上創建一個新 GameObject，命名為 "UpgradeUIManager"
2. 添加 **UpgradeUI.cs** 腳本
3. 在 Inspector 中配置：
   - **Upgrade Panel** - UpgradePanel GameObject
   - **Gold Text** - GoldDisplay TextMeshPro
   - **Upgrade Button Container** - ButtonContainer Transform
   - **Upgrade Button Prefab** - UpgradeButton Prefab
   - **Instruction Text** - InstructionText TextMeshPro

### 5. 配置玩家升級
1. 選中玩家 GameObject
2. 添加 **PlayerUpgradable.cs** 腳本
3. 確保玩家上有：
   - PlayerAttack 組件
   - PlayerHealth 組件

### 6. 配置敵人金幣掉落
1. 選中敵人 Prefab（或敵人 GameObject）
2. 添加 **GoldDrop.cs** 腳本
3. 在 Inspector 中設置 **Gold Amount**（推薦 5-20）
4. 確保敵人上有 **Damageable.cs** 組件

## 🎯 升級類型說明

### 1. 攻擊傷害 (Attack Damage)
- **效果**：增加每次攻擊的傷害值
- **每級效果**：+1 傷害
- **初始成本**：20 金幣
- **最大等級**：10

### 2. 攻擊速度 (Attack Speed)
- **效果**：減少攻擊冷卻時間
- **每級效果**：-0.05s 冷卻
- **初始成本**：25 金幣
- **最大等級**：10

### 3. 最大血量 (Max Health)
- **效果**：增加玩家最大生命值
- **每級效果**：+5 血量
- **初始成本**：30 金幣
- **最大等級**：10

### 4. 爆擊機率 (Critical Chance)
- **效果**：增加攻擊爆擊機率（爆擊傷害為 2x）
- **每級效果**：+2% 爆擊機率
- **初始成本**：40 金幣
- **最大等級**：5

## ⌨️ 控制鍵

| 按鍵 | 功能 |
|------|------|
| **U** | 開啟/關閉升級 UI |
| **1-4** | 快速升級（需要 UI 開啟） |
| **滑鼠左鍵** | 點擊升級按鈕 |

## 💰 金幣系統

### 獲得金幣
- **擊殺敵人**：根據敵人配置的 GoldDrop 數量掉落
- **代碼添加**：`UpgradeSystem.Instance.AddGold(amount)`

### 花費金幣
- 升級費用逐級增加：`成本 = 基礎成本 × 當前等級`
- 例如：等級 1→2 需要 20 金幣，等級 2→3 需要 40 金幣

## 🔧 自定義升級

要添加新的升級類型，編輯 UpgradeSystem.cs 的 InitializeUpgrades() 方法：

```csharp
void InitializeUpgrades()
{
    upgrades = new Upgrade[]
    {
        // ... existing upgrades ...
        new Upgrade 
        { 
            name = "新升級名稱", 
            description = "升級描述",
            currentLevel = 1,
            maxLevel = 10,
            costPerLevel = 50,
            valuePerLevel = 1f
        }
    };
}
```

然後在 PlayerUpgradable.cs 的 ApplyUpgrade() 方法中添加案例：

```csharp
case 4: // 新升級索引
    // 應用升級邏輯
    break;
```

## 🐛 常見問題

**Q: 升級不工作？**
A: 確保玩家上有 PlayerUpgradable.cs 且場景中有 UpgradeSystem.Instance

**Q: 金幣沒有增加？**
A: 確保敵人上有 GoldDrop.cs 組件且已設置金幣數量

**Q: UI 不顯示？**
A: 確認 UpgradeUI.cs 中所有 UI 元件都正確分配

## 📝 進階功能

### 添加升級音效
在 UpgradeButton.cs 中添加：
```csharp
public AudioClip upgradeSound;

public void TryUpgrade()
{
    // ... upgrade logic ...
    if (upgrade successful)
        AudioSource.PlayClipAtPoint(upgradeSound, Vector3.zero);
}
```

### 添加升級動畫
在 UpgradeButton.cs 中添加 UI 動畫效果

### 保存進度
擴展 UpgradeSystem.cs 添加 Save/Load 功能

祝遊戲開發順利！🚀
