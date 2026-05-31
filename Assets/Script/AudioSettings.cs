using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class AudioSettings : MonoBehaviour
{
    public enum Channel { Master, Music, SFX }

    [SerializeField] private Channel channel = Channel.Master;

    [FormerlySerializedAs("masterSlider")]
    [SerializeField] private Slider slider;
    [FormerlySerializedAs("masterText")]
    [SerializeField] private TextMeshProUGUI volumeText;

    private string VolumeKey => "save_volume_" + channel;

    // 0–1 value for this channel; read this when wiring up audio later.
    public float Volume01 { get; private set; } = 1f;

    private void Start()
    {
        float saved = PlayerPrefs.GetFloat(VolumeKey, slider.value);
        slider.value = saved;
        ApplyVolume(saved);
        slider.onValueChanged.AddListener(ApplyVolume);
    }

    private void ApplyVolume(float value)
    {
        volumeText.text = $"{(int)value} %";
        Volume01 = value / 100f;

        if (channel == Channel.Master)
            AudioListener.volume = Volume01;

        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
