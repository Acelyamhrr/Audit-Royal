using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Gère l'introduction du jeu avec le dialogue du chef adapté au scénario.
/// </summary>
public class IntroSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bandeau;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Settings")]
    [SerializeField] private float delayBeforeStart = 2f;
    [SerializeField] private float typingSpeed = 0.03f;

    private ScenarioRoot scenarioData;
    private int currentLineIndex = 0;
    private string[] dialogueLines;
    private bool isTyping = false;
    private bool waitingForChoice = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        // Récupère le scénario sélectionné depuis le GameStateManager
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager introuvable ! Retour au menu.");
            SceneManager.LoadScene("MainMenu");
            return;
        }

        int scenarioId = GameStateManager.Instance.ScenarioActuel;
        
        // Charge les données du scénario
        if (!ChargerScenario(scenarioId))
        {
            Debug.LogError($"Impossible de charger le scénario {scenarioId}");
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // Initialise l'UI
        InitialiserUI();

        // Démarre le dialogue après un délai
        StartCoroutine(DemarrerDialogueApresDelai());
    }

    void Update()
    {
        // Si on est en train d'attendre un choix, gérer les touches 1-4
        if (waitingForChoice)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                OnChoixSelectionne(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                OnChoixSelectionne(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                OnChoixSelectionne(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                OnChoixSelectionne(3);
            return;
        }

        // Espace ou Entrée pour avancer
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
            {
                // Finir d'écrire immédiatement
                FinirEcriture();
            }
            else
            {
                // Ligne suivante
                AfficherLigneSuivante();
            }
        }
    }

    /// <summary>
    /// Charge le fichier JSON du scénario.
    /// </summary>
    bool ChargerScenario(int numeroScenario)
    {
        string nomFichier = $"scenario{numeroScenario}.json";
        string filePath = Path.Combine(Application.streamingAssetsPath, nomFichier);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fichier scénario introuvable : {filePath}");
            return false;
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            scenarioData = JsonConvert.DeserializeObject<ScenarioRoot>(jsonContent);
            
            Debug.Log($"Scénario chargé : {scenarioData.titre}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors du chargement du scénario : {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Initialise l'interface utilisateur.
    /// </summary>
    void InitialiserUI()
    {
        if (bandeau != null) bandeau.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// Génère les lignes de dialogue en fonction du scénario chargé.
    /// </summary>
    string[] GenererDialogue()
    {
        string nomService = ObtenirNomCompletService(scenarioData.service_audite);
        
        return new string[]
        {
            "Bonjour. Merci d'être venu.",
            $"Nous avons décidé de vous confier un audit interne sur le service {nomService}.",
            $"Thème de la mission : {scenarioData.theme}",
            "*Le chef marque une pause et regarde le joueur attentivement.*",
            "Voici la situation :",
            scenarioData.problematique,
            "*Le chef pose un dossier sur le bureau.*",
            "Votre mission est simple :",
            "– Interrogez les membres du personnel et les utilisateurs,",
            "– Recueillez leurs témoignages,",
            "– Faites la part entre les faits, les exagérations et les rumeurs,",
            "– Et enfin, rédigez un rapport fiable que je pourrai présenter au conseil de direction."
        };
    }

    /// <summary>
    /// Convertit l'identifiant du service en nom complet.
    /// </summary>
    string ObtenirNomCompletService(string serviceId)
    {
        switch (serviceId.ToLower())
        {
            case "technicien": return "Technique (Entretien et Propreté)";
            case "info": return "Informatique";
            case "restauration": return "Restauration";
            case "comptabilite": return "Comptabilité";
            case "communication": return "Communication";
            default: return serviceId;
        }
    }

    /// <summary>
    /// Démarre le dialogue après un délai.
    /// </summary>
    IEnumerator DemarrerDialogueApresDelai()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        if (bandeau != null) bandeau.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        
        if (speakerText != null)
            speakerText.text = "Chef";

        dialogueLines = GenererDialogue();
        currentLineIndex = 0;

        AfficherLigne();
    }

    /// <summary>
    /// Affiche la ligne de dialogue actuelle avec effet typewriter.
    /// </summary>
    void AfficherLigne()
    {
        if (currentLineIndex >= dialogueLines.Length)
        {
            AfficherChoix();
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(EffetTypewriter(dialogueLines[currentLineIndex]));
    }

    /// <summary>
    /// Effet d'écriture lettre par lettre.
    /// </summary>
    IEnumerator EffetTypewriter(string ligne)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in ligne)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// Finit l'écriture immédiatement.
    /// </summary>
    void FinirEcriture()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = dialogueLines[currentLineIndex];
        isTyping = false;
    }

    /// <summary>
    /// Passe à la ligne suivante.
    /// </summary>
    void AfficherLigneSuivante()
    {
        currentLineIndex++;
        AfficherLigne();
    }

    /// <summary>
    /// Affiche les choix du joueur.
    /// </summary>
    void AfficherChoix()
    {
        waitingForChoice = true;
        
        if (speakerText != null)
            speakerText.text = "Vous";

        dialogueText.text = 
            "1. Je comprends. Je vais commencer par rencontrer l'équipe directement.\n" +
            "2. Je préférerais d'abord consulter vos procédures avant d'interroger qui que ce soit.\n" +
            "3. Avez-vous déjà une idée précise des problèmes ?\n" +
            "4. Je n'ai pas besoin d'explications supplémentaires, je m'occupe de tout.";
    }

    /// <summary>
    /// Gère la sélection d'un choix.
    /// </summary>
    void OnChoixSelectionne(int choiceIndex)
    {
        waitingForChoice = false;
        
        if (speakerText != null)
            speakerText.text = "Chef";

        string[] reponses = new string[]
        {
            "Très bien, mais souvenez-vous : ils peuvent être sur la défensive.",
            "Sage décision. Vous trouverez des documents intéressants dans votre inventaire.",
            "Nous avons des soupçons, mais aucune preuve. C'est à vous de trier le vrai du faux.",
            "Hum… J'espère que vous êtes aussi sûr de vous dans vos conclusions."
        };

        dialogueText.text = reponses[choiceIndex] + "\n\n[Appuyez sur Espace pour continuer]";
        
        // Attend que le joueur appuie sur Espace
        StartCoroutine(AttendreEspaceEtChargerMap());
    }

    /// <summary>
    /// Attend que le joueur appuie sur Espace puis charge la Map.
    /// </summary>
    IEnumerator AttendreEspaceEtChargerMap()
    {
        while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }
        
        SceneManager.LoadScene("Map");
    }
}