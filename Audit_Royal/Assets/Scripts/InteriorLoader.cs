using UnityEngine;
using System.Collections;

public class InteriorLoader : MonoBehaviour
{
    [Tooltip("Parent sous lequel instancier le prefab (vide ou un Canvas).")]
    public Transform container;

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

        // Recherche du prefab
        foreach (var prefab in servicePrefabs)
        {
            if (prefab.name.Equals(service, System.StringComparison.OrdinalIgnoreCase))
            {
                Instantiate(prefab, container != null ? container : null);
                Debug.Log($"Prefab '{prefab.name}' instancié pour le service '{service}'");
                yield break;
            }
        }

        Debug.LogError($"Prefab pour le service '{service}' introuvable. Prefabs disponibles : {string.Join(", ", System.Array.ConvertAll(servicePrefabs, p => p.name))}");
    }
}