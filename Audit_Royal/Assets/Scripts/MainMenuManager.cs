using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Gère le menu principal du jeu.
/// Tire au sort un scénario disponible, initialise le GameStateManager et lance la scène de jeu.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary>
    /// Bouton pour commencer le jeu.
    /// </summary>
    public Button boutonJouer;

    /// <summary>
    /// Bouton pour quitter le jeu.
    /// </summary>
    public Button boutonQuitter;
    
    /// <summary>
    /// Liste des scénarios disponibles pour le tirage au sort.
    /// </summary>
    public int[] scenariosDisponibles = { 1, 2 };
    
    /// <summary>
    /// Référence au ScenarioManager, utilisé pour générer les fichiers de vérités.
    /// </summary>
    private ScenarioManager scenarioManager;
    
    /// <summary>
    /// Méthode appelée au démarrage. Initialise le ScenarioManager et configure les boutons.
    /// </summary>
    void Start()
    {
        // Initialisation du ScenarioManager
        scenarioManager = FindFirstObjectByType<ScenarioManager>();
        if (scenarioManager == null)
        {
            GameObject go = new GameObject("ScenarioManager");
            scenarioManager = go.AddComponent<ScenarioManager>();
        }
        
        // Configurer les boutons
        if (boutonJouer != null) {
            boutonJouer.onClick.AddListener(CommencerNouvelAudit);
		}
        
        if (boutonQuitter != null) {
            boutonQuitter.onClick.AddListener(QuitterJeu);
		}
       
  
    }
    
    /// <summary>
    /// Tire au sort un scénario et commence le jeu.
    /// Initialise le GameStateManager et génère le fichier de vérités pour le niveau 1.
    /// </summary>
    void CommencerNouvelAudit()
    {
        // Tirer au sort un scénario
        int indexAleatoire = Random.Range(0, scenariosDisponibles.Length);
        int scenarioChoisi = scenariosDisponibles[indexAleatoire];
        
        Debug.Log($"Nouveau scénario tiré au sort : {scenarioChoisi}");
        
        // Initialiser le GameStateManager si nécessaire
        if (GameStateManager.Instance == null)
        {
            GameObject go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
        }
        
        // Réinitialiser au niveau 1
        GameStateManager.Instance.DefinirScenarioEtNiveau(scenarioChoisi, 1);
        
        // Générer le fichier de vérités pour le niveau 1
        scenarioManager.GenerateVeritesFile(scenarioChoisi, 1);
        
        // Charger le titre du scénario pour l'affichage
        string titre = ChargerTitreScenario(scenarioChoisi);
        Debug.Log($"Lancement du jeu - {titre} - Niveau 1");
        
        // Charger la scène de jeu
        SceneManager.LoadScene("Couloir");
    }
    
    /// <summary>
    /// Quitte le jeu. Fonctionne également en mode éditeur.
    /// </summary>
    void QuitterJeu()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    /// <summary>
    /// Charge le titre d'un scénario depuis le fichier JSON correspondant.
    /// </summary>
    /// <param name="numeroScenario">Numéro du scénario à charger.</param>
    /// <returns>Le titre du scénario si trouvé, sinon une chaîne par défaut.</returns>
    string ChargerTitreScenario(int numeroScenario)
    {
        string nomFichier = $"scenario{numeroScenario}.json";
        string filePath = Path.Combine(Application.streamingAssetsPath, nomFichier);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fichier scénario introuvable : {filePath}");
            return $"Scénario {numeroScenario}";
        }
        
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            ScenarioRoot scenarioData = JsonConvert.DeserializeObject<ScenarioRoot>(jsonContent);
            return scenarioData.titre;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors du chargement du scénario : {e.Message}");
            return $"Scénario {numeroScenario}";
        }
    }
}