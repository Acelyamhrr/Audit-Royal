using UnityEngine;
using System.Collections;

public class InteriorLoader : MonoBehaviour
{
    public GameObject[] servicePrefabs;

    void Start()
    {
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

    IEnumerator WaitAndLoadService()		// ienumerator c'est genre, on attend un moment pour etre sur que ca charge, et ensuite on fait ...
    {
		// debug
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