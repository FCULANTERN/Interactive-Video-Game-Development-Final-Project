using UnityEngine;
using UnityEngine.Audio;

public class AudioInit : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    private void Start()
    {
        ApplySavedVolumes();
    }

    private void ApplySavedVolumes()
    {
        Set("Master", "MasterVolume");
        Set("Music", "MusicVolume");
        Set("SFX", "SFXVolume");
    }

    private void Set(string key, string param)
    {
        float value = PlayerPrefs.GetFloat("save_volume_" + key, 100f);
        float normalized = value / 100f;
        float dB = Mathf.Log10(Mathf.Max(normalized, 0.0001f)) * 20f;

        mixer.SetFloat(param, dB);
    }
}