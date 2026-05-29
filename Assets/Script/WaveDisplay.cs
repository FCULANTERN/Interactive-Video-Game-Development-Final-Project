using UnityEngine;
using TMPro;

public class WaveDisplay : MonoBehaviour
{
    private TextMeshProUGUI waveText;

    void Start()
    {
        waveText = GetComponent<TextMeshProUGUI>();

        if (ZombieWaveSpawner.Instance != null)
        {
            ZombieWaveSpawner.Instance.OnWaveChanged += UpdateDisplay;
            UpdateDisplay(ZombieWaveSpawner.Instance.currentWave);
        }
    }

    void UpdateDisplay(int wave)
    {
        if (waveText != null)
            waveText.text = $"Wave: {wave}";
    }

    void OnDestroy()
    {
        if (ZombieWaveSpawner.Instance != null)
            ZombieWaveSpawner.Instance.OnWaveChanged -= UpdateDisplay;
    }
}
