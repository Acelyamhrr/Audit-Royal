using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundStretchToCamera : MonoBehaviour
{
    SpriteRenderer sr;
    Camera cam;
    Vector2 lastScreen = Vector2.zero;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        if (cam == null) cam = Camera.current;
    }

    void Start()
    {
        Stretch();
    }

    void Update()
    {
        if (Screen.width != lastScreen.x || Screen.height != lastScreen.y)
        {
            Stretch();
            lastScreen = new Vector2(Screen.width, Screen.height);
        }
    }

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

