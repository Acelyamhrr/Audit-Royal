using UnityEngine;
using UnityEngine.SceneManagement;

public class EntryDetection : MonoBehaviour
{
    private Collider2D zoneCollider;
    private Collider2D playerCollider;

    private bool hasLoadedScene = false; 
    private string nomBatiment = "";

    private void Start()
    {
        zoneCollider = GetComponent<Collider2D>();
        Debug.Log($"EntryDetection initialisé sur : {gameObject.name}");
    }

    private void Update()
    {
        if (!hasLoadedScene && playerCollider != null && zoneCollider.OverlapPoint(playerCollider.bounds.center))
        {
            hasLoadedScene = true;
            Debug.Log($"Joueur détecté dans la zone : {gameObject.name}");

            // Détermine quel bâtiment on entre
            switch (gameObject.name)
            {
                case "EntryZoneCrous":
                    nomBatiment = "Crous";
                    break;

                case "EntryZoneInfo":
                    nomBatiment = "Informatique";
                    break;

                case "EntryZoneCompta":
                    nomBatiment = "Compta";
                    break;

                case "EntryZoneCom":
                    nomBatiment = "Communication";
                    break;

                case "EntryZoneBTP":
                    nomBatiment = "Techniciens";
                    break;
                    
                default:
                    Debug.LogError($"Nom de GameObject inconnu : {gameObject.name}");
                    break;
            }

            Debug.Log($"Bâtiment déterminé : {nomBatiment}");

            if (!string.IsNullOrEmpty(nomBatiment))
            {
                // Met à jour GameStateManager
                if (GameStateManager.Instance != null)
                {
                    Debug.Log($"Appel de EntrerDansBatiment('{nomBatiment}')");
                    GameStateManager.Instance.EntrerDansBatiment(nomBatiment);
                    Debug.Log($"ServiceActuel après appel : '{GameStateManager.Instance.ServiceActuel}'");
                }
                else
                {
                    Debug.LogError("GameStateManager.Instance est NULL !");
                }

                // On charge TOUJOURS la même scène maintenant
                Debug.Log("Chargement de InteriorScene...");
                SceneManager.LoadScene("InteriorScene");
            }
            else
            {
                Debug.LogError("nomBatiment est vide ou null !");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCollider = other;
            Debug.Log($"Player entré dans la zone : {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCollider = null;
            hasLoadedScene = false;
            Debug.Log($"Player sorti de la zone : {gameObject.name}");
        }
    }
}