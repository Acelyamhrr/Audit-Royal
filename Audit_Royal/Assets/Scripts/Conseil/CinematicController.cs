using UnityEngine;
using System.Collections;

public class CinematicController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;  // Animator sur CinematicOverlay
    public Camera cam;

    [Header("Camera Settings")]
    public float normalSize = 8f;
    public float zoomSize = 6.5f;

    [Header("Timing (doit correspondre à tes animations)")]
    public float fadeDuration = 1.8f;  // Durée de ton animation Fade_InOut
    public float barsDuration = 1.0f;  // Durée de Bars_In ou Bars_Out

    private bool isInCinematic = false;

    // ENTRER EN CINÉMATIQUE
    public IEnumerator EnterCinematic()
    {
        if (isInCinematic) yield break;
        isInCinematic = true;

        // 1. Fade to black
        animator.SetTrigger("Fade");
        yield return new WaitForSeconds(fadeDuration);

        // 2. Zoom caméra (invisible, on est en noir)
        if (cam != null)
            cam.orthographicSize = zoomSize;

        // 3. Barres entrent
        animator.SetTrigger("Bars_In");
        yield return new WaitForSeconds(barsDuration);
    }

    // SORTIR DE LA CINÉMATIQUE
    public IEnumerator ExitCinematic()
    {
        if (!isInCinematic) yield break;

        // 1. Barres sortent
        animator.SetTrigger("Bars_Out");
        yield return new WaitForSeconds(barsDuration);

        // 2. Fade to black
        animator.SetTrigger("Fade");
        yield return new WaitForSeconds(fadeDuration);

        // 3. Revenir au zoom normal
        if (cam != null)
            cam.orthographicSize = normalSize;

        isInCinematic = false;
    }

    public bool IsInCinematic()
    {
        return isInCinematic;
    }
}