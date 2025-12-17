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
    
    public GameObject salleNormal;
    public GameObject salleZoom;

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
        // 1️⃣ Fade IN (écran noir)
        animator.SetTrigger("Fade");
        
        // 2️⃣ Switch image (caché par le noir)
        salleNormal.SetActive(false);
        salleZoom.SetActive(true);
        
        yield return new WaitForSeconds(0.3f);
        

        // 3️⃣ Fade OUT + barres visibles
        animator.SetTrigger("Bars_In");
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

        // 2️⃣ Fade IN
        animator.SetTrigger("Fade");
        // 3️⃣ Retour image normale
        salleZoom.SetActive(false);
        salleNormal.SetActive(true);
        yield return new WaitForSeconds(0.3f);

    }


    public bool IsInCinematic()
    {
        return isInCinematic;
    }
}