using UnityEngine;

/// <summary>
/// Déclenche une transition avec fondu vers une autre scène
/// lorsque le joueur entre dans une zone de collision.
/// </summary>
public class TriggerFadeDoor : MonoBehaviour
{
    /// <summary>
    /// Référence vers le script gérant le fondu et le changement de scène.
    /// </summary>
    public SceneTransition fadeScript;
    
    /// <summary>
    /// Nom de la scène à charger après le fondu.
    /// </summary>
    public string sceneToLoad = "Bureau";

    /// <summary>
    /// Empêche le déclenchement multiple du changement de scène.
    /// </summary>
    private bool triggered = false;

    /// <summary>
    /// Détecte l’entrée d’un objet dans la zone de collision.
    /// Si l’objet est le joueur, lance la transition vers la scène cible.
    /// </summary>
    /// <param name="other">Collider de l’objet entrant dans le trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return; // pour éviter plusieurs déclenchements
        if (other.CompareTag("Player"))
        {
            triggered = true;
            fadeScript.FadeAndLoadScene(sceneToLoad);
        }
    }
}
