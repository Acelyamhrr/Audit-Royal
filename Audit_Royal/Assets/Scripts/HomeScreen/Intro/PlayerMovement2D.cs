using UnityEngine;

/// <summary>
/// Gère le déplacement 2D d’un joueur via les axes clavier (Horizontal / Vertical).
/// Le déplacement est appliqué sur un RectTransform, typiquement pour une UI ou un canvas en World Space.
/// </summary>
public class PlayerMovement2D : MonoBehaviour
{
    /// <summary>
    /// Vitesse de déplacement du joueur (en unités par seconde).
    /// </summary>
    public float speed = 200f; 
    
    /// <summary>
    /// Référence au RectTransform du joueur.
    /// </summary>
    private RectTransform rect;

    /// <summary>
    /// Initialise la référence au RectTransform du GameObject.
    /// </summary>
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Gère le déplacement du joueur à chaque frame en fonction des entrées clavier.
    /// </summary>
    void Update()
    {
        float h = Input.GetAxis("Horizontal"); 
        float v = Input.GetAxis("Vertical");   

        Vector3 move = new Vector3(h, v, 0) * speed * Time.deltaTime;
        rect.anchoredPosition += new Vector2(move.x, move.y);
    }
}
