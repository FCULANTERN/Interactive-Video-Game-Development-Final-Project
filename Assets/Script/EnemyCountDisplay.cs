using UnityEngine;
using TMPro;

public class EnemyCountDisplay : MonoBehaviour
{
    public string format = "Enemies: {0}";

    private TextMeshProUGUI countText;
    private int lastCount = -1;

    void Start()
    {
        countText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (countText == null || ZombieWaveSpawner.Instance == null)
            return;

        int count = ZombieWaveSpawner.Instance.aliveEnemyCount;
        if (count != lastCount)
        {
            lastCount = count;
            countText.text = string.Format(format, count);
        }
    }
}
