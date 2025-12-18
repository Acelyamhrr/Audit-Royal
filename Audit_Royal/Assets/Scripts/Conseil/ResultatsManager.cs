using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ResultatsManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    
    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    public GameObject tamponValide;
    public Button continueButton;
    
    [Header("Cinematic")]
    public CinematicController cinematicController;
    
    [Header("Audio")]
    public AudioClip applauseSound;
    public AudioClip scoreSound;
    private AudioSource audioSource;
    
    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float fastTypingSpeed = 0.01f;  // Vitesse rapide quand on appuie une fois
    public float scoreAnimationDuration = 2f;
    public KeyCode nextDialogueKey = KeyCode.Space;
    
    // Variables privées
    private int scoreFinal;
    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    private bool isTyping = false;
    private bool isFastTyping = false;  // Mode rapide activé
    private bool canAdvance = false;
    private Coroutine typingCoroutine;
    private string currentFullText = "";  // Texte complet de la ligne en cours
    
    private enum Phase
    {
        Introduction,
        WaitingForCinematic,
        InCinematic,
        Conclusion,
        Finished
    }
    private Phase currentPhase = Phase.Introduction;
    
    [System.Serializable]
    private class DialogueLine
    {
        public string speakerName;
        public string text;
        
        public DialogueLine(string speaker, string dialogue)
        {
            speakerName = speaker;
            text = dialogue;
        }
    }
    
    void Start()
    {
        // Récupérer le score
        if (GameStateManager.Instance != null)
        {
            scoreFinal = GameStateManager.Instance.ScoreDernierRapport;
            Debug.Log($"Score récupéré : {scoreFinal}%");
        }
        else
        {
            scoreFinal = 90;
            Debug.LogWarning("GameStateManager introuvable, score de test : 90%");
        }
        
        if (tamponValide != null)
        {
            
            tamponValide.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
        
        if (scoreText != null)
        {
            scoreText.text = "0%";
        }

        
        // Démarrer
        StartIntroduction();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(nextDialogueKey))
        {
            if (isTyping && !isFastTyping)
            {
                // Premier appui : accélérer
                isFastTyping = true;
                Debug.Log("Mode rapide activé");
            }
            else if (isTyping && isFastTyping)
            {
                // Deuxième appui : afficher tout immédiatement
                StopTypingAndShowAll();
            }
            else if (canAdvance)
            {
                // Le texte est fini, passer au dialogue suivant
                DisplayNextDialogue();
            }
        }
    }
    
    void StartIntroduction()
    {
        currentPhase = Phase.Introduction;
        
        List<DialogueLine> introDialogues = new List<DialogueLine>()
        {
            new DialogueLine("Chef du Conseil", "Bonjour et bienvenue au conseil d'administration."),
            new DialogueLine("Chef du Conseil", "Nous avons examiné votre rapport d'audit avec attention."),
            new DialogueLine("Chef du Conseil", "Voyons ensemble vos résultats...")
        };
        
        StartDialogueSequence(introDialogues);
    }
    
    void StartDialogueSequence(List<DialogueLine> dialogues)
    {
        dialogueQueue.Clear();
        
        foreach (var line in dialogues)
        {
            dialogueQueue.Enqueue(line);
        }
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        DisplayNextDialogue();
    }
    
    void DisplayNextDialogue()
    {
        if (dialogueQueue.Count == 0)
        {
            OnDialoguePhaseFinished();
            return;
        }
        
        DialogueLine line = dialogueQueue.Dequeue();
        currentFullText = line.text;  // Sauvegarder le texte complet
        isFastTyping = false;  // Réinitialiser le mode rapide
        
        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }
    
    IEnumerator TypeText(string text)
    {
        isTyping = true;
        canAdvance = false;
        dialogueText.text = "";
        
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            
            // Utiliser la vitesse rapide si le mode est activé
            float currentSpeed = isFastTyping ? fastTypingSpeed : typingSpeed;
            yield return new WaitForSeconds(currentSpeed);
        }
        
        isTyping = false;
        isFastTyping = false;
        canAdvance = true;
    }
    
    void StopTypingAndShowAll()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        // Afficher le texte complet immédiatement
        dialogueText.text = currentFullText;
        isTyping = false;
        isFastTyping = false;
        canAdvance = true;
        Debug.Log("Texte affiché en entier");
    }
    
    void OnDialoguePhaseFinished()
    {
        switch (currentPhase)
        {
            case Phase.Introduction:
                StartCoroutine(StartCinematicSequence());
                break;
                
            case Phase.Conclusion:
                ShowContinueButton();
                break;
        }
    }
    
    IEnumerator StartCinematicSequence()
    {
        currentPhase = Phase.WaitingForCinematic;
        canAdvance = false;
        
        yield return new WaitForSeconds(0.5f);
        
        // CACHER LE DIALOGUE PANEL pour voir le score !
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);

        }
        
        // Entrer en mode cinématique
        currentPhase = Phase.InCinematic;
        cinematicController.EnterCinematic();
        yield return new WaitForSeconds(1.15f); // Attendre que l'animation soit finie
        
        // Animation du score
        Debug.Log("Animation Score");
        scoreText.gameObject.SetActive(true);
        
        
        yield return StartCoroutine(AnimerScore());


        if (scoreFinal >= 85)
        {
            if (tamponValide != null)
            {
                tamponValide.SetActive(true);
                StartCoroutine(AnimerTampon());
            }
        }

        yield return new WaitForSeconds(3f);
        
        // Sortir du mode cinématique et désactiver la jauge

        
        cinematicController.ExitCinematic();
        scoreText.gameObject.SetActive(false);
        tamponValide.SetActive(false);
        yield return new WaitForSeconds(1.5f); // Attendre que l'animation soit finie
        
        // RÉAFFICHER LE DIALOGUE PANEL pour la conclusion
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        // Passer à la conclusion
        StartConclusion();
    }
    
    void StartConclusion()
    {
        currentPhase = Phase.Conclusion;
        
        List<DialogueLine> conclusionDialogues = new List<DialogueLine>();
        
        if (scoreFinal >= 85)
        {
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Excellent travail !"));
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Vous avez parfaitement compris les enjeux de cet audit."));
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Nous sommes impressionnés par votre rigueur."));
        }
        else if (scoreFinal >= 75)
        {
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Très bien !"));
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Votre rapport est solide, continuez sur cette lancée."));
        }
        else if (scoreFinal >= 50)
        {
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "C'est correct..."));
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Mais il y a encore des points à améliorer."));
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Soyez plus attentif aux détails la prochaine fois."));
        }
        else
        {
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Hmm... Ce rapport nécessite plus de rigueur."));
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Prenez le temps d'analyser les informations avec attention."));
            conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Vous pouvez faire mieux."));
        }
        
        conclusionDialogues.Add(new DialogueLine("Chef du Conseil", "Vous pouvez maintenant continuer votre mission."));
        
        StartDialogueSequence(conclusionDialogues);
    }
    
    void ShowContinueButton()
    {
        currentPhase = Phase.Finished;
        
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(PasserNiveauSuivant);
        }
    }
    
    IEnumerator AnimerScore()
    {
        float temps = 0f;
        int scoreActuel = 0;
        
        while (temps < scoreAnimationDuration)
        {
            temps += Time.deltaTime;
            float progression = temps / scoreAnimationDuration;
            
            scoreActuel = Mathf.RoundToInt(Mathf.Lerp(0, scoreFinal, progression));
            
            if (scoreText != null)
            {
                scoreText.text = scoreActuel + "%";
            }
            
            
            yield return null;
        }
        
        if (scoreText != null)
        {
            scoreText.text = scoreFinal + "%";
        }
    }
    
    IEnumerator AnimerTampon()
    {
        float duree = 1.0f;
        float temps = 0f;
        
        Transform tamponTransform = tamponValide.transform;
        Vector3 echelleDepart = Vector3.zero;
        Vector3 echelleFin = Vector3.one;
        Quaternion rotationDepart = Quaternion.Euler(0, 0, -15f);
        Quaternion rotationFin = Quaternion.Euler(0, 0, 15f);
        
        tamponTransform.localScale = echelleDepart;
        tamponTransform.rotation = rotationDepart;
        
        while (temps < duree)
        {
            temps += Time.deltaTime;
            float progression = temps / duree;
            
            tamponTransform.localScale = Vector3.Lerp(echelleDepart, echelleFin, progression);
            tamponTransform.rotation = Quaternion.Lerp(rotationDepart, rotationFin, progression);
            
            yield return null;
        }
        
        tamponTransform.localScale = echelleFin;
        tamponTransform.rotation = rotationFin;
    }
    
    void PasserNiveauSuivant()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.PasserNiveauSuivant();
        }
        
        SceneManager.LoadScene("Map");
    }
}