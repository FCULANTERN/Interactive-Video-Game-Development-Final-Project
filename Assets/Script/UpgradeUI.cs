using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI instructionText;

    private void Awake()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }


    void Start()
    {
        if (UpgradeSystem.Instance == null)
        {
            Debug.LogError("UpgradeSystem not found!");
            return;
        }

        UpgradeSystem.Instance.OnGoldChanged += UpdateGoldDisplay;

        UpdateGoldDisplay(UpgradeSystem.Instance.CurrentGold);

        if (instructionText != null)
            instructionText.text = "Press 'U' to open upgrades";

    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame && upgradePanel != null)
            upgradePanel.SetActive(!upgradePanel.activeSelf);
    }

    void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }

    void OnDestroy()
    {
        if (UpgradeSystem.Instance != null)
            UpgradeSystem.Instance.OnGoldChanged -= UpdateGoldDisplay;
    }
}

