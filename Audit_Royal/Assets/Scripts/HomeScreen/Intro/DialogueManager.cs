// DialogueManager.cs
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // ===== RÉFÉRENCES UI =====
    [Header("Interface")]
    public GameObject bandeau;              // Bandeau avec le nom du personnage
    public GameObject dialoguePanel;        // Panel avec le texte
    public TextMeshProUGUI speakerText;     // Nom du personnage qui parle
    public TextMeshProUGUI dialogueText;    // Le texte du dialogue
    public Button nextButton;               // Bouton optionnel

    [Header("Paramètres")]
    public float typingSpeed = 0.02f;       // Vitesse d'affichage des lettres
    public MonoBehaviour playerMovementScript; // Script de déplacement à désactiver

    // pour DialogueAutoStart)
    [HideInInspector] public string speaker = "Chef";           // Nom du personnage qui parle
    [HideInInspector] public bool isIntroDialogue = false;      // Est-ce le dialogue d'intro avec choix ?

    private string[] lines;                 // les lignes de dialogue
    private int index = 0;                  // La ligne actuelle
    private bool isTyping = false;          // Est-ce qu'on est en train d'écrire ?
    private bool dialogueActive = false;    // Le dialogue est-il actif ?
    private bool waitingForChoice = false;  // Attend-on un choix du joueur ?
    private bool waitingForPlayer = false;  // Attend-on que le joueur appuie sur Entrée ?
    private Coroutine typingCoroutine;      // pour effet typewriter

    // ===== INITIALISATION =====
    void Start()
    {
        // Cacher le dialogue au démarrage
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        
        // bouton Next
        if (nextButton != null) nextButton.onClick.AddListener(NextLine);
    }

    // GESTION DES INPUTS
    void Update()
    {
        // Si le dialogue n'est pas actif, on ne fait rien
        if (!dialogueActive) return;

        // CAS 1 : On attend que le joueur appuie sur Entrée après avoir fait un choix
        if (waitingForPlayer && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            waitingForPlayer = false;
            EndDialogue();
            return;
        }

        // CAS 2 : On attend un choix du joueur (touches 1-4)
        if (waitingForChoice && !isTyping)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                OnChoiceSelected("choix1");
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                OnChoiceSelected("choix2");
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                OnChoiceSelected("choix3");
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                OnChoiceSelected("choix4");
            return;
        }

        // CAS 3 : Dialogue normal - Appuyer sur Espace/Entrée pour accélerer le truc
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            // Si on est en train d'écrire → Finir la ligne immédiatement
            if (isTyping)
            {
                FinishTyping();
            }
            // Sinon → Passer à la ligne suivante
            else if (!waitingForChoice)
            {
                NextLine();
            }
        }
    }

    // ===== DÉMARRER UN DIALOGUE =====
    public void StartDialogue(string[] dialogueLines)
    {
        // Vérifier qu'on a bien des lignes
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        // Initialiser
        lines = dialogueLines;
        index = 0;
        dialogueActive = true;
        waitingForChoice = false;
        waitingForPlayer = false;

        // Afficher l'interface
        if (bandeau != null) bandeau.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        speakerText.text = speaker;

        // désactiver le mouvement du joueur car plus de joueur dans la scène
        if (playerMovementScript != null) 
            playerMovementScript.enabled = false;

        // Afficher la première ligne
        ShowLine();
    }

    // ===== AFFICHER LA LIGNE ACTUELLE =====
    void ShowLine()
    {
        // Vérifier qu'on n'a pas dépassé la fin
        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        // Arrêter l'écriture précédente si elle existe
        if (typingCoroutine != null) 
            StopCoroutine(typingCoroutine);

        // effet typewriter
        typingCoroutine = StartCoroutine(TypeLine(lines[index]));
    }

    // EFFET TYPEWRITER (écriture lettre par lettre)
    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        // Ajouter chaque lettre une par une
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // FINIR L'ÉCRITURE IMMÉDIATEMENT
    void FinishTyping()
    {
        if (typingCoroutine != null) 
            StopCoroutine(typingCoroutine);
        
        dialogueText.text = lines[index];
        isTyping = false;
    }

    // PASSER À LA LIGNE SUIVANTE
    void NextLine()
    {
        // Si on est encore en train d'écrire, finir d'abord
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        index++;

        // Si on a encore des lignes à afficher
        if (index < lines.Length)
        {
            ShowLine();
        }
        else
        {
            // À la fin du tableau
            // Si c'est le dialogue d'intro → Afficher les choix
            if (isIntroDialogue)
            {
                ShowPlayerChoices();
            }
            // Sinon → Terminer le dialogue
            else
            {
                EndDialogue();
            }
        }
    }

    // AFFICHER LES CHOIX DU JOUEUR
    void ShowPlayerChoices()
    {
        waitingForChoice = true;
        speaker = "Vous";  // Le joueur parle maintenant
        speakerText.text = speaker;

        // Afficher les 4 choix
        dialogueText.text =
            "1. <link=\"choix1\">Je comprends. Je vais commencer par rencontrer l'équipe IT directement.</link>\n" +
            "2. <link=\"choix2\">Je préférerais d'abord consulter vos procédures de sécurité avant d'interroger qui que ce soit.</link>\n" +
            "3. <link=\"choix3\">Est-ce que vous avez déjà une idée précise des problèmes ?</link>\n" +
            "4. <link=\"choix4\">Je n'ai pas besoin d'explications supplémentaires, je m'occupe de tout.</link>";
    }

    // SÉLECTIONNER UN CHOIX
    public void OnChoiceSelected(string linkID)
    {
        Debug.Log("Choix sélectionné : " + linkID);
        
        waitingForChoice = false;
        speakerText.text = "Chef";  // Le chef répond

        // Réponse en fonction du choix
        switch (linkID)
        {
            case "choix1":
                dialogueText.text = "Très bien, mais souvenez-vous : ils peuvent être sur la défensive.";
                break;
            case "choix2":
                dialogueText.text = "Sage décision. Vous trouverez des documents intéressants dans votre sac à dos.";
                break;
            case "choix3":
                dialogueText.text = "Nous avons des soupçons, mais aucune preuve. C'est à vous de trier le vrai du faux.";
                break;
            case "choix4":
                dialogueText.text = "Hum… J'espère que vous êtes aussi sûr de vous dans vos conclusions.";
                break;
        }

        // On attend que le joueur appuie sur Espace/Entrée pour finir
        waitingForPlayer = true;
    }

    // TERMINER LE DIALOGUE
    void EndDialogue()
    {
        // Cacher l'interface
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (bandeau != null) bandeau.SetActive(false);
        
        dialogueActive = false;

        // Réactiver le mouvement du joueur
        if (playerMovementScript != null) 
            playerMovementScript.enabled = true;

        // Charger la scène Map
        SceneManager.LoadScene("Map");
    }
}