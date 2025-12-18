using UnityEngine;

/// <summary>
/// Gestionnaire audio centralisé utilisant le pattern Singleton.
/// Ce script est "Immortel" (DontDestroyOnLoad) et gère la musique de fond du jeu.
/// </summary>
public class AudioManager : MonoBehaviour
{
    
    /// <summary>
    /// Instance statique permettant d'accéder à l'AudioManager depuis n'importe quel script.
    /// </summary>
    public static AudioManager instance;

    [Header("Configuration Audio")]
    
    /// <summary>
    /// Le composant AudioSource qui va diffuser le son.
    /// </summary>

    public AudioSource musiqueSource;
    
    /// <summary>
    /// Le fichier audio (musique) à jouer en boucle.
    /// </summary>
    public AudioClip musiqueDeFond;  
    
    
    /// <summary>
    /// Gère l'unicité de l'AudioManager et sa persistance entre les scènes.
    /// </summary>
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Debug.Log($"Name {gameObject.name}");
        DontDestroyOnLoad(gameObject);
    }
    
    
    /// <summary>
    /// Lance la musique configurée au démarrage du jeu.
    /// </summary>
    void Start()
    {
        if (musiqueSource != null && musiqueDeFond != null)
        {
            musiqueSource.clip = musiqueDeFond;
            musiqueSource.loop = true;
            musiqueSource.volume = 0;
            musiqueSource.Play();
        }
    }
    
    
    /// <summary>
    /// Modifie le volume de la source audio.
    /// </summary>
    /// /// <param name="volume">Nouvelle valeur de volume (0 à 1).</param>
    public void reglerVolume(float volume)
    {
        if (musiqueSource != null)
        {
            musiqueSource.volume = volume;
        }
    }
}