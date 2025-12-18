using UnityEngine;

/// <summary>
/// Déclenche un changement de scène lorsque le joueur s’approche suffisamment de la porte du couloir (début du jeu).
/// Utilise une transition avec effet de fondu.
/// </summary>
public class DoorTrigger2D : MonoBehaviour
{
    /// <summary>
    /// Transform du joueur utilisé pour calculer la distance avec la porte.
    /// </summary>
    public Transform player;
    
    /// <summary>
    /// Nom de la scène à charger après le déclenchement.
    /// </summary>
    public string nextScene = "Bureau";
    
    /// <summary>
    /// Distance minimale entre le joueur et la porte pour déclencher la transition.
    /// </summary>
    public float triggerDistance = 50f;

    /// <summary>
    /// Référence au gestionnaire de transition de scène.
    /// </summary>
    private SceneTransition transition;
    
    /// <summary>
    /// Empêche le déclenchement multiple de la transition.
    /// </summary>
    private bool hasTriggered = false;

    /// <summary>
    /// Recherche automatiquement le composant SceneTransition présent dans la scène.
    /// </summary>
    void Start()
    {
        transition = FindFirstObjectByType<SceneTransition>();
    }

    /// <summary>
    /// Vérifie à chaque frame la distance entre le joueur et la porte
    /// et déclenche la transition si la distance est inférieure au seuil défini.
    /// </summary>
    void Update()
    {
        if (hasTriggered) return;

        float distance = Vector2.Distance(player.position, transform.position);
        if (distance < triggerDistance)
        {
            hasTriggered = true;
            transition.FadeAndLoadScene(nextScene);
        }
    }
}
