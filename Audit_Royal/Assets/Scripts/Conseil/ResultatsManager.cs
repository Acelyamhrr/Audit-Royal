using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestionnaire de la scène de résultats d'audit.
/// Gère l'affichage des dialogues, l'animation du score, les cinématiques et la navigation.
/// </summary>
public class ResultatsManager : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Dialogue UI")]
    [Tooltip("Texte affichant le nom du personnage qui parle")]
    public TextMeshProUGUI speakerNameText;
    
    [Tooltip("Texte du dialogue principal avec effet de frappe")]
    public TextMeshProUGUI dialogueText;
    
    [Tooltip("Panel contenant l'interface de dialogue")]
    public GameObject dialoguePanel;
    
    [Header("Score UI")]
    [Tooltip("Texte affichant le score en pourcentage")]
    public TextMeshProUGUI scoreText;
    
    [Tooltip("Image du tampon 'VALIDÉ' qui s'affiche si score >= 85%")]
    public GameObject tamponValide;
    
    [Tooltip("Bouton permettant de continuer vers le prochain niveau")]
    public Button continueButton;
    
    [Header("Cinematic")]
    [Tooltip("Contrôleur gérant les effets cinématiques (barres noires)")]
    public CinematicController cinematicController;
    
    [Header("Settings")]
    [Tooltip("Vitesse normale d'affichage du texte (secondes par caractère)")]
    public float typingSpeed = 0.05f;
    
    [Tooltip("Vitesse rapide d'affichage quand l'utilisateur appuie une fois")]
    public float fastTypingSpeed = 0.01f;
    
    [Tooltip("Durée de l'animation du score (de 0 au score final)")]
    public float scoreAnimationDuration = 2f;
    
    [Tooltip("Durée d'affichage des nombres aléatoires avant le score")]
    public float randomNumbersDuration = 1.5f;
    
    [Tooltip("Vitesse de changement des nombres aléatoires")]
    public float randomNumbersSpeed = 0.08f;
    
    [Tooltip("Touche pour passer au dialogue suivant ou accélérer le texte")]
    public KeyCode nextDialogueKey = KeyCode.Space;
    
    #endregion
    
    #region Private Fields
    
    /// <summary>Score final obtenu par le joueur (en pourcentage)</summary>
    private int scoreFinal;
    
    /// <summary>Niveau actuel du joueur (1-5)</summary>
    private int niveauActuel;
    
    /// <summary>File d'attente contenant les lignes de dialogue à afficher</summary>
    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    
    /// <summary>Indique si le texte est en train d'être affiché caractère par caractère</summary>
    private bool isTyping = false;
    
    /// <summary>Indique si le mode d'affichage rapide est activé</summary>
    private bool isFastTyping = false;  // Mode rapide activé
    
    /// <summary>Indique si le joueur peut passer au dialogue suivant</summary>
    private bool canAdvance = false;
    
    /// <summary>Référence vers la coroutine d'affichage du texte en cours</summary>
    private Coroutine typingCoroutine;
    
    /// <summary>Texte complet de la ligne de dialogue actuellement affichée</summary>
    private string currentFullText = ""; 
    
    /// <summary>
    /// Phases de la séquence de résultats
    /// </summary>
    private enum Phase
    {
        Introduction,           // dialogues d'introduction
        WaitingForCinematic,    // en attente du démarrage de la cinématique 
        InCinematic,            // pendant l'affichage du score avec cinématique
        Conclusion,             // dialogues de conclusion
        Finished                // séquence terminée, affichage du bouton
    }
    
    /// <summary>Phase actuelle de la séquence</summary>
    private Phase currentPhase = Phase.Introduction;
    
    /// <summary>
    /// Représente une ligne de dialogue avec son locuteur et son texte
    /// </summary>
    [System.Serializable]
    private class DialogueLine
    {
        public string speakerName;      // nom du personnage
        public string text;             // texte du dialogue
        
        /// <summary>
        /// Constructeur d'une ligne de dialogue
        /// </summary>
        /// <param name="speaker">Nom du personnage qui parle</param>
        /// <param name="dialogue">Texte du dialogue</param>
        public DialogueLine(string speaker, string dialogue)
        {
            speakerName = speaker;
            text = dialogue;
        }
    }
    
    #endregion

    #region Unity Lifecycle
    
    /// <summary>
    /// Initialisation au démarrage de la scène
    /// Récupère le score et le niveau, initialise l'interface
    /// </summary>
    void Start()
    {
        // Récupérer le score et le niveau
        if (GameStateManager.Instance != null)
        {
            scoreFinal = GameStateManager.Instance.ScoreDernierRapport;
            niveauActuel = GameStateManager.Instance.NiveauActuel;
            Debug.Log($"Score récupéré : {scoreFinal}% - Niveau : {niveauActuel}");
        }
        else
        {
            // Valeurs de test si le manager n'existe pas
            scoreFinal = 90;
            niveauActuel = 1;
            Debug.LogWarning("GameStateManager introuvable, score de test : 90% - Niv 1");
        }
        
        // Masquer le tampon "VALIDÉ" au départ
        if (tamponValide != null)
        {
            
            tamponValide.SetActive(false);
        }

        // Masquer le bouton "Continuer" au départ
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
        
        // Initialiser le score à 0% (sera animé plus tard)
        if (scoreText != null)
        {
            scoreText.text = "0%";
        }
        
        // Démarrer la séquence d'introduction
        StartIntroduction();
    }
    
    /// <summary>
    /// Gestion des inputs utilisateur chaque frame
    /// </summary>
    void Update()
    {
        // Détection de l'appui sur la touche de progression
        if (Input.GetKeyDown(nextDialogueKey))
        {
            if (isTyping && !isFastTyping)
            {
                // Premier appui : accélérer
                isFastTyping = true;
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
    
    #endregion

    #region Dialogue Management
    
    /// <summary>
    /// Démarre la phase d'introduction avec les dialogues contextuels
    /// </summary>
    void StartIntroduction()
    {
        currentPhase = Phase.Introduction;
    
        // Générer les dialogues d'introduction selon le niveau
        List<DialogueLine> introDialogues = GenererDialoguesIntroduction();
        StartDialogueSequence(introDialogues);
        
    }
    
    /// <summary>
    /// Génère les dialogues d'introduction adaptés au niveau actuel
    /// </summary>
    /// <returns>Liste des lignes de dialogue d'introduction</returns>
    List<DialogueLine> GenererDialoguesIntroduction()
    {
        List<DialogueLine> dialogues = new List<DialogueLine>();
        
        // Dialogues personnalisés selon le niveau
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
    
    /// <summary>
    /// Démarre une séquence de dialogues à partir d'une liste
    /// </summary>
    /// <param name="dialogues">Liste des dialogues à afficher</param>
    void StartDialogueSequence(List<DialogueLine> dialogues)
    {
        // Vider la file d'attente et remplir avec les nouveaux dialogues
        dialogueQueue.Clear();
        
        foreach (var line in dialogues)
        {
            dialogueQueue.Enqueue(line);
        }
        
        // Afficher le panel de dialogue
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        // Commencer à afficher le premier dialogue
        DisplayNextDialogue();
    }
    
    /// <summary>
    /// Affiche le prochain dialogue de la file d'attente
    /// </summary>
    void DisplayNextDialogue()
    {
        // Si plus de dialogues, passer à la phase suivante
        if (dialogueQueue.Count == 0)
        {
            OnDialoguePhaseFinished();
            return;
        }
        
        // Récupérer et afficher le prochain dialogue
        DialogueLine line = dialogueQueue.Dequeue();
        currentFullText = line.text;  // Sauvegarder le texte complet
        isFastTyping = false;  // Réinitialiser le mode rapide
        
        // Afficher le nom du locuteur
        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;
        
        // Arrêter la coroutine précédente si elle existe
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        // Démarrer l'animation d'affichage du texte
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }
    
    /// <summary>
    /// Coroutine affichant le texte caractère par caractère (effet machine à écrire)
    /// </summary>
    /// <param name="text">Texte à afficher</param>
    IEnumerator TypeText(string text)
    {
        isTyping = true;
        canAdvance = false;
        dialogueText.text = "";
        
        // Afficher chaque caractère un par un
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            
            // Utiliser la vitesse rapide si le mode est activé
            float currentSpeed = isFastTyping ? fastTypingSpeed : typingSpeed;
            yield return new WaitForSeconds(currentSpeed);
        }
        
        // Affichage terminé
        isTyping = false;
        isFastTyping = false;
        canAdvance = true;
    }
    
    /// <summary>
    /// Arrête l'animation de frappe et affiche tout le texte immédiatement
    /// </summary>
    void StopTypingAndShowAll()
    {
        // Arrêter la coroutine d'affichage
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
    
    /// <summary>
    /// Appelé quand une phase de dialogue est terminée
    /// Détermine quelle est la prochaine étape
    /// </summary>
    void OnDialoguePhaseFinished()
    {
        switch (currentPhase)
        {
            case Phase.Introduction:
                // Après l'intro : lancer la cinématique du score
                StartCoroutine(StartCinematicSequence());
                break;
                
            case Phase.Conclusion:
                // Après la conclusion : afficher le bouton de continuation
                ShowContinueButton();
                break;
        }
    }
    
    #endregion
    
    #region Cinematic & Score Animation
    
    /// <summary>
    /// Coroutine gérant la séquence cinématique complète du score
    /// 1. Masquer le dialogue
    /// 2. Activer les barres cinématiques
    /// 3. Animer le score avec nombres aléatoires puis score final
    /// 4. Afficher le tampon si score >= 85%
    /// 5. Sortir de la cinématique
    /// 6. Passer à la conclusion
    /// </summary>
    IEnumerator StartCinematicSequence()
    {
        currentPhase = Phase.WaitingForCinematic;
        canAdvance = false;
        
        yield return new WaitForSeconds(0.5f);
        
        // Masquer le panneau de dialogue pour voir le score
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
    
    /// <summary>
    /// Coroutine affichant des nombres aléatoires pour créer du suspense
    /// avant l'affichage du score final
    /// </summary>
    IEnumerator AnimerNombresAleatoires()
    {
        float tempsEcoule = 0f;
        
        // Afficher des nombres aléatoires pendant la durée définie
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
    
    /// <summary>
    /// Coroutine animant progressivement le score de 0 au score final
    /// </summary>
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
    
    /// <summary>
    /// Coroutine animant l'apparition du tampon "VALIDÉ"
    /// avec effet d'échelle et de rotation
    /// </summary>
    IEnumerator AnimerTampon()
    {
        float duree = 1.0f;
        float temps = 0f;
        
        Transform tamponTransform = tamponValide.transform;
        Vector3 echelleDepart = Vector3.zero;                       // commence invisible
        Vector3 echelleFin = Vector3.one;                           // taille normale
        Quaternion rotationDepart = Quaternion.Euler(0, 0, -15f);   // légère rotation gauche
        Quaternion rotationFin = Quaternion.Euler(0, 0, 15f);       // légère rotation droite
        
        // Initialiser la transformation
        tamponTransform.localScale = echelleDepart;
        tamponTransform.rotation = rotationDepart;
        
        // Animation progressive
        while (temps < duree)
        {
            temps += Time.deltaTime;
            float progression = temps / duree;
            
            tamponTransform.localScale = Vector3.Lerp(echelleDepart, echelleFin, progression);
            tamponTransform.rotation = Quaternion.Lerp(rotationDepart, rotationFin, progression);
            
            yield return null;
        }
        
        // S'assurer que les valeurs finales sont exactes
        tamponTransform.localScale = echelleFin;
        tamponTransform.rotation = rotationFin;
    }

    #endregion

    #region Conclusion & Navigation
    
    /// <summary>
    /// Démarre la phase de conclusion avec les dialogues finaux
    /// </summary>
    void StartConclusion()
    {
        currentPhase = Phase.Conclusion;
        
        // Générer les dialogues de conclusion selon le score et le niveau
        List<DialogueLine> conclusionDialogues = GenererDialoguesConclusion();
        StartDialogueSequence(conclusionDialogues);

    }
    
    /// <summary>
    /// Génère les dialogues de conclusion selon le score obtenu et le niveau
    /// </summary>
    /// <returns>Liste des lignes de dialogue de conclusion</returns>
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
    
    /// <summary>
    /// Affiche le bouton de continuation et configure son action
    /// selon le niveau (continuer ou retour menu)
    /// </summary>
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
                continueButton.onClick.AddListener(RetourMenu);
            }
            else
            {
                continueButton.onClick.AddListener(PasserNiveauSuivant);
            }
        }

    }
    
    /// <summary>
    /// Charge la scène de la carte pour passer au niveau suivant
    /// </summary>
    void PasserNiveauSuivant()
    {
        // Indiquer au GameStateManager qu'il doit terminer le niveau
        GameStateManager.Instance.DoTerminerNiveauApresRapport = true;
        SceneManager.LoadScene("Map");
    }
    
    /// <summary>
    /// Retourne au menu principal et réinitialise l'état du jeu
    /// </summary>
    void RetourMenu()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ResetEtat();
        }
        
        SceneManager.LoadScene("MainMenu");
    }
    
    #endregion

}