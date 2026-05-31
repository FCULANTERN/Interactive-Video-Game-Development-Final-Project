using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;

public class AudioSettings : MonoBehaviour
{
    public enum Channel { Master, Music, SFX }

    [SerializeField] private Channel channel = Channel.Master;

    [FormerlySerializedAs("masterSlider")]
    [SerializeField] private Slider slider;
    [FormerlySerializedAs("masterText")]
    [SerializeField] private TextMeshProUGUI volumeText;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string exposedParamName;

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

        float normalized = value / 100f;
        float dB = Mathf.Log10(Mathf.Max(normalized, 0.0001f)) * 20f;

        if (mixer != null)
        {
            if (channel == Channel.Master)
                mixer.SetFloat(exposedParamName, dB);

            if (channel == Channel.Music)
                mixer.SetFloat(exposedParamName, dB);

            if (channel == Channel.SFX)
                mixer.SetFloat(exposedParamName, dB);
        }

        PlayerPrefs.SetFloat(VolumeKey, value);
    }
}
