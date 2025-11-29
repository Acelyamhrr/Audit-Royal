using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

/// Gère le menu principal
/// Tire au sort un scénario et initialise le jeu
/// À attacher sur un GameObject dans la scène MainMenu
public class MainMenuManager : MonoBehaviour
{
    public Button boutonJouer;
    public Button boutonQuitter;
    
    public int[] scenariosDisponibles = { 1, 2 };
    
    private ScenarioManager scenarioManager;
    
    void Start()
    {
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
    
    // Tire au sort un nouveau scénario et lance le jeu
    void CommencerNouvelAudit()
    {
        // Tirer au sort un scénario
        int indexAleatoire = Random.Range(0, scenariosDisponibles.Length);
        int scenarioChoisi = scenariosDisponibles[indexAleatoire];
        
        Debug.Log($"Nouveau scénario tiré au sort : {scenarioChoisi}");
        
        // Initialiser le GameStateManager avec ce scénario
        if (GameStateManager.Instance == null)
        {
            GameObject go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
        }
        
        // Réinitialiser au niveau 1
        GameStateManager.Instance.DefinirScenarioEtNiveau(scenarioChoisi, 1);
        
        // Générer le fichier de vérités pour le niveau 1
        scenarioManager.GenerateVeritesFile(scenarioChoisi, 1);
        
        string titre = ChargerTitreScenario(scenarioChoisi);
        Debug.Log($"Lancement du jeu - {titre} - Niveau 1");
        
        SceneManager.LoadScene("Couloir");
    }
    
    void QuitterJeu()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
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