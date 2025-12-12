using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;

public class MapUIManager : MonoBehaviour
{
    public TextMeshProUGUI texteMission;
    public Button boutonTerminerNiveau;
    public GameObject panelFinNiveau;
    public TextMeshProUGUI texteFinNiveau;
    public Button boutonConfirmerFin;
    public Button boutonAnnuler;
    
    private ScenarioManager scenarioManager;
    
    void Start()
    {
        scenarioManager = FindFirstObjectByType<ScenarioManager>();
        if (scenarioManager == null)
        {
            GameObject go = new GameObject("ScenarioManager");
            scenarioManager = go.AddComponent<ScenarioManager>();
        }
        
        AfficherMission();
        ConfigurerBoutons();
        
        if (GameStateManager.Instance.DoTerminerNiveauApresRapport)
        {
            GameStateManager.Instance.DoTerminerNiveauApresRapport = false;
            TerminerNiveau();
        }
        
        if (panelFinNiveau != null)
            panelFinNiveau.SetActive(false);
    }
    
    void ConfigurerBoutons()
    {
        if (boutonTerminerNiveau != null)
            boutonTerminerNiveau.onClick.AddListener(AfficherConfirmationFin);
        
        if (boutonConfirmerFin != null)
            boutonConfirmerFin.onClick.AddListener(PasserRapport);
        
        if (boutonAnnuler != null)
            boutonAnnuler.onClick.AddListener(AnnulerFin);
    }
    
    void AfficherMission()
    {
        if (texteMission == null)
        {
            Debug.LogWarning("texteMission non assigné dans MapUIManager !");
            return;
        }
        
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager introuvable !");
            texteMission.text = "Chargement...";
            return;
        }
        
        int niveau = GameStateManager.Instance.NiveauActuel;
        int scenario = GameStateManager.Instance.ScenarioActuel;
        
        // recup le service audité depuis le fichier scenario.json
        ScenarioRoot scenarioData = ChargerScenario(scenario);
        
        if (scenarioData == null)
        {
            texteMission.text = "Erreur de chargement du scénario";
            return;
        }
        
        string message = GenererMessageMission(niveau, scenarioData);
        texteMission.text = message;
        
        Debug.Log($"Mission affichée - Niveau {niveau} : {message}");
    }
    
    string GenererMessageMission(int niveau, ScenarioRoot scenarioData)
    {
        string serviceNom = scenarioData.service_audite.ToUpper();
        string titre = scenarioData.titre;
        
        string message = $"{titre} - NIVEAU {niveau}\n\n";
        
        switch (niveau)
        {
            case 1:
                message += $"Mission : Auditez le service {serviceNom}.\n";
                message += $"Posez la question qui vous est assignée à chaque poste.";
                break;
            
            case 2:
                message += $"Mission : Auditez le service {serviceNom}.\n";
                message += $"Interrogez les différents postes avec les questions disponibles.";
                break;
            
            case 3:
                message += $"Mission : Service principal - {serviceNom}.\n";
                message += $"Vous pouvez aussi interroger les autres services pour obtenir plus d'informations.";
                break;
            
            case 4:
                message += $"Mission : Auditez tous les services disponibles.\n";
                message += $"Croisez les informations pour établir la vérité.";
                break;
            
            case 5:
                message += $"Mission finale : Auditez tous les services.\n";
                message += $"Toutes les questions sont disponibles. Préparez votre rapport complet.";
                break;
            
            default:
                message += $"Explorez la carte et interrogez les personnages.";
                break;
        }
        
        return message;
    }
    
    ScenarioRoot ChargerScenario(int numeroScenario)
    {
        string nomFichier = $"scenario{numeroScenario}.json";
        string filePath = Path.Combine(Application.streamingAssetsPath, nomFichier);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fichier scénario introuvable : {filePath}");
            return null;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            ScenarioRoot scenarioData = JsonConvert.DeserializeObject<ScenarioRoot>(jsonContent);
            return scenarioData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors du chargement du scénario : {e.Message}");
            return null;
        }
    }
    
    // Affiche le panel de confirmation de fin de niveau
    void AfficherConfirmationFin()
    {
        if (panelFinNiveau == null)
        {
            // Si pas de panel, terminer directement
            TerminerNiveau();
            return;
        }
        
        int niveauActuel = GameStateManager.Instance.NiveauActuel;
        
        if (texteFinNiveau != null)
        {
            if (niveauActuel < 5)
            {
                texteFinNiveau.text = $"Terminer le niveau {niveauActuel} et passer au rapport ?\n\n" +
                                     $"Vous passerez au niveau {niveauActuel + 1}.";
            }
            else
            {
                texteFinNiveau.text = $"🎉 Félicitations !\n\n" +
                                     $"Vous avez terminé tous les niveaux de ce scénario !\n\n" +
                                     $"Retour au menu principal ?";
            }
        }
        
        panelFinNiveau.SetActive(true);
    }
    
    void AnnulerFin()
    {
        if (panelFinNiveau != null)
            panelFinNiveau.SetActive(false);
    }

    void PasserRapport()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Rapport");
    }
    
    void TerminerNiveau()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager introuvable !");
            return;
        }
        
        int niveauActuel = GameStateManager.Instance.NiveauActuel;
        int scenarioActuel = GameStateManager.Instance.ScenarioActuel;
        
        if (niveauActuel < 5)
        {
            // Passer au niveau suivant
            int nouveauNiveau = niveauActuel + 1;
            GameStateManager.Instance.DefinirScenarioEtNiveau(scenarioActuel, nouveauNiveau);
            
            Debug.Log($"Passage au niveau {nouveauNiveau}");
            
            // Générer le nouveau fichier de vérités
            scenarioManager.GenerateVeritesFile(scenarioActuel, nouveauNiveau);
            
            // Recharger la mission
            AfficherMission();
            
            // Cacher le panel
            if (panelFinNiveau != null)
                panelFinNiveau.SetActive(false);
        }
        else
        {
            // Tous les niveaux terminés : retour au menu
            Debug.Log("Tous les niveaux terminés ! Retour au menu principal.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}