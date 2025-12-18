using UnityEngine;

/// <summary>
/// Stocke et partage les données persistantes liées au joueur entre les scènes.
/// Cette classe agit comme une instance globale (Singleton)
/// </summary>
public class PlayerMovementData : MonoBehaviour
{
    /// <summary>
    /// Permet d'accéder aux données du joueur depuis n'importe quelle scène.
    /// </summary>
    public static PlayerMovementData Instance { get; private set; }

    /// <summary>
    /// Position actuelle du joueur dans le monde.
    /// Cette valeur est utilisée pour restaurer la position du joueur
    /// lors du retour dans la scène de la map.
    /// </summary>
    public Vector3 playerPosition;

    /// <summary>
    /// Méthode appelée lors de l'initialisation de l'objet.
    /// Met en place le pattern Singleton et empêche la destruction
    /// de l'objet lors du chargement d'une nouvelle scène.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Empêche la destruction de cet objet lors du changement de scène
            DontDestroyOnLoad(gameObject);
        }
    }
}
