using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(ChangerVolumeManuel);
        if (AudioManager.instance != null && AudioManager.instance.musiqueSource != null)
        {
            slider.value = AudioManager.instance.musiqueSource.volume;
        }
    }

    public void ChangerVolumeManuel(float valeur)
    {

        if (AudioManager.instance != null)
        {
            AudioManager.instance.reglerVolume(valeur);
        }
    }
}