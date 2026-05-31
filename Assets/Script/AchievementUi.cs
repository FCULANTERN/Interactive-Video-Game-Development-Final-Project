using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    [System.Serializable]
    public class AchievementSlot
    {
        public Image colorImage;
        public Image grayImage;
    }

    public AchievementSlot[] slots = new AchievementSlot[6];

    void OnEnable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementChanged += OnAchievementChanged;

        RefreshAll();
    }

    void OnDisable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementChanged -= OnAchievementChanged;
    }

    private void OnAchievementChanged(int index, bool unlocked)
    {
        if (index == -1)
            RefreshAll();
        else
            RefreshSlot(index);
    }

    public void RefreshAll()
    {
        if (AchievementManager.Instance == null) return;

        for (int i = 0; i < slots.Length; i++)
            RefreshSlot(i);
    }

    private void RefreshSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        AchievementSlot slot = slots[index];
        if (slot == null) return;

        bool unlocked = AchievementManager.Instance != null
                        && AchievementManager.Instance.IsUnlocked[index];

        if (slot.colorImage != null) slot.colorImage.gameObject.SetActive(unlocked);
        if (slot.grayImage != null) slot.grayImage.gameObject.SetActive(!unlocked);
    }
}