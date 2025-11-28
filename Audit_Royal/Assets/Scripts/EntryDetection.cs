using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

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
                // Vérifier si le service est accessible selon le niveau
                if (!EstServiceAccessible(nomBatiment))
                {
                    Debug.Log($"Service {nomBatiment} non accessible pour le niveau actuel");
                    hasLoadedScene = false; // Réinitialiser pour permettre une nouvelle tentative
                    // TODO: Afficher un message à l'écran "Ce service n'est pas accessible pour ce niveau"
                    return;
                }
                
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
    
    /// <summary>
    /// Vérifie si le service est accessible selon le niveau actuel
    /// </summary>
    bool EstServiceAccessible(string nomBatiment)
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager introuvable !");
            return true; // Par sécurité, on laisse passer
        }
        
        int niveau = GameStateManager.Instance.NiveauActuel;
        int scenario = GameStateManager.Instance.ScenarioActuel;
        
        // Charger le service audité depuis le scénario
        string serviceAudite = ChargerServiceAudite(scenario);
        
        // Convertir le nom du bâtiment en nom de service
        string serviceEntree = ConvertirBatimentEnService(nomBatiment);
        
        Debug.Log($"Niveau {niveau} - Service audité: {serviceAudite} - Service demandé: {serviceEntree}");
        
        // Niveaux 1 et 2 : Seulement le service audité
        if (niveau == 1 || niveau == 2)
        {
            return serviceEntree.Equals(serviceAudite, System.StringComparison.OrdinalIgnoreCase);
        }
        
        // Niveaux 3, 4, 5 : Tous les services accessibles
        return true;
    }
    
    /// <summary>
    /// Convertit le nom du bâtiment en identifiant de service
    /// </summary>
    string ConvertirBatimentEnService(string nomBatiment)
    {
        switch (nomBatiment)
        {
            case "Crous": return "restauration";
            case "Informatique": return "info";
            case "Compta": return "comptabilite";
            case "Communication": return "communication";
            case "Techniciens": return "technicien";
            default: return nomBatiment.ToLower();
        }
    }
    
    /// <summary>
    /// Charge le service audité depuis le fichier scenario.json
    /// </summary>
    string ChargerServiceAudite(int numeroScenario)
    {
        string nomFichier = $"scenario{numeroScenario}.json";
        string filePath = Path.Combine(Application.streamingAssetsPath, nomFichier);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fichier scénario introuvable : {filePath}");
            return "";
        }
        
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            ScenarioRoot scenarioData = JsonConvert.DeserializeObject<ScenarioRoot>(jsonContent);
            return scenarioData.service_audite;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors du chargement du service audité : {e.Message}");
            return "";
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