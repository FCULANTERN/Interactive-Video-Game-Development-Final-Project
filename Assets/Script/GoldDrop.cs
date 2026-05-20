using UnityEngine;

public class GoldDrop : MonoBehaviour
{
    [SerializeField] private int goldAmount = 10;

    public void DropGold()
    {
        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.AddGold(goldAmount);
        }
    }

    public void SetGoldAmount(int amount)
    {
        goldAmount = amount;
    }
}
