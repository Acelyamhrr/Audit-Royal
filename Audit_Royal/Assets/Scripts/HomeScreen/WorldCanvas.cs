using UnityEngine;

/// <summary>
/// Ajuste automatiquement un Canvas en World Space pour qu'il remplisse la vue d'une caméra.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class WorldCanvasFitToCamera : MonoBehaviour
{
    /// <summary>
    /// Caméra à laquelle le Canvas doit être ajusté.
    /// Si null, Camera.main sera utilisée.
    /// </summary>
    public Camera targetCamera;
    
    /// <summary>
    /// Distance devant la caméra pour placer le Canvas.
    /// </summary>
    public float planeDistance = 5f;
    
    /// <summary>
    /// Pixels par unité pour le Canvas, correspondant au PPU des sprites.
    /// </summary>
    public float pixelsPerUnit = 100f;

    
    /// <summary>
    /// Référence vers le RectTranform.
    /// </summary>
    RectTransform rt;
    
    /// <summary>
    /// Référence vers le Canvas.
    /// </summary>
    Canvas canvas;

    /// <summary>
    /// Initialise le Canvas et le RectTransform.
    /// </summary>
    void Awake()
    {
        canvas = GetComponent<Canvas>();
        rt = GetComponent<RectTransform>();
        if (targetCamera == null) targetCamera = Camera.main;
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = targetCamera;
    }

    /// <summary>
    /// Met à jour la taille et la position du Canvas pour correspondre à la vue de la caméra.
    /// </summary>
    void Update()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || rt == null) return;

        // Calcul taille visible par la caméra en unités world
        float worldScreenHeight = targetCamera.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * Screen.width / (float)Screen.height;

        // Définir la taille du RectTransform en pixels (sizeDelta)
        rt.sizeDelta = new Vector2(worldScreenWidth * pixelsPerUnit, worldScreenHeight * pixelsPerUnit);

        // Positionner le canvas devant la caméra
        Vector3 camPos = targetCamera.transform.position;
        Vector3 forward = targetCamera.transform.forward;
        transform.position = camPos + forward * planeDistance;
        transform.rotation = targetCamera.transform.rotation;
        transform.localScale = Vector3.one / pixelsPerUnit; // ajuste l'échelle pour que 1 unité world = pixelsPerUnit px
    }
}

