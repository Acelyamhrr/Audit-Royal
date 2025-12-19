using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Leaderboards.Exceptions;

public class LeaderboardsManager : MonoBehaviour
{
    private const string clefPseudo = "NomDuJoueur"; 

    [Header("UI References")]
    [SerializeField] private GameObject leaderboardParent;
    [SerializeField] private Transform leaderboardContentParent;
    [SerializeField] private Transform leaderboardItemPrefab;
    
    [Header("Background Sprites")]
    [SerializeField] private Sprite defaultBackgroundSprite;
    [SerializeField] private Sprite bronzeTierBackground;
    [SerializeField] private Sprite silverTierBackground;
    [SerializeField] private Sprite goldenTierBackground;

    private string leaderboardID = "lbcall";
    private string pseudoActuel;
    private int level;

    private async void Start()
    {
        // 1. Initialisation
        await UnityServices.InitializeAsync();
    
        // 2. Connexion (si pas déjà fait)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // 3. RÉCUPÉRATION DU SCORE DE TEST
        if (GameStateManager.Instance != null && GameStateManager.Instance.ScoreTotalCumule > 0)
        {
            Debug.Log("Score détecté dans le Manager : " + GameStateManager.Instance.ScoreTotalCumule);
            // On l'envoie direct !
            SoumettreScoreFinal(GameStateManager.Instance.ScoreTotalCumule, 5);
        
            // Optionnel : On remet à zéro pour pas renvoyer le même score en boucle
            GameStateManager.Instance.ScoreTotalCumule = 0;
        }
        else {
            // Si on lance la scène Leaderboard directement, on affiche juste
            AfficherClassement();
        }
    }

    // --- AJOUT DE LA FONCTION UPDATE POUR LE TEST ---
    void Update()
    {
        // Si tu appuies sur T en jeu, ça envoie un score de 95% direct pour tester
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Test rapide activé ! Envoi de 95%");
            SoumettreScoreFinal(95, 5);
        }
    }

    // --- BOUTON TEST DANS L'INSPECTOR ---
    [ContextMenu("Test : Envoyer 50%")]
    public void TestViteFait()
    {
        SoumettreScoreFinal(50, 1);
    }

    public async Task MettreAJourPseudo(string nouveauPseudo)
    {
        if (string.IsNullOrWhiteSpace(nouveauPseudo)) return;
        
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(nouveauPseudo);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Erreur pseudo : " + ex.Message);
        }
    }

    public async void SoumettreScoreFinal(int score, int level)
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;
        this.level = level;
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, score);
            AfficherClassement(); 
        }
        catch (LeaderboardsException e)
        {
            Debug.LogError("Erreur soumission score : " + e.Reason);
        }
    }

    public async void AfficherClassement()
    {
        foreach (Transform child in leaderboardContentParent)
        {
            Destroy(child.gameObject);
        }

        try 
        {
            var options = new GetScoresOptions { Limit = 10 };
            LeaderboardScoresPage scoresPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID, options);
         
            foreach (LeaderboardEntry entry in scoresPage.Results)
            {
                Transform item = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
                 
                Image bg = item.GetComponent<Image>();
                Sprite bgSprite = defaultBackgroundSprite;

                switch (entry.Tier)
                {
                    case "bronze_tier": bgSprite = bronzeTierBackground; break;
                    case "silver_tier": bgSprite = silverTierBackground; break;
                    case "golden_tier": bgSprite = goldenTierBackground; break;
                }
                
                if (bg != null) bg.sprite = bgSprite;
                
                item.GetChild(0).GetComponent<TextMeshProUGUI>().text = entry.PlayerName;
                item.GetChild(1).GetComponent<TextMeshProUGUI>().text = entry.Score.ToString() + "%";
                item.GetChild(2).GetComponent<TextMeshProUGUI>().text = level.ToString();
            }
        }
        catch (LeaderboardsException e)
        {
            Debug.LogError("Erreur récupération classement : " + e.Reason);
        }
    }
}