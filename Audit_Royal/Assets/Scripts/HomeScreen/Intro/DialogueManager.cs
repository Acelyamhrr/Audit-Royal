// DialogueManager.cs
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("UI references")]
    public GameObject bandeau; // le bandeau en haut
    public GameObject dialoguePanel; // le panel de bas
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton; // optionnel

    [Header("Typing")]
    public float typingSpeed = 0.02f;

    [HideInInspector] public string speaker = "Chef";
    [HideInInspector] public string[] lines;
    [HideInInspector] public bool isIntroDialogue = false;

    int index = 0;
    Coroutine typingCoroutine;
    bool isTyping = false;
    bool dialogueActive = false;

    bool waitingForChoice = false;
    private bool waitingForPlayer = false;


    [Header("Player (optionnel)")]
    public MonoBehaviour playerMovementScript; // glisse ici le script de movement du joueur pour le désactiver

    void Start()
    {
        if (bandeau != null) bandeau.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(NextLine);
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (waitingForPlayer && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            waitingForPlayer = false;
            EndDialogue(); // ou charger la scène suivante
        }

        // Si on attend le choix du joueur et que le typewriter n'est pas actif
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
        }

        // Avancer avec Espace ou Entrée si on n'attend pas un choix
        if (!isTyping && !waitingForChoice && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            NextLine();
        }
        // Si on tape pendant le typewriter, espace ou entrée => finir la ligne
        else if (isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = lines[index];
            isTyping = false;
        }
    }


    public void StartDialogue(string[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        index = 0;
        if (bandeau != null) bandeau.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        speakerText.text = speaker;
        dialogueActive = true;

        if (playerMovementScript != null) playerMovementScript.enabled = false;
        ShowLine();
    }

    void ShowLine()
    {
        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(lines[index]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void NextLine()
    {
        if (isTyping)
        {
            // si on est en train d'écrire, terminer immédiatement
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = lines[index];
            isTyping = false;
            return;
        }

        index++;

        if (index < lines.Length)
        {
            // afficher la ligne suivante
            ShowLine();
        }
        else
        {
            // à la fin du tableau, décider quoi faire
            // si on est dans le dialogue d'intro avec choix
            if (isIntroDialogue)
            {
                ShowPlayerChoices();  // afficher les choix du joueur
            }
            else
            {
                EndDialogue();        // fin normale du dialogue
            }
        }
    }


    void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (bandeau != null) bandeau.SetActive(false);
        dialogueActive = false;

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        SceneManager.LoadScene("Map");
    }

    void ShowPlayerChoices()
    {
        waitingForChoice = true;
        speaker = "Vous";               //Changer le personnage qui parle
        speakerText.text = speaker;

        dialogueText.text =
            "1. <link=\"choix1\">Je comprends. Je vais commencer par rencontrer l’équipe IT directement.</link>\n" +
            "2. <link=\"choix2\">Je préférerais d’abord consulter vos procédures de sécurité avant d’interroger qui que ce soit.</link>\n" +
            "3. <link=\"choix3\">Est-ce que vous avez déjà une idée précise des problèmes ?</link>\n" +
            "4. <link=\"choix4\">Je n’ai pas besoin d’explications supplémentaires, je m’occupe de tout.</link>\n";

    }

    public void OnChoiceSelected(string linkID)
    {
        Debug.Log("Choix cliqué : " + linkID);
        speakerText.text = "Chef";
        switch(linkID)
        {
            case "choix1":
                dialogueText.text = "Très bien, mais souvenez-vous : ils peuvent être sur la défensive.";
                break;
            case "choix2":
                dialogueText.text = "Sage décision. Vous trouverez des documents intéressants dans votre sac à dos.";
                break;
            case "choix3":
                dialogueText.text = "Nous avons des soupçons, mais aucune preuve. C’est à vous de trier le vrai du faux.";
                break;
            case "choix4":
                dialogueText.text = "Hum… J’espère que vous êtes aussi sûr de vous dans vos conclusions.";
                break;
        }

        // On attend que le joueur appuie sur entrée
        waitingForPlayer = true;
    }


}