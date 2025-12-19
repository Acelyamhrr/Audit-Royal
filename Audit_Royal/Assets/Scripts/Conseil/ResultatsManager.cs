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
    
    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float fastTypingSpeed = 0.01f;  // Vitesse rapide quand on appuie une fois
    public float scoreAnimationDuration = 2f;
    public float randomNumbersDuration = 1.5f;
    public float randomNumbersSpeed = 0.08f;
    public KeyCode nextDialogueKey = KeyCode.Space;
    
    // Variables privées
    private int scoreFinal;
    //private int scoreLb = 1;
    private int niveauActuel;
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
        // Récupérer le score ET le niveau
        if (GameStateManager.Instance != null)
        {
            scoreFinal = GameStateManager.Instance.ScoreDernierRapport;
            niveauActuel = GameStateManager.Instance.NiveauActuel;

        	GameStateManager.Instance.ScoreTotalCumule += scoreFinal;

        	Debug.Log($"Score récupéré : {scoreFinal}% - Niveau : {niveauActuel}");
        	Debug.Log($"Score total cumulé : {GameStateManager.Instance.ScoreTotalCumule}");        }
        else
        {
            scoreFinal = 90;
            niveauActuel = 1;
            Debug.LogWarning("GameStateManager introuvable, score de test : 90% - Niv 1");
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
    
        List<DialogueLine> introDialogues = GenererDialoguesIntroduction();
        StartDialogueSequence(introDialogues);
        
    }
    
    List<DialogueLine> GenererDialoguesIntroduction()
    {
        List<DialogueLine> dialogues = new List<DialogueLine>();
        
        switch (niveauActuel)
        {
            case 1:
                dialogues.Add(new DialogueLine("Chef du Conseil", "Bonjour et bienvenue au conseil d'administration."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "C'est votre premier audit, nous allons voir ce que vous valez."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Voyons ensemble vos résultats..."));
                break;
                
            case 2:
                dialogues.Add(new DialogueLine("Chef du Conseil", "De retour parmi nous."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Vous commencez à prendre vos marques, voyons si vous progressez."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Analysons votre rapport..."));
                break;
                
            case 3:
                dialogues.Add(new DialogueLine("Chef du Conseil", "Bienvenue pour votre troisième audit."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "À ce stade, nous attendons davantage de rigueur de votre part."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Examinons votre travail..."));
                break;
                
            case 4:
                dialogues.Add(new DialogueLine("Chef du Conseil", "Vous êtes maintenant un auditeur expérimenté."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Cette mission était complexe, j'espère que vous avez été à la hauteur."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Voyons cela de plus près..."));
                break;
                
            case 5:
                dialogues.Add(new DialogueLine("Chef du Conseil", "Votre dernière mission. Le niveau le plus difficile."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Nous avons placé en vous de grandes attentes."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Découvrons ensemble votre performance finale..."));
                break;
                
            default:
                // Fallback si niveau inconnu
                dialogues.Add(new DialogueLine("Chef du Conseil", "Bonjour et bienvenue au conseil d'administration."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Nous avons examiné votre rapport d'audit avec attention."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Voyons ensemble vos résultats..."));
                break;
        }
        
        return dialogues;
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
        
        // dabord : nbs aléatoires pour mettre du suspens
        yield return StartCoroutine(AnimerNombresAleatoires());
        
        // ensuite animation vers le score final
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
    
    IEnumerator AnimerNombresAleatoires()
    {
        float tempsEcoule = 0f;
        
        while (tempsEcoule < randomNumbersDuration)
        {
            // Génère un nombre aléatoire entre 0 et 100
            int nombreAleatoire = Random.Range(0, 101);
            
            if (scoreText != null)
            {
                scoreText.text = nombreAleatoire + "%";
            }
            
            yield return new WaitForSeconds(randomNumbersSpeed);
            tempsEcoule += randomNumbersSpeed;
        }
    }

    
    void StartConclusion()
    {
        currentPhase = Phase.Conclusion;
        
        List<DialogueLine> conclusionDialogues = GenererDialoguesConclusion();
        StartDialogueSequence(conclusionDialogues);

    }
    
    void ShowContinueButton()
    {
        currentPhase = Phase.Finished;
        
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            
            // Si niveau 5, bouton différent
            if (niveauActuel >= 5)
            {
                TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Terminer";
                }
                continueButton.onClick.AddListener(Leaderboard);
            }
            else
            {
                continueButton.onClick.AddListener(PasserNiveauSuivant);
            }
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
            //scoreLb += scoreActuel;
            
            yield return null;
        }
        
        if (scoreText != null)
        {
            scoreText.text = scoreFinal + "%";
        }
    }
    
    List<DialogueLine> GenererDialoguesConclusion()
    {
        List<DialogueLine> dialogues = new List<DialogueLine>();
        
        // Commentaire sur le score
        if (scoreFinal >= 85)
        {
            if (niveauActuel >= 4)
            {
                dialogues.Add(new DialogueLine("Chef du Conseil", "Remarquable ! Vous maîtrisez parfaitement votre métier."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Votre expertise fait honneur à notre établissement."));
            }
            else
            {
                dialogues.Add(new DialogueLine("Chef du Conseil", "Excellent travail !"));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Vous avez parfaitement compris les enjeux de cet audit."));
            }
            dialogues.Add(new DialogueLine("Chef du Conseil", "Nous sommes impressionnés par votre rigueur."));
        }
        else if (scoreFinal >= 75)
        {
            if (niveauActuel >= 3)
            {
                dialogues.Add(new DialogueLine("Chef du Conseil", "Très bien, mais nous attendions un peu plus à ce niveau."));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Continuez à vous perfectionner."));
            }
            else
            {
                dialogues.Add(new DialogueLine("Chef du Conseil", "Très bien !"));
                dialogues.Add(new DialogueLine("Chef du Conseil", "Votre rapport est solide, continuez sur cette lancée."));
            }
        }
        else if (scoreFinal >= 50)
        {
            dialogues.Add(new DialogueLine("Chef du Conseil", "C'est correct..."));
            dialogues.Add(new DialogueLine("Chef du Conseil", "Mais il y a encore des points à améliorer."));
            
            if (niveauActuel >= 3)
            {
                dialogues.Add(new DialogueLine("Chef du Conseil", "À ce stade, nous espérions mieux de votre part."));
            }
            else
            {
                dialogues.Add(new DialogueLine("Chef du Conseil", "Soyez plus attentif aux détails la prochaine fois."));
            }
        }
        else
        {
            dialogues.Add(new DialogueLine("Chef du Conseil", "Hmm... Ce rapport nécessite plus de rigueur."));
            dialogues.Add(new DialogueLine("Chef du Conseil", "Prenez le temps d'analyser les informations avec attention."));
            
            if (niveauActuel >= 3)
            {
                dialogues.Add(new DialogueLine("Chef du Conseil", "C'est décevant pour un auditeur de votre niveau."));
            }
            else
            {
                dialogues.Add(new DialogueLine("Chef du Conseil", "Vous pouvez faire beaucoup mieux."));
            }
        }
        
        // Message de fin selon le niveau
        if (niveauActuel >= 5)
        {
            dialogues.Add(new DialogueLine("Chef du Conseil", "C'était votre dernière mission. Félicitations pour votre parcours."));
        }
        else
        {
            dialogues.Add(new DialogueLine("Chef du Conseil", "Vous pouvez maintenant continuer votre mission."));
        }
        
        return dialogues;
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
        /*
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.PasserNiveauSuivant();
        }*/
        
        GameStateManager.Instance.DoTerminerNiveauApresRapport = true;
        SceneManager.LoadScene("Map");
    }
    
    void RetourMenu()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ResetEtat();
        }
        
        SceneManager.LoadScene("MainMenu");
    }

    void Leaderboard()
    {
        // On dépose le score dans le manager persistant
        if (GameStateManager.Instance != null)
        {
			Debug.Log($"Envoi du score total au leaderboard : {GameStateManager.Instance.ScoreTotalCumule}");
        }

        // On change de scène
        SceneManager.LoadScene("Leaderboard");
    }

}