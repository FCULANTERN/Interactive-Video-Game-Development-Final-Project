using UnityEngine;
using TMPro;

public class WaveDisplay : MonoBehaviour
{
    public string format = "Wave: {0}";

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
            waveText.text = string.Format(format, wave);
    }

    void OnDestroy()
    {
        if (ZombieWaveSpawner.Instance != null)
            ZombieWaveSpawner.Instance.OnWaveChanged -= UpdateDisplay;
    }
}
