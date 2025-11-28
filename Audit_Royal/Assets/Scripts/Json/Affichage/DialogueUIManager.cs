using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// Gère l'affichage du dialogue avec un personnage
/// À attacher sur une scène de personnage (DirectorCom, Chef, etc.)
/// </summary>
public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References - Image")]
    public Image personnageImage;
    
    [Header("UI References - Carnet Questions")]
    public GameObject panelQuestions;
    public TextMeshProUGUI questionsText;
    public Button carnetButton;
    public Button boutonFermerCarnet;
    
    [Header("UI References - Panel Dialogue")]
    public GameObject panelDialogue;
    public TextMeshProUGUI nomPersonnageDialogue;
    public TextMeshProUGUI metierDialogue;
    public TextMeshProUGUI dialogueReponse;
    
    [Header("Boutons")]
    public Button retourButton;
    
    private JsonDialogueManager dialogueManager;
    private PlayerData personnageActuel;
    private List<string> questionsDisponibles = new List<string>();
    private Dictionary<string, string> questionsIdMap = new Dictionary<string, string>(); // Texte → ID
    private bool carnetOuvert = false;
    
    void Start()
    {
        // Récupérer ou créer le DialogueManager
        dialogueManager = FindFirstObjectByType<JsonDialogueManager>();
        if (dialogueManager == null)
        {
            GameObject go = new GameObject("DialogueManager");
            dialogueManager = go.AddComponent<JsonDialogueManager>();
        }
        
        // Configuration des boutons
        if (retourButton != null)
        {
            retourButton.onClick.AddListener(RetourBatiment);
        }
        
        if (carnetButton != null)
        {
            carnetButton.onClick.AddListener(OuvrirCarnet);
        }
        
        if (boutonFermerCarnet != null)
        {
            boutonFermerCarnet.onClick.AddListener(FermerCarnet);
        }
        
        // État initial : tout est caché sauf le carnet button
        if (panelQuestions != null)
            panelQuestions.SetActive(false);
        
        if (panelDialogue != null)
            panelDialogue.SetActive(false);
        
        ChargerPersonnage();
        PreparerQuestions();
    }
    
    void Update()
    {
        GererInputs();
    }
    
    /// <summary>
    /// Charge les données du personnage depuis GameStateManager
    /// </summary>
    void ChargerPersonnage()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager introuvable !");
            return;
        }
        
        string fichierPersonnage = GameStateManager.Instance.FichierPersonnageActuel;
        
        if (string.IsNullOrEmpty(fichierPersonnage))
        {
            Debug.LogError("Aucun personnage sélectionné !");
            return;
        }
        
        personnageActuel = dialogueManager.ObtenirInfosPersonnage(fichierPersonnage);
        
        if (personnageActuel != null)
        {
            // Affiche dans le panel dialogue (même si caché pour l'instant)
            if (nomPersonnageDialogue != null)
                nomPersonnageDialogue.text = $"{personnageActuel.prenom} {personnageActuel.nom}";
            
            if (metierDialogue != null)
                metierDialogue.text = $"{personnageActuel.metier} - Service {personnageActuel.service}";
            
            // Charge l'image par défaut (émotion normale)
            ChargerImagePersonnage("normal");
            
            Debug.Log($"Personnage chargé : {personnageActuel.prenom} {personnageActuel.nom}");
        }
    }
    
    /// <summary>
    /// Charge l'image du personnage avec l'émotion spécifiée
    /// </summary>
    void ChargerImagePersonnage(string emotion)
    {
        if (personnageImage == null)
        {
            Debug.LogWarning("personnageImage n'est pas assigné dans l'inspecteur !");
            return;
        }
        
        string fichierPersonnage = GameStateManager.Instance.FichierPersonnageActuel;
        
        // Retire l'extension .json du nom de fichier
        string nomSansExtension = fichierPersonnage.Replace(".json", "");
        
        // Construit le chemin vers l'image
        string cheminImage = $"Characters/{nomSansExtension}_{emotion}";
        
        // Charge le sprite depuis Resources
        Sprite sprite = Resources.Load<Sprite>(cheminImage);
        
        if (sprite != null)
        {
            personnageImage.sprite = sprite;
            Debug.Log($"Image chargée : {cheminImage}");
        }
        else
        {
            Debug.LogWarning($"Image introuvable : {cheminImage}");
            
            // Essaie de charger l'image "normal" en fallback
            if (emotion != "normal")
            {
                string cheminFallback = $"Characters/{nomSansExtension}_normal";
                Sprite spriteFallback = Resources.Load<Sprite>(cheminFallback);
                
                if (spriteFallback != null)
                {
                    personnageImage.sprite = spriteFallback;
                    Debug.Log($"Fallback vers image normale : {cheminFallback}");
                }
            }
        }
    }
    
    /// <summary>
    /// Prépare les questions disponibles depuis le fichier scenario_verites.json
    /// </summary>
    void PreparerQuestions()
    {
        if (GameStateManager.Instance == null || personnageActuel == null)
            return;
        
        questionsDisponibles.Clear();
        questionsIdMap.Clear();
        
        // Charger les vérités pour savoir quelles questions sont disponibles
        string filePath = Path.Combine(Application.persistentDataPath, "GameData", "scenario_verites.json");
        
        if (!File.Exists(filePath))
        {
            Debug.LogError("Fichier vérités introuvable");
            return;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        VeritesScenarioRoot verites = JsonConvert.DeserializeObject<VeritesScenarioRoot>(jsonContent);
        
        // Charger le scénario pour récupérer la liste complète des questions
        int scenario = GameStateManager.Instance.ScenarioActuel;
        ScenarioRoot scenarioData = ChargerScenario(scenario);
        
        if (scenarioData == null)
        {
            Debug.LogError("Impossible de charger le scénario");
            return;
        }
        
        // Déterminer si c'est le service audité ou un autre service
        bool estServiceAudite = personnageActuel.service.Trim().ToLower() == scenarioData.service_audite.Trim().ToLower();
        
        List<string> listeQuestionsScenario = null;
        
        if (estServiceAudite && scenarioData.questions.service_technicien != null)
        {
            listeQuestionsScenario = scenarioData.questions.service_technicien.liste;
        }
        else if (!estServiceAudite && scenarioData.questions.autres_services != null)
        {
            listeQuestionsScenario = scenarioData.questions.autres_services.liste;
        }
        
        if (listeQuestionsScenario == null)
        {
            Debug.LogError("Liste questions scénario est null!");
            return;
        }
        
        // Récupérer les IDs des questions disponibles depuis les vérités
        if (verites.verites.ContainsKey(personnageActuel.service))
        {
            var serviceVerites = verites.verites[personnageActuel.service];
            
            if (serviceVerites.postes.ContainsKey(personnageActuel.metier))
            {
                var posteVerites = serviceVerites.postes[personnageActuel.metier];
                
                // Les clés du dictionnaire verites sont les IDs des questions disponibles
                foreach (string questionId in posteVerites.verites.Keys)
                {
                    int index = int.Parse(questionId);
                    if (index < listeQuestionsScenario.Count)
                    {
                        string texteQuestion = listeQuestionsScenario[index];
                        questionsDisponibles.Add(texteQuestion);
                        questionsIdMap[texteQuestion] = questionId;
                    }
                }
            }
        }
        
        Debug.Log($"Total questions disponibles: {questionsDisponibles.Count}");
        AfficherQuestionsCarnet();
    }
    
    /// <summary>
    /// Charge le scénario depuis le fichier JSON
    /// </summary>
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
    
    /// <summary>
    /// Affiche les questions dans le carnet
    /// </summary>
    void AfficherQuestionsCarnet()
    {
        if (questionsText == null)
            return;
        
        string texte = "📋 Questions disponibles :\n\n";
        
        if (questionsDisponibles.Count == 0)
        {
            texte += "Aucune question disponible pour ce personnage à ce niveau.\n\n";
        }
        else
        {
            for (int i = 0; i < questionsDisponibles.Count; i++)
            {
                texte += $"[{i + 1}] {questionsDisponibles[i]}\n\n";
            }
        }
        
        texte += "\n[R] Retour au bâtiment";
        
        questionsText.text = texte;
    }
    
    /// <summary>
    /// Ouvre le carnet de questions
    /// </summary>
    void OuvrirCarnet()
    {
        if (panelQuestions != null)
        {
            panelQuestions.SetActive(true);
            carnetOuvert = true;
        }
        
        // Ferme le dialogue si ouvert
        if (panelDialogue != null)
            panelDialogue.SetActive(false);
    }
    
    /// <summary>
    /// Ferme le carnet de questions
    /// </summary>
    void FermerCarnet()
    {
        if (panelQuestions != null)
        {
            panelQuestions.SetActive(false);
            carnetOuvert = false;
        }
    }
    
    /// <summary>
    /// Gère les inputs clavier
    /// </summary>
    void GererInputs()
    {
        // Si le dialogue est affiché : Espace pour fermer
        if (panelDialogue != null && panelDialogue.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                panelDialogue.SetActive(false);
                // Réouvre le carnet
                if (panelQuestions != null)
                {
                    panelQuestions.SetActive(true);
                    carnetOuvert = true;
                }
            }
            return;
        }
        
        // Si le carnet est ouvert : touches 1-9 pour poser une question
        if (carnetOuvert)
        {
            for (int i = 0; i < Mathf.Min(questionsDisponibles.Count, 9); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    PoserQuestion(i);
                    return;
                }
            }
            
            // R pour retour
            if (Input.GetKeyDown(KeyCode.R))
            {
                RetourBatiment();
            }
        }
    }
    
    /// <summary>
    /// Pose une question au personnage et affiche la réponse
    /// </summary>
    void PoserQuestion(int indexQuestion)
    {
        if (GameStateManager.Instance == null || personnageActuel == null)
            return;
        
        if (indexQuestion < 0 || indexQuestion >= questionsDisponibles.Count)
        {
            Debug.LogError($"Index de question invalide : {indexQuestion}");
            return;
        }
        
        string texteQuestion = questionsDisponibles[indexQuestion];
        string numeroQuestion = questionsIdMap[texteQuestion];
        
        int scenario = GameStateManager.Instance.ScenarioActuel;
        string fichierPersonnage = GameStateManager.Instance.FichierPersonnageActuel;
        
        // Obtient le dialogue ET l'émotion
        (string reponse, string emotion) = dialogueManager.ObtenirDialogueAvecEmotion(
            scenario,
            fichierPersonnage,
            numeroQuestion
        );
        
        // Charge l'image avec l'émotion correspondante
        ChargerImagePersonnage(emotion);
        
        // Ferme le carnet
        if (panelQuestions != null)
            panelQuestions.SetActive(false);
        
        carnetOuvert = false;
        
        // Affiche le dialogue
        if (panelDialogue != null)
        {
            panelDialogue.SetActive(true);
            
            if (dialogueReponse != null)
            {
                dialogueReponse.text = $"\"{reponse}\"\n\n[Espace] Continuer";
            }
        }
        
        Debug.Log($"Question {numeroQuestion} posée → Réponse : {reponse} (Émotion: {emotion})");
    }
    
    /// <summary>
    /// Retour vers la scène du bâtiment
    /// </summary>
    void RetourBatiment()
    {
        SceneManager.LoadScene("InteriorScene");
    }
}