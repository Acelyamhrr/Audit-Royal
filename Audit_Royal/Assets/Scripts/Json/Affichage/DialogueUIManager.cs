using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// Gère l’interface utilisateur des dialogues avec les personnages.
/// </summary>
/// <remarks>
/// Cette classe s’occupe de :
/// <list type="bullet">
/// <item><description>Charger les données du personnage sélectionné</description></item>
/// <item><description>Afficher les questions disponibles selon le scénario</description></item>
/// <item><description>Afficher les réponses et changer l’émotion du personnage</description></item>
/// <item><description>Gérer le carnet de questions et les interactions clavier</description></item>
/// </list>
/// </remarks>
public class DialogueUIManager : MonoBehaviour
{
    #region UI References

    /// <summary>
    /// Image du personnage affichée à l’écran.
    /// </summary>
    [Header("UI References - Image")]
    public Image personnageImage;
    
    /// <summary>
    /// Panel du carnet de questions.
    /// </summary>
    [Header("UI References - Carnet Questions")]
    public GameObject panelQuestions;
    
    /// <summary>
    /// Texte affichant la liste des questions disponibles.
    /// </summary>
    public TextMeshProUGUI questionsText;
    
    /// <summary>
    /// Bouton permettant d’ouvrir le carnet.
    /// </summary>
    public Button carnetButton;
    
    /// <summary>
    /// Bouton permettant de fermer le carnet.
    /// </summary>
    public Button boutonFermerCarnet;
    
    /// <summary>
    /// Panel affichant le dialogue avec le personnage.
    /// </summary>
    [Header("UI References - Panel Dialogue")]
    public GameObject panelDialogue;
    
    /// <summary>
    /// Nom du personnage affiché dans le dialogue.
    /// </summary>
    public TextMeshProUGUI nomPersonnageDialogue;
    
    /// <summary>
    /// Texte de la réponse du personnage.
    /// </summary>
    public TextMeshProUGUI dialogueReponse;
    
    /// <summary>
    /// Bouton de retour vers le bâtiment.
    /// </summary>
    [Header("Boutons")]
    public Button retourButton;
    
    #endregion

    /// <summary>
    /// Gestionnaire des dialogues JSON.
    /// </summary>
    private JsonDialogueManager dialogueManager;
    
    /// <summary>
    /// Données du personnage actuellement interrogé.
    /// </summary>
    private PlayerData personnageActuel;
    
    /// <summary>
    /// Liste des textes des questions disponibles.
    /// </summary>
    private List<string> questionsDisponibles = new List<string>();
    
    /// <summary>
    /// Association texte de question → identifiant de question.
    /// </summary>
    private Dictionary<string, string> questionsIdMap = new Dictionary<string, string>();
    
    /// <summary>
    /// Indique si le carnet de questions est ouvert.
    /// </summary>
    private bool carnetOuvert = false;
    
    /// <summary>
    /// Initialise l’interface et charge les données nécessaires.
    /// </summary>
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
    
    /// <summary>
    /// Gère les entrées clavier à chaque frame.
    /// </summary>
    void Update()
    {
        GererInputs();
    }
    
    /// <summary>
    /// Charge les données du personnage actuellement sélectionné.
    /// </summary>
    /// <remarks>
    /// Les informations sont récupérées depuis le <see cref="GameStateManager"/>.
    /// </remarks>
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
                nomPersonnageDialogue.text = $"{personnageActuel.prenom} {personnageActuel.nom} - {personnageActuel.metier} - {personnageActuel.service}";
            
            // Charge l'image par défaut donc l'emotion normal 
            ChargerImagePersonnage("normal");
            
            Debug.Log($"Personnage chargé : {personnageActuel.prenom} {personnageActuel.nom}");
        }
    }
    
    /// <summary>
    /// Charge l’image du personnage selon l’émotion donnée.
    /// </summary>
    /// <param name="emotion">
    /// Nom de l’émotion à afficher (ex: normal, colere, anxieux…).
    /// </param>
    /// <remarks>
    /// Si l’image correspondant à l’émotion n’est pas trouvée,
    /// l’image normale est chargée par défaut.
    /// </remarks>
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
    
    /// <summary>
    /// Prépare la liste des questions disponibles pour le personnage.
    /// </summary>
    /// <remarks>
    /// Cette méthode :
    /// <list type="bullet">
    /// <item><description>Charge le fichier <c>scenario_verites.json</c></description></item>
    /// <item><description>Détermine si le personnage est du service audité</description></item>
    /// <item><description>Associe les questions du scénario aux vérités disponibles</description></item>
    /// </list>
    /// </remarks>
    void PreparerQuestions()
{
    Debug.Log("=== DÉBUT PRÉPARATION QUESTIONS ===");
    
    if (GameStateManager.Instance == null || personnageActuel == null) {
        Debug.LogError("GameStateManager ou personnageActuel est NULL !");
        return;
    }
    
    questionsDisponibles.Clear();
    questionsIdMap.Clear();
    
    // DEBUG 1 : Vérifier le chemin du fichier vérités
    string filePath = Path.Combine(Application.persistentDataPath, "GameData", "scenario_verites.json");
    Debug.Log($"Chemin fichier vérités : {filePath}");
    Debug.Log($"Fichier existe : {File.Exists(filePath)}");
    
    if (!File.Exists(filePath))
    {
        Debug.LogError("Fichier vérités introuvable");
        return;
    }
    
    string jsonContent = File.ReadAllText(filePath);
    Debug.Log($"Contenu du fichier vérités (premiers 500 caractères) :\n{jsonContent.Substring(0, Mathf.Min(500, jsonContent.Length))}");
    
    VeritesScenarioRoot verites = JsonConvert.DeserializeObject<VeritesScenarioRoot>(jsonContent);
    
    // DEBUG 2 : Informations sur les vérités chargées
    Debug.Log($"Scénario dans vérités : {verites.scenario}");
    Debug.Log($"Niveau dans vérités : {verites.niveau}");
    Debug.Log($"Services disponibles dans vérités : [{string.Join(", ", verites.verites.Keys)}]");
    
    int scenario = GameStateManager.Instance.ScenarioActuel;
    ScenarioRoot scenarioData = ChargerScenario(scenario);
    
    if (scenarioData == null)
    {
        Debug.LogError("Impossible de charger le scénario");
        return;
    }
    
    // DEBUG 3 : Informations sur le personnage actuel
    Debug.Log($"=== PERSONNAGE ACTUEL ===");
    Debug.Log($"Nom : {personnageActuel.prenom} {personnageActuel.nom}");
    Debug.Log($"Service du personnage : '{personnageActuel.service}'");
    Debug.Log($"Métier du personnage : '{personnageActuel.metier}'");
    Debug.Log($"Caractère : {personnageActuel.caractere}");
    
    // DEBUG 4 : Informations sur le scénario
    Debug.Log($"=== SCÉNARIO ===");
    Debug.Log($"Service audité du scénario : '{scenarioData.service_audite}'");
    Debug.Log($"Questions service_audite disponibles : {scenarioData.questions.service_audite != null}");
    Debug.Log($"Questions autres_services disponibles : {scenarioData.questions.autres_services != null}");
    
    bool estServiceAudite = personnageActuel.service.Trim().ToLower() == scenarioData.service_audite.Trim().ToLower();
    Debug.Log($"Est service audité : {estServiceAudite}");
    Debug.Log($"Comparaison : '{personnageActuel.service.Trim().ToLower()}' == '{scenarioData.service_audite.Trim().ToLower()}'");

    List<string> listeQuestionsScenario = null;

    if (estServiceAudite && scenarioData.questions.service_audite != null)
    {
        listeQuestionsScenario = scenarioData.questions.service_audite.liste;
        Debug.Log($"Service audité - {listeQuestionsScenario.Count} questions dans le scénario");
    }
    else if (!estServiceAudite && scenarioData.questions.autres_services != null)
    {
        listeQuestionsScenario = scenarioData.questions.autres_services.liste;
        Debug.Log($"Autre service - {listeQuestionsScenario.Count} questions dans le scénario");
    }
    
    if (listeQuestionsScenario == null)
    {
        Debug.LogError("Liste questions scénario est null!");
        return;
    }
    
    Debug.Log($"=== LISTE DES QUESTIONS DU SCÉNARIO ===");
    for (int i = 0; i < listeQuestionsScenario.Count; i++)
    {
        Debug.Log($"Question {i}: {listeQuestionsScenario[i]}");
    }

    // DEBUG 5 : Vérification dans les vérités
    Debug.Log($"=== RECHERCHE DANS LES VÉRITÉS ===");
    Debug.Log($"Recherche du service '{personnageActuel.service}' dans les vérités...");
    
    if (verites.verites.ContainsKey(personnageActuel.service))
    {
        Debug.Log($"✓ Service '{personnageActuel.service}' trouvé dans les vérités");
        
        var serviceVerites = verites.verites[personnageActuel.service];
        Debug.Log($"Postes disponibles dans ce service : [{string.Join(", ", serviceVerites.postes.Keys)}]");
        
        if (serviceVerites.postes.ContainsKey(personnageActuel.metier))
        {
            Debug.Log($"✓ Métier '{personnageActuel.metier}' trouvé dans le service");
            
            var posteVerites = serviceVerites.postes[personnageActuel.metier];
            Debug.Log($"Nombre de questions disponibles pour ce poste : {posteVerites.verites.Count}");
            Debug.Log($"IDs des questions : [{string.Join(", ", posteVerites.verites.Keys)}]");
            
            foreach (string questionId in posteVerites.verites.Keys)
            {
                Debug.Log($"Traitement question ID : {questionId}");
                int index = int.Parse(questionId);
                Debug.Log($"Index parsé : {index}, Taille liste : {listeQuestionsScenario.Count}");
                
                if (index < listeQuestionsScenario.Count)
                {
                    string texteQuestion = listeQuestionsScenario[index];
                    questionsDisponibles.Add(texteQuestion);
                    questionsIdMap[texteQuestion] = questionId;
                    Debug.Log($"✓ Question ajoutée : {texteQuestion}");
                }
                else
                {
                    Debug.LogError($"✗ Index {index} hors limites (max: {listeQuestionsScenario.Count - 1})");
                }
            }
        }
        else
        {
            Debug.LogError($"✗ Métier '{personnageActuel.metier}' NON trouvé dans le service '{personnageActuel.service}'!");
            Debug.LogError($"Métiers disponibles : [{string.Join(", ", serviceVerites.postes.Keys)}]");
        }
    }
    else
    {
        Debug.LogError($"✗ Service '{personnageActuel.service}' NON trouvé dans les vérités!");
        Debug.LogError($"Services disponibles : [{string.Join(", ", verites.verites.Keys)}]");
    }
    
    Debug.Log($"=== RÉSULTAT FINAL ===");
    Debug.Log($"Total questions trouvées: {questionsDisponibles.Count}");
    
    if (questionsDisponibles.Count > 0)
    {
        Debug.Log("Questions disponibles :");
        for (int i = 0; i < questionsDisponibles.Count; i++)
        {
            Debug.Log($"  {i + 1}. {questionsDisponibles[i]}");
        }
    }
    
    AfficherQuestionsCarnet();
}
    
    /// <summary>
    /// Charge les données d’un scénario depuis un fichier JSON.
    /// </summary>
    /// <param name="numeroScenario">Numéro du scénario.</param>
    /// <returns>
    /// Objet <see cref="ScenarioRoot"/> contenant les données du scénario,
    /// ou null en cas d’erreur.
    /// </returns>
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
    /// Affiche les questions disponibles dans le carnet.
    /// </summary>
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
    
    /// <summary>
    /// Ouvre le carnet de questions.
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
    /// Ferme le carnet de questions.
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
    /// Gère les entrées clavier de l’utilisateur.
    /// </summary>
    /// <remarks>
    /// Touches gérées :
    /// <list type="bullet">
    /// <item><description>Touches 1 à 9 : poser une question</description></item>
    /// <item><description>Espace : fermer le dialogue</description></item>
    /// <item><description>R : retour au bâtiment</description></item>
    /// </list>
    /// </remarks>
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
    
    /// <summary>
    /// Pose une question au personnage et affiche la réponse.
    /// </summary>
    /// <param name="indexQuestion">
    /// Index de la question dans la liste des questions disponibles.
    /// </param>
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
    
    /// <summary>
    /// Retourne à la scène du bâtiment.
    /// </summary>
    void RetourBatiment()
    {
        SceneManager.LoadScene("InteriorScene");
    }
}