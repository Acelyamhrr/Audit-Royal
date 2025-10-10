using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    public Image fadeImage;        // Assigne le même FadeImage que pour le fade-out
    public float fadeDuration = 1f;

    void Start()
    {
        if (fadeImage != null)
        {
            // Commence complètement noir
            fadeImage.color = new Color(0, 0, 0, 1);
            StartCoroutine(FadeInCoroutine());
        }
    }

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

