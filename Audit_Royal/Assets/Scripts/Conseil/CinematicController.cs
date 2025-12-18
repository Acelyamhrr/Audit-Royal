using UnityEngine;
using System.Collections;

public class CinematicController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Camera Settings")]
    public float normalSize = 8f;
    public float zoomSize = 6.5f;

    [Header("Timing")]
    public float fadeDuration = 1.8f;
    public float barsDuration = 1.0f;

    private bool isInCinematic = false;
    
    void Start()
    {
        Debug.Log("🎬 CinematicController - START");
        
        if (animator == null)
            Debug.LogError("❌ ANIMATOR EST NULL ! Assigne-le dans l'Inspector !");
        else
            Debug.Log("✅ Animator trouvé : " + animator.name);
    }

    public void EnterCinematic()
    {
        Debug.Log("🎬 ========== ENTER CINEMATIC START ==========");

        StartCoroutine(EnterRoutine());        
        Debug.Log("🎬 ========== ENTER CINEMATIC END ==========");
    }
    
    IEnumerator EnterRoutine()
    {
        isInCinematic = true;
        
        // Déclencher l'animation des barres
        if (animator != null)
        {
            animator.SetTrigger("Bars_In");
            Debug.Log("Trigger 'Bars_In' activé");
        }
        
        // Attendre que l'animation se termine
        yield return new WaitForSeconds(barsDuration);
        
        Debug.Log("🎬 EnterRoutine terminée");
    }


    public void ExitCinematic()
    {
        Debug.Log("🎬 ========== EXIT CINEMATIC START ==========");
        
        StartCoroutine(ExitRoutine());
        
        Debug.Log("🎬 ========== EXIT CINEMATIC END ==========");
    }
    
    IEnumerator ExitRoutine()
    {
        // 1️⃣ Barres repartent
        animator.SetTrigger("Bars_Out");
        yield return new WaitForSeconds(0.4f);
        
    }


    public bool IsInCinematic()
    {
        return isInCinematic;
    }
}