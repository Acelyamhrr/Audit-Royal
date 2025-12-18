using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Composant à placer sur un Slider Unity pour contrôler le volume global.
/// Se synchronise automatiquement avec l'AudioManager au moment de l'activation.
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    
    /// <summary>
    /// Référence interne au composant Slider.
    /// </summary>
    private Slider slider;

    /// <summary>
    /// Récupère le composant Slider attaché à l'objet.
    /// </summary>
    void Awake()
    {
        slider = GetComponent<Slider>();
    }
    
    /// <summary>
    /// Initialise le slider lors de l'activation du panneau (ex: ouverture des settings).
    /// </summary>
    void OnEnable()
    {
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(ChangerVolumeManuel);
        if (AudioManager.instance != null && AudioManager.instance.musiqueSource != null)
        {
            slider.value = AudioManager.instance.musiqueSource.volume;
        }
    }
    
    
    /// <summary>
    /// Transmet la nouvelle valeur du slider à l'AudioManager.
    /// </summary>
    /// /// <param name="valeur">Valeur entre 0.0 et 1.0 envoyée par le Slider.</param>
    public void ChangerVolumeManuel(float valeur)
    {

        if (AudioManager.instance != null)
        {
            AudioManager.instance.reglerVolume(valeur);
        }
    }
}