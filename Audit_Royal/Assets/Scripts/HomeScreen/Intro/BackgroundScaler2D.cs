using UnityEngine;

/// <summary>
/// Ajuste automatiquement un Sprite de fond pour qu’il remplisse
/// entièrement la caméra orthographique, en acceptant une déformation.
/// Fonctionne en mode édition et en mode jeu.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundStretchToCamera : MonoBehaviour
{
    /// <summary>
    /// SpriteRenderer utilisé pour récupérer la taille du sprite.
    /// </summary>
    SpriteRenderer sr;
    
    /// <summary>
    /// Caméra utilisée comme référence pour le redimensionnement.
    /// </summary>
    Camera cam;
    
    /// <summary>
    /// Dernière résolution connue de l’écran pour détecter les changements.
    /// </summary>
    Vector2 lastScreen = Vector2.zero;

    /// <summary>
    /// Initialise les références au SpriteRenderer et à la caméra.
    /// </summary>
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        if (cam == null) cam = Camera.current;
    }

    /// <summary>
    /// Ajuste la taille du sprite au démarrage.
    /// </summary>
    void Start()
    {
        Stretch();
    }

    /// <summary>
    /// Vérifie si la résolution de l’écran a changé
    /// et redimensionne le sprite si nécessaire.
    /// </summary>
    void Update()
    {
        if (Screen.width != lastScreen.x || Screen.height != lastScreen.y)
        {
            Stretch();
            lastScreen = new Vector2(Screen.width, Screen.height);
        }
    }

    /// <summary>
    /// Redimensionne le sprite pour couvrir toute la zone visible
    /// de la caméra orthographique, avec une échelle indépendante sur X et Y.
    /// </summary>
    void Stretch()
    {
        if (sr == null || sr.sprite == null || cam == null) return;

        // Center on camera XY
        Vector3 camPos = cam.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, 0f);

        // Remet l'échelle par défaut puis calcule l'échelle indépendante X/Y
        transform.localScale = Vector3.one;

        // Taille sprite en unités world (après PPU)
        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        // Taille visible par la caméra en units
        float worldScreenHeight = cam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * Screen.width / (float)Screen.height;

        // Échelle indépendante : on accepte la déformation
        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}

