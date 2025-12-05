using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class WorldCanvasFitToCamera : MonoBehaviour
{
    public Camera targetCamera; // assign MainCamera or leave empty to use Camera.main
    public float planeDistance = 5f; // distance devant la caméra
    public float pixelsPerUnit = 100f; // correspond au PPU de tes sprites si tu veux 1 unité = 100px

    RectTransform rt;
    Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        rt = GetComponent<RectTransform>();
        if (targetCamera == null) targetCamera = Camera.main;
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = targetCamera;
    }

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

