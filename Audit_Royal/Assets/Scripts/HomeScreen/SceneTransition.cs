using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Gère la transition entre scènes avec un fondu sortant (fade-out).
/// À attacher sur un GameObject et assigner une Image couvrant l'écran.
/// </summary>
public class SceneTransition : MonoBehaviour
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
    /// Lance le fondu sortant puis charge la scène spécifiée.
    /// </summary>
    /// <param name="sceneName">Nom de la scène à charger après le fondu.</param>
    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    /// <summary>
    /// Coroutine qui anime le fondu sortant et charge la scène.
    /// </summary>
    /// <param name="sceneName">Nom de la scène à charger.</param>
    /// <returns>IEnumerator pour la coroutine.</returns>
    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        // FADE OUT
        float t = 0;
        Color color = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Charge la scène une fois que l'écran est noir
        SceneManager.LoadScene(sceneName);
    }
}

