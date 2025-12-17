using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DebriefingManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject salleReunionComplete;
    public GameObject chefZoomDramatique;
    public GameObject overlayDark;
    public TextMeshProUGUI scoreDisplay;
    public Slider scoreGauge;
    public Button continueButton;
    public ParticleSystem confettiEffect;

    [Header("Dialogue System")]
    public DialogueManager dialogueManager;
    public CinematicController cinematicController;

    [Header("Animation Settings")]
    public float scoreAnimationDuration = 2f;
    public float suspenseDuration = 2f;

    private int playerScore;
    private int currentLevel;
    private bool canSkip = false;
    private Coroutine currentSequence;

    void Start()
    {
        // Récupérer le score et le niveau
        if (GameStateManager.Instance != null)
        {
            playerScore = GameStateManager.Instance.DernierScoreRapport;
            currentLevel = GameStateManager.Instance.NiveauActuel;
        }
        else
        {
            Debug.LogWarning("GameStateManager non trouvé ! Score par défaut : 75%");
            playerScore = 75;
            currentLevel = 1;
        }

        // Initialiser l'UI
        InitializeUI();

        // Configurer le bouton
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        // Callbacks du dialogue
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueEnd = OnDialogueFinished;
        }

        // Démarrer la séquence
        currentSequence = StartCoroutine(DebriefingSequence());
    }

    void Update()
    {
        // Permettre de skip avec Espace
        if (canSkip && Input.GetKeyDown(KeyCode.Space))
        {
            OnContinueClicked();
        }
    }

    void InitializeUI()
    {
        if (salleReunionComplete != null)
            salleReunionComplete.SetActive(true);
        
        if (chefZoomDramatique != null)
            chefZoomDramatique.SetActive(false);
        
        if (overlayDark != null)
            overlayDark.SetActive(false);
        
        if (scoreDisplay != null)
            scoreDisplay.gameObject.SetActive(false);
        
        if (scoreGauge != null)
            scoreGauge.gameObject.SetActive(false);
        
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
        
        if (confettiEffect != null)
            confettiEffect.gameObject.SetActive(false);
    }

    IEnumerator DebriefingSequence()
    {
        // PHASE 1 : Dialogue d'introduction (SANS cinématique)
        yield return new WaitForSeconds(1f);

        string[] introLines = new string[]
        {
            "Bonjour. Nous avons examiné votre rapport avec attention.",
            "Voyons maintenant vos résultats..."
        };

        dialogueManager.SetSpeaker("Chef");
        dialogueManager.isIntroDialogue = false; // Pas de choix ici
        dialogueManager.StartDialogue(introLines, withCinematic: false);

        // Attendre la fin du dialogue
        while (dialogueManager.IsActive())
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // PHASE 2 : Suspense - Zoom dramatique AVEC cinématique
        yield return StartCoroutine(ZoomDramatiqueTransition());

        // PHASE 3 : Révélation du score
        yield return StartCoroutine(RevealScore());

        // PHASE 4 : Retour à la vue normale
        yield return StartCoroutine(RetourVueNormale());

        // PHASE 5 : Commentaire du chef (SANS cinématique)
        string[] commentaireLines = new string[]
        {
            GetCommentaireSelonScore(playerScore)
        };

        dialogueManager.StartDialogue(commentaireLines, withCinematic: false);

        while (dialogueManager.IsActive())
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // PHASE 6 : Transition vers le prochain niveau
        yield return TransitionProchainNiveau();

        // Activer le bouton pour continuer
        canSkip = true;
        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
    }

    IEnumerator ZoomDramatiqueTransition()
    {
        // Activer le mode cinématique pour le suspense
        if (cinematicController != null)
        {
            cinematicController.EnterCinematic();
            yield return new WaitForSeconds(2.8f);
        }

        // Activer l'overlay et le zoom du chef
        if (overlayDark != null)
            overlayDark.SetActive(true);
        
        if (chefZoomDramatique != null)
            chefZoomDramatique.SetActive(true);

        // Fondu de l'overlay
        if (overlayDark != null)
        {
            CanvasGroup overlayGroup = overlayDark.GetComponent<CanvasGroup>();
            if (overlayGroup == null)
                overlayGroup = overlayDark.AddComponent<CanvasGroup>();

            overlayGroup.alpha = 0;

            float elapsed = 0;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                overlayGroup.alpha = Mathf.Lerp(0, 1, elapsed);
                yield return null;
            }
        }

        yield return new WaitForSeconds(suspenseDuration);
    }

    IEnumerator RevealScore()
    {
        if (scoreDisplay != null)
            scoreDisplay.gameObject.SetActive(true);
        
        if (scoreGauge != null)
            scoreGauge.gameObject.SetActive(true);

        float elapsed = 0;
        int currentScore = 0;

        while (elapsed < scoreAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scoreAnimationDuration;
            currentScore = Mathf.RoundToInt(Mathf.Lerp(0, playerScore, progress));

            if (scoreDisplay != null)
                scoreDisplay.text = currentScore + "%";
            
            if (scoreGauge != null)
                scoreGauge.value = currentScore;

            Color scoreColor = GetColorForScore(currentScore);
            if (scoreDisplay != null)
                scoreDisplay.color = scoreColor;

            if (scoreGauge != null && scoreGauge.fillRect != null)
            {
                Image fillImage = scoreGauge.fillRect.GetComponent<Image>();
                if (fillImage != null)
                    fillImage.color = scoreColor;
            }

            yield return null;
        }

        if (scoreDisplay != null)
            scoreDisplay.text = playerScore + "%";
        
        if (scoreGauge != null)
            scoreGauge.value = playerScore;

        if (playerScore >= 85 && confettiEffect != null)
        {
            confettiEffect.gameObject.SetActive(true);
            confettiEffect.Play();
        }

        yield return new WaitForSeconds(2f);
    }

    IEnumerator RetourVueNormale()
    {
        if (cinematicController != null)
        {
            cinematicController.ExitCinematic();
            yield return new WaitForSeconds(2.8f);
        }

        if (overlayDark != null)
        {
            CanvasGroup overlayGroup = overlayDark.GetComponent<CanvasGroup>();

            if (overlayGroup != null)
            {
                float elapsed = 0;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    overlayGroup.alpha = Mathf.Lerp(1, 0, elapsed / 0.5f);
                    yield return null;
                }
            }
        }

        if (chefZoomDramatique != null)
            chefZoomDramatique.SetActive(false);
        
        if (overlayDark != null)
            overlayDark.SetActive(false);

        if (scoreDisplay != null)
            scoreDisplay.gameObject.SetActive(false);
        
        if (scoreGauge != null)
            scoreGauge.gameObject.SetActive(false);
    }

    IEnumerator TransitionProchainNiveau()
    {
        string[] transitionLines;

        if (currentLevel < 5)
        {
            transitionLines = new string[]
            {
                $"Nous passons maintenant au niveau {currentLevel + 1}.",
                "La difficulté augmente. Vous aurez accès à plus de services et de questions."
            };
        }
        else
        {
            transitionLines = new string[]
            {
                "Félicitations ! Vous avez terminé tous les niveaux de cet audit.",
                "Votre travail est exemplaire."
            };
        }

        dialogueManager.StartDialogue(transitionLines, withCinematic: false);

        while (dialogueManager.IsActive())
        {
            yield return null;
        }
    }

    string GetCommentaireSelonScore(int score)
    {
        if (score >= 90)
            return "Remarquable ! Votre analyse est précise et vos conclusions irréprochables. Vous avez su démêler le vrai du faux avec brio.";
        else if (score >= 75)
            return "Bon travail. Quelques petites imprécisions, mais dans l'ensemble, votre rapport est solide et exploitable.";
        else if (score >= 60)
            return "C'est correct, mais il y a des zones d'ombre. Vous auriez pu creuser davantage certains témoignages.";
        else if (score >= 40)
            return "Hmm... Votre rapport manque de rigueur. Plusieurs erreurs d'appréciation remettent en question vos conclusions.";
        else
            return "C'est préoccupant. Vous avez confondu vérités et mensonges. Un audit ne peut pas reposer sur des bases aussi fragiles.";
    }

    Color GetColorForScore(int score)
    {
        if (score >= 85)
            return new Color(0, 1, 0);
        else if (score >= 60)
            return new Color(1, 0.65f, 0);
        else
            return new Color(1, 0, 0);
    }

    void OnDialogueFinished()
    {
        Debug.Log("Dialogue terminé !");
    }

    void OnContinueClicked()
    {
        if (currentSequence != null)
            StopCoroutine(currentSequence);

        if (dialogueManager != null)
            dialogueManager.ForceEnd();

        if (GameStateManager.Instance != null)
        {
            if (currentLevel < 5)
            {
                GameStateManager.Instance.NiveauActuel++;
                GameStateManager.Instance.DoTerminerNiveauApresRapport = false;
                
                ScenarioManager scenarioManager = FindFirstObjectByType<ScenarioManager>();
                if (scenarioManager != null)
                {
                    scenarioManager.GenerateVeritesFile(
                        GameStateManager.Instance.ScenarioActuel,
                        GameStateManager.Instance.NiveauActuel
                    );
                }

                SceneManager.LoadScene("Map");
            }
            else
            {
                SceneManager.LoadScene("MenuPrincipal");
            }
        }
    }
}