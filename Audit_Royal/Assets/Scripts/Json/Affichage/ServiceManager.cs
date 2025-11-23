using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

public class ServiceUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject panelService; // Panel principal du service (se montre quand on entre)
    public GameObject panelQuestions; // Panel avec les 3 questions
    public GameObject panelReponse; // Panel avec la réponse
    
    [Header("Textes personnage")]
    public TextMeshProUGUI texteNomPersonnage;
    public TextMeshProUGUI textePostePersonnage;
    
    [Header("Questions (3 boutons)")]
    public TextMeshProUGUI texteQuestion1;
    public TextMeshProUGUI texteQuestion2;
    public TextMeshProUGUI texteQuestion3;
    
    [Header("Réponse")]
    public TextMeshProUGUI texteReponse;
    
    private JsonDialogueManager dialogueManager;
    private int scenarioActuel;
    private ScenarioRoot scenarioData;
    
    private string serviceActuel;
    private string fichierPersonnageActuel;
    private PlayerData personnageActuel;
    
    private List<string> toutesLesQuestions;
    private List<int> indicesQuestions; // Les vrais indices pour le DialogueManager
    
    void Start()
    {
        dialogueManager = FindFirstObjectByType<JsonDialogueManager>();
        if (dialogueManager == null)
        {
            GameObject go = new GameObject("DialogueManager");
            dialogueManager = go.AddComponent<JsonDialogueManager>();
        }
        
        // Récupérer le scénario actuel depuis GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            scenarioActuel = gm.scenariosDisponibles[0]; // Prendre le premier par défaut
        }
        else
        {
            scenarioActuel = 1;
        }
        
        // Cacher tous les panels au départ
        CacherTout();
    }
    
    void CacherTout()
    {
        if (panelService != null) panelService.SetActive(false);
        if (panelQuestions != null) panelQuestions.SetActive(false);
        if (panelReponse != null) panelReponse.SetActive(false);
    }
    
    // Appelé quand le joueur entre dans un bâtiment
    public void ChargerService(string nomService)
    {
        serviceActuel = nomService;
        Debug.Log($"Chargement du service: {nomService}");
        
        // Charger le scénario si pas déjà fait
        ChargerScenario(scenarioActuel);
        
        // Afficher le panel du service
        if (panelService != null)
        {
            panelService.SetActive(true);
        }
    }
    
    public void CacherService()
    {
        CacherTout();
    }
    
    void ChargerScenario(int numeroScenario)
    {
        if (scenarioData != null) return; // Déjà chargé
        
        string nomFichier = $"scenario{numeroScenario}.json";
        string filePath = Path.Combine(Application.streamingAssetsPath, nomFichier);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fichier scénario introuvable : {filePath}");
            return;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        scenarioData = JsonConvert.DeserializeObject<ScenarioRoot>(jsonContent);
        
        Debug.Log($"Scénario '{scenarioData.titre}' chargé");
    }
    
    // Appelé quand on clique sur un personnage
    public void OnPersonnageClique(string fichierPersonnage)
    {
        Debug.Log($"Clic sur: {fichierPersonnage}");
        
        fichierPersonnageActuel = fichierPersonnage;
        personnageActuel = dialogueManager.ObtenirInfosPersonnage(fichierPersonnage);
        
        // Afficher nom et poste
        if (texteNomPersonnage != null)
        {
            texteNomPersonnage.text = $"{personnageActuel.prenom} {personnageActuel.nom}";
        }
        
        if (textePostePersonnage != null)
        {
            textePostePersonnage.text = personnageActuel.metier;
        }
        
        // Sélectionner 3 questions aléatoires
        SelectionnerQuestionsAleatoires();
        
        // Afficher le panel questions
        if (panelService != null) panelService.SetActive(false);
        if (panelQuestions != null) panelQuestions.SetActive(true);
        if (panelReponse != null) panelReponse.SetActive(false);
    }
    
    void SelectionnerQuestionsAleatoires()
    {
        // Déterminer la liste de questions selon le service du personnage
        if (personnageActuel.service.Trim().ToLower() == scenarioData.service_audite.Trim().ToLower())
        {
            toutesLesQuestions = scenarioData.questions.service_technicien.liste;
        }
        else
        {
            toutesLesQuestions = scenarioData.questions.autres_services.liste;
        }
        
        if (toutesLesQuestions == null || toutesLesQuestions.Count == 0)
        {
            Debug.LogError("Aucune question disponible!");
            return;
        }
        
        // Prendre 3 questions au hasard
        int nbQuestions = Mathf.Min(3, toutesLesQuestions.Count);
        indicesQuestions = new List<int>();
        
        List<int> indicesDisponibles = Enumerable.Range(0, toutesLesQuestions.Count).ToList();
        
        for (int i = 0; i < nbQuestions; i++)
        {
            int randomIndex = Random.Range(0, indicesDisponibles.Count);
            int indiceQuestion = indicesDisponibles[randomIndex];
            indicesDisponibles.RemoveAt(randomIndex);
            
            indicesQuestions.Add(indiceQuestion);
        }
        
        // Afficher les questions dans l'UI
        if (indicesQuestions.Count > 0 && texteQuestion1 != null)
        {
            texteQuestion1.text = toutesLesQuestions[indicesQuestions[0]];
        }
        
        if (indicesQuestions.Count > 1 && texteQuestion2 != null)
        {
            texteQuestion2.text = toutesLesQuestions[indicesQuestions[1]];
        }
        
        if (indicesQuestions.Count > 2 && texteQuestion3 != null)
        {
            texteQuestion3.text = toutesLesQuestions[indicesQuestions[2]];
        }
        
        Debug.Log($"Questions sélectionnées: {string.Join(", ", indicesQuestions)}");
    }
    
    // Appelé par les boutons de questions (0, 1 ou 2)
    public void OnQuestionCliquee(int indexBouton)
    {
        if (indexBouton >= indicesQuestions.Count)
        {
            Debug.LogError($"Index invalide: {indexBouton}");
            return;
        }
        
        int numeroQuestion = indicesQuestions[indexBouton];
        
        Debug.Log($"Question {numeroQuestion} posée");
        
        // Obtenir la réponse
        string reponse = dialogueManager.ObtenirDialogue(
            scenarioActuel,
            fichierPersonnageActuel,
            numeroQuestion.ToString()
        );
        
        // Afficher la réponse
        if (texteReponse != null)
        {
            texteReponse.text = $"\"{reponse}\"";
        }
        
        // Changer de panel
        if (panelQuestions != null) panelQuestions.SetActive(false);
        if (panelReponse != null) panelReponse.SetActive(true);
    }
    
    // Bouton "Continuer" après une réponse
    public void OnContinuer()
    {
        // Revenir aux questions
        if (panelReponse != null) panelReponse.SetActive(false);
        if (panelQuestions != null) panelQuestions.SetActive(true);
    }
    
    // Bouton "Retour"
    public void OnRetour()
    {
        if (panelQuestions != null) panelQuestions.SetActive(false);
        if (panelReponse != null) panelReponse.SetActive(false);
        if (panelService != null) panelService.SetActive(true);
    }
}
