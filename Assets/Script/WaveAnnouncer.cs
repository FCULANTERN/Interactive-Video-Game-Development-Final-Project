using UnityEngine;
using TMPro;

public class WaveAnnouncer : MonoBehaviour
{
    [Tooltip("Text used for the banner. Defaults to the TMP on this GameObject.")]
    public TextMeshProUGUI bannerText;

    [Tooltip("Total time the 'Wave X' banner stays up when a wave begins.")]
    public float bannerDuration = 2f;

    [Tooltip("Time the banner takes to pop in to full size.")]
    public float scaleInDuration = 0.35f;

    [Tooltip("How much the pop overshoots before settling (0 = no overshoot).")]
    public float overshoot = 1.7f;

    [TextArea]
    public string waveStartFormat = "Wave {0}\n波次 {0}";

    [TextArea]
    public string bossWaveFormat = "Boss Wave {0}!\n魔王波次 {0}！";

    private int lastWave = 0;
    private float bannerTimer;
    private Vector3 baseScale = Vector3.one;

    void Awake()
    {
        if (bannerText == null)
            bannerText = GetComponent<TextMeshProUGUI>();
        if (bannerText != null)
        {
            baseScale = bannerText.transform.localScale;
            bannerText.enabled = false;
        }
    }

    void Update()
    {
        var spawner = ZombieWaveSpawner.Instance;
        if (spawner == null || bannerText == null)
            return;

        if (spawner.currentWave != lastWave)
        {
            lastWave = spawner.currentWave;
            bannerTimer = bannerDuration;
            string format = spawner.isBossWave ? bossWaveFormat : waveStartFormat;
            bannerText.text = string.Format(format, spawner.currentWave);
        }

        if (bannerTimer > 0f)
        {
            float elapsed = bannerDuration - bannerTimer;
            bannerText.enabled = true;
            bannerText.transform.localScale = baseScale * PopScale(elapsed);
            bannerTimer -= Time.deltaTime;
        }
        else if (bannerText.enabled)
        {
            bannerText.enabled = false;
            bannerText.transform.localScale = baseScale;
        }
    }

    float PopScale(float elapsed)
    {
        if (scaleInDuration <= 0f)
            return 1f;

        float t = Mathf.Clamp01(elapsed / scaleInDuration);
        return EaseOutBack(t);
    }

    float EaseOutBack(float t)
    {
        float c1 = overshoot;
        float c3 = c1 + 1f;
        float p = t - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }
}
