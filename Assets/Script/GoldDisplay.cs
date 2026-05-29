using UnityEngine;
using TMPro;

public class GoldDisplay : MonoBehaviour
{
    public string format = "Gold: {0}";

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
            goldText.text = string.Format(format, gold);
    }

    void OnDestroy()
    {
        if (UpgradeSystem.Instance != null)
            UpgradeSystem.Instance.OnGoldChanged -= UpdateDisplay;
    }
}
