using UnityEngine;
using System.Collections;

/// <summary>
/// Contrôleur de cinématiques gérant les transitions visuelles
/// (barres noires, zoom caméra, fade) pour les séquences narratives.
/// </summary>
public class CinematicController : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("References")]
    [Tooltip("Animator contrôlant les barres cinématiques")]
    public Animator animator;

    [Header("Camera Settings")]
    [Tooltip("Taille normale de la caméra orthographique")]
    public float normalSize = 8f;
    
    [Tooltip("Taille de la caméra lors du zoom cinématique")]
    public float zoomSize = 6.5f;

    [Header("Timing")]
    [Tooltip("Durée de l'effet de fade (en secondes)")]
    public float fadeDuration = 1.8f;
    
    [Tooltip("Durée d'animation des barres cinématiques (en secondes)")]
    public float barsDuration = 1.0f;
    
    #endregion

    #region Private Fields
    
    /// <summary>
    /// Indique si une séquence cinématique est actuellement active
    /// </summary>
    private bool isInCinematic = false;
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Initialisation au démarrage - Vérifie la présence de l'Animator
    /// </summary>
    void Start()
    {
        
        if (animator == null)
            Debug.LogError("ANIMATOR EST NULL ! Assigne-le dans l'Inspector !");
        else
            Debug.Log("Animator trouvé : " + animator.name);
    }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Démarre une séquence cinématique avec apparition des barres noires
    /// </summary>
    public void EnterCinematic()
    {

        StartCoroutine(EnterRoutine());        
        
    }
    
    /// <summary>
    /// Termine une séquence cinématique avec disparition des barres noires
    /// </summary>
    public void ExitCinematic()
    {
        
        StartCoroutine(ExitRoutine());
        
    }
    
    /// <summary>
    /// Vérifie si une cinématique est actuellement en cours
    /// </summary>
    /// <returns>True si en mode cinématique, False sinon</returns>
    public bool IsInCinematic()
    {
        return isInCinematic;
    }
    
    #endregion

    #region Coroutines
    
    /// <summary>
    /// Coroutine gérant l'entrée progressive en mode cinématique
    /// - Active l'animation des barres noires
    /// - Attend la fin de l'animation
    /// </summary>
    IEnumerator EnterRoutine()
    {
        // Marque le début de la séquence cinématique
        isInCinematic = true;
        
        // Déclencher l'animation des barres noires
        if (animator != null)
        {
            animator.SetTrigger("Bars_In");
            Debug.Log("Trigger 'Bars_In' activé");
        }
        
        // Attendre que l'animation se termine
        yield return new WaitForSeconds(barsDuration);
        
    }
    
    /// <summary>
    /// Coroutine gérant la sortie du mode cinématique
    /// - Active l'animation de disparition des barres noires
    /// - Attend un court délai avant de terminer
    /// </summary>
    IEnumerator ExitRoutine()
    {
        // déclenche l'animation de disparition des barres noires
        animator.SetTrigger("Bars_Out");
        
        // attend un délai pour la transition visuelle
        yield return new WaitForSeconds(0.4f);
        
    }
    
    #endregion
    
}