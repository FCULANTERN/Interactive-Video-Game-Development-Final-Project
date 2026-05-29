using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI instructionText;

    private bool isOpen = false;

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

            isOpen = !upgradePanel.activeSelf;

            upgradePanel.SetActive(isOpen);

            if (isOpen)
            {
                Time.timeScale = 0f; // pause
            }
            else
            {
                Time.timeScale = 1f; // reprise
            }
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

