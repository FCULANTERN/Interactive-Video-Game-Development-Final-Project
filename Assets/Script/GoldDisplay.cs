using UnityEngine;
using TMPro;

public class GoldDisplay : MonoBehaviour
{
    private TextMeshProUGUI goldText;

    void Start()
    {
        goldText = GetComponent<TextMeshProUGUI>();

        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.OnGoldChanged += UpdateDisplay;
            UpdateDisplay(UpgradeSystem.Instance.CurrentGold);
        }
    }

    void UpdateDisplay(int gold)
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }

    void OnDestroy()
    {
        if (UpgradeSystem.Instance != null)
            UpgradeSystem.Instance.OnGoldChanged -= UpdateDisplay;
    }
}
