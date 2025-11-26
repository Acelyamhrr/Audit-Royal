using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// Gère l'affichage du dialogue avec un personnage
/// À attacher sur une scène de personnage (DirectorCom, Chef, etc.)
public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nomPersonnageText;
    public TextMeshProUGUI metierText;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI questionsText;
    
    [Header("Boutons")]
    public Button retourButton;
    
    private JsonDialogueManager dialogueManager;
    private PlayerData personnageActuel;
    private List<string> questionsDisponibles = new List<string>();
    
    void Start()
    {
        // Récupérer ou créer le DialogueManager
        dialogueManager = FindFirstObjectByType<JsonDialogueManager>();
        if (dialogueManager == null)
        {
            GameObject go = new GameObject("DialogueManager");
            dialogueManager = go.AddComponent<JsonDialogueManager>();
        }
        
        // Bouton retour vers le bâtiment
        if (retourButton != null)
        {
            retourButton.onClick.AddListener(RetourBatiment);
        }
        
        ChargerPersonnage();
        AfficherQuestions();
    }
    
    void Update()
    {
        // Gestion des touches pour poser les questions
        GererInputQuestions();
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
            // Affiche le nom et métier
            if (nomPersonnageText != null)
                nomPersonnageText.text = $"{personnageActuel.prenom} {personnageActuel.nom}";
            
            if (metierText != null)
                metierText.text = $"{personnageActuel.metier} - Service {personnageActuel.service}";
            
            Debug.Log($"Personnage chargé : {personnageActuel.prenom} {personnageActuel.nom}");
        }
    }
    
    /// Affiche les questions disponibles pour ce personnage
    void AfficherQuestions()
    {
        if (GameStateManager.Instance == null || personnageActuel == null)
            return;
        
        // TODO: recup les questions depuis le json
        
        string texte = "Questions disponibles :\n\n";
        texte += "[1] Question 1\n";
        texte += "[2] Question 2\n";
        texte += "[3] Question 3\n\n";
        texte += "[R] Retour au bâtiment";
        
        if (questionsText != null)
            questionsText.text = texte;
    }
    
    /// Gère les inputs pour poser les questions
    void GererInputQuestions()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PoserQuestion("0"); // Question 0
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PoserQuestion("1"); // Question 1
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PoserQuestion("2"); // Question 2
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            RetourBatiment();
        }
    }
    
    /// Pose une question au personnage et affiche la réponse
    void PoserQuestion(string numeroQuestion)
    {
        if (GameStateManager.Instance == null || personnageActuel == null)
            return;
        
        int scenario = GameStateManager.Instance.ScenarioActuel;
        string fichierPersonnage = GameStateManager.Instance.FichierPersonnageActuel;
        
        string reponse = dialogueManager.ObtenirDialogue(
            scenario,
            fichierPersonnage,
            numeroQuestion
        );
        
        if (dialogueText != null)
        {
            dialogueText.text = $"\"{reponse}\"\n\n[Espace] Continuer";
        }
        
        Debug.Log($"Question {numeroQuestion} posée → Réponse : {reponse}");
    }
    
    /// Retour vers la scène du bâtiment
    void RetourBatiment()
    {
        // Détermine quelle scène de bâtiment charger selon le service
        string sceneBatiment = "Map"; // Par défaut retour à la map
        
        if (GameStateManager.Instance != null)
        {
            string service = GameStateManager.Instance.ServiceActuel;
            
            switch (service)
            {
                case "communication":
                    sceneBatiment = "Communication";
                    break;
                case "info":
                    sceneBatiment = "Informatique";
                    break;
                case "comptabilite":
                    sceneBatiment = "Compta";
                    break;
                case "restauration":
                    sceneBatiment = "Crous";
                    break;
            }
        }
        
        SceneManager.LoadScene(sceneBatiment);
    }
}