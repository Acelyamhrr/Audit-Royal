using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Gère l'effet de fondu entrant (fade-in) d'une scène.
/// À attacher sur un GameObject contenant une Image couvrant l'écran.
/// </summary>
public class SceneFadeIn : MonoBehaviour
{
    /// <summary>
    /// L'image utilisée pour le fondu. Doit être assignée depuis l'Inspector.
    /// </summary>
    public Image fadeImage;
    
    /// <summary>
    /// Durée du fondu en secondes.
    /// </summary>
    public float fadeDuration = 1f;

    /// <summary>
    /// Initialisation du fondu. L'image commence noire et devient transparente progressivement.
    /// </summary>
    void Start()
    {
        if (fadeImage != null)
        {
            // Commence complètement noir
            fadeImage.color = new Color(0, 0, 0, 1);
            StartCoroutine(FadeInCoroutine());
        }
    }

    /// <summary>
    /// Coroutine qui anime le fondu entrant.
    /// </summary>
    /// <returns>IEnumerator pour la coroutine.</returns>
    private IEnumerator FadeInCoroutine()
    {
        float t = 0;
        Color color = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, t / fadeDuration); // Devient transparent progressivement
            fadeImage.color = color;
            yield return null;
        }
    }
}

