using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TextMeshProUGUI masterText;

    private void Start()
    {
        UpdateMasterVolume(masterSlider.value);

    }

    private void UpdateMasterVolume(float value)
    {
        masterText.text = $"{(int)value} %";
    }
}