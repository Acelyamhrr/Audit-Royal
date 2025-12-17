using UnityEngine;
using System.Collections;

/// <summary>
/// Charge dynamiquement le prefab du service correspondant au joueur lorsqu'il entre dans un bâtiment.
/// </summary>
public class InteriorLoader : MonoBehaviour
{
    /// <summary>
    /// Liste des prefabs correspondant aux différents services.
    /// Le nom du prefab doit correspondre à l'identifiant du service.
    /// </summary>
    public GameObject[] servicePrefabs;

    /// <summary>
    /// Méthode appelée au démarrage. Vérifie l'existence du GameStateManager et lance le chargement du service.
    /// </summary>
    void Start()
    {
        EnsureGameStateManagerExists();
        
        StartCoroutine(WaitAndLoadService());
    }

    /// <summary>
    /// Vérifie si une instance de <see cref="GameStateManager"/> existe, sinon en crée une pour les tests.
    /// </summary>
    void EnsureGameStateManagerExists()
    {
        if (GameStateManager.Instance == null)
        {
            GameObject go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
            Debug.LogWarning("GameStateManager créé par InteriorLoader (mode test)");
        }
    }

    /// <summary>
    /// Coroutine qui attend un frame pour s'assurer que tout est initialisé, puis instancie le prefab correspondant au service actuel.
    /// </summary>
    /// <returns>IEnumerator pour l'exécution en coroutine.</returns>
    IEnumerator WaitAndLoadService()
    {
        // Attente d'un frame pour s'assurer que GameStateManager est initialisé
        yield return null;

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager toujours introuvable après création !");
            yield break;
        }

        string service = GameStateManager.Instance.ServiceActuel;
        
        if (string.IsNullOrEmpty(service))
        {
            Debug.LogWarning("ServiceActuel vide - utilisation du service par défaut pour les tests");
            service = "info"; // Service par défaut pour les tests
        }

        // Recherche du prefab correspondant au service
        foreach (var prefab in servicePrefabs)
        {
            if (prefab.name.Equals(service, System.StringComparison.OrdinalIgnoreCase))
            {
                Instantiate(prefab);
                Debug.Log($"Prefab '{prefab.name}' instancié pour le service '{service}'");
                yield break;
            }
        }

        Debug.LogError($"Prefab pour le service '{service}' introuvable. " +
                      $"Prefabs disponibles : {string.Join(", ", System.Array.ConvertAll(servicePrefabs, p => p.name))}");
    }
}