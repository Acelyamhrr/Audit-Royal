using UnityEngine;
using System.Collections;

/// <summary>
/// Charge dynamiquement le prefab du service correspondant
/// Chaque prefab contient son propre Canvas et gère son affichage
/// </summary>
public class InteriorLoader : MonoBehaviour
{
    [Tooltip("Liste de prefabs de services (nom du prefab doit être l'identifiant du service).")]
    public GameObject[] servicePrefabs;

    void Start()
    {
        // Créer le GameStateManager s'il n'existe pas
        EnsureGameStateManagerExists();
        
        StartCoroutine(WaitAndLoadService());
    }

    void EnsureGameStateManagerExists()
    {
        if (GameStateManager.Instance == null)
        {
            GameObject go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
            Debug.LogWarning("GameStateManager créé par InteriorLoader (mode test)");
        }
    }

    IEnumerator WaitAndLoadService()
    {
        // Petite attente pour s'assurer que le GameStateManager est initialisé
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
                // Instancier directement sans parent
                // Le Canvas du prefab gère l'affichage et le responsive
                Instantiate(prefab);
                Debug.Log($"Prefab '{prefab.name}' instancié pour le service '{service}'");
                yield break;
            }
        }

        Debug.LogError($"Prefab pour le service '{service}' introuvable. " +
                      $"Prefabs disponibles : {string.Join(", ", System.Array.ConvertAll(servicePrefabs, p => p.name))}");
    }
}