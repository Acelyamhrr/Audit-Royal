using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

/// Gère l'affichage du dialogue avec un personnage
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
    private Dictionary<string, string> questionsIdMap = new Dictionary<string, string>();
    private bool carnetOuvert = false;
    
    void Start()
    {
        // recup ou créer le DialogueManager
        dialogueManager = FindFirstObjectByType<JsonDialogueManager>();
        if (dialogueManager == null)
        {
            GameObject go = new GameObject("DialogueManager");
            dialogueManager = go.AddComponent<JsonDialogueManager>();
        }
        
        // config des boutons
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
        
        // au début tt est caché sauf le ptn
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
    
    /// Charge les données du personnage depuis GameStateManager
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
            // Affiche dans le panel dialogue
            if (nomPersonnageDialogue != null)
                nomPersonnageDialogue.text = $"{personnageActuel.prenom} {personnageActuel.nom}";
            
            if (metierDialogue != null)
                metierDialogue.text = $"{personnageActuel.metier} - Service {personnageActuel.service}";
            
            // Charge l'image par défaut donc l'emotion normal 
            ChargerImagePersonnage("normal");
            
            Debug.Log($"Personnage chargé : {personnageActuel.prenom} {personnageActuel.nom}");
        }
    }
    
    /// Charge l'image du perso avec l'émotion spécifiée
    void ChargerImagePersonnage(string emotion)
    {
        if (personnageImage == null)
        {
            Debug.LogWarning("personnageImage n'est pas assigné dans l'inspecteur !");
            return;
        }
        
        string fichierPersonnage = GameStateManager.Instance.FichierPersonnageActuel;
        
        // recup le nom
        string nomSansExtension = fichierPersonnage.Replace(".json", "");
        
        string cheminImage = $"Characters/{nomSansExtension}_{emotion}";
        
        // Charge l'image depuis Resources
        Sprite sprite = Resources.Load<Sprite>(cheminImage);
        
        if (sprite != null)
        {
            personnageImage.sprite = sprite;
            Debug.Log($"Image chargée : {cheminImage}");
        }
        else
        {
            Debug.LogWarning($"Image introuvable : {cheminImage}");
            
            // charge l'image normal si pas trouvé pour éviter de crash
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
    
    /// Prépare les questions dispo depuis le fichier scenario_verites.json
    void PreparerQuestions()
    {
        if (GameStateManager.Instance == null || personnageActuel == null) {
            return;
		}
        
        questionsDisponibles.Clear();
        questionsIdMap.Clear();
        
        // charge les vérités pour savoir quelles questions sont dispo
        string filePath = Path.Combine(Application.persistentDataPath, "GameData", "scenario_verites.json");
        
        if (!File.Exists(filePath))
        {
            Debug.LogError("Fichier vérités introuvable");
            return;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        VeritesScenarioRoot verites = JsonConvert.DeserializeObject<VeritesScenarioRoot>(jsonContent);
        
        // Charger le scénario pour recup la liste complete des qu.
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
    
  		  	if (estServiceAudite && scenarioData.questions.service_audite != null)
   		 	{
   		     	listeQuestionsScenario = scenarioData.questions.service_audite.liste;
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

        
        // recup les id des questions dispo depuis les vérités
        if (verites.verites.ContainsKey(personnageActuel.service))
        {
		    Debug.Log($"Service trouvé : {personnageActuel.service}");

            var serviceVerites = verites.verites[personnageActuel.service];
            
            if (serviceVerites.postes.ContainsKey(personnageActuel.metier))
            {
				Debug.Log($"Métier trouvé : {personnageActuel.metier}");

                var posteVerites = serviceVerites.postes[personnageActuel.metier];
                
		        Debug.Log("Liste des IDs trouvés dans posteVerites.verites :");

                // Les clés du dictionnaire verites sont les id des questions disponibles dcp
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
    
    /// Charge le scénario depuis le fichier JSON
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
    
    /// Affiche les questions dans le carnet
    void AfficherQuestionsCarnet()
    {
        if (questionsText == null)
            return;
        
        string texte = "Questions disponibles :\n\n";
        
        if (questionsDisponibles.Count == 0)
        {
            texte += "Aucune question dispo pour ce personnage à ce niveau.\n\n";
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
    
    /// Ouvre le carnet de questions
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
    
    /// Ferme le carnet de questions
    void FermerCarnet()
    {
        if (panelQuestions != null)
        {
            panelQuestions.SetActive(false);
            carnetOuvert = false;
        }
    }
    
    // Gère les inputs clavier
    void GererInputs()
    {
        // espace pour fermer le panel de dialogue
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
        
        //touches 1-9 pour poser une question
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
    
    /// Pose une question au personnage et affiche la réponse
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
        
        // Obtient le dialogue ET l'émotion pour afficher l'image par rap a ca
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
    
    // Retour vers la scène du bat
    void RetourBatiment()
    {
        SceneManager.LoadScene("InteriorScene");
    }
}