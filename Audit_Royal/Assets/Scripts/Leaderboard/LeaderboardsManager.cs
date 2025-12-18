using System.Collections;
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

/// <summary>
/// Gère l'affichage, la mise à jour et la soumission des scores vers les services de Leaderboard d'Unity.
/// </summary>
public class LeaderboardsManager : MonoBehaviour
{
    private const string clefPseudo = "NomDuJoueur"; 

    [Header("UI References")]
    /// <summary> Objet parent contenant tout l'UI du classement. </summary>
    [SerializeField] private GameObject leaderboardParent;
    /// <summary> Le container (souvent un Content de ScrollView) où seront instanciés les scores. </summary>
    [SerializeField] private Transform leaderboardContentParent;
    /// <summary> Le prefab représentant une ligne dans le classement. </summary>
    [SerializeField] private Transform leaderboardItemPrefab;
    
    [Header("Tier Sprites")]
    [SerializeField] private Sprite bronzeTierSprite;
    [SerializeField] private Sprite silverTierSprite;
    [SerializeField] private Sprite goldenTierSprite;

    [SerializeField] private Sprite defaultBackgroundSprite;
    [SerializeField] private Sprite bronzeTierBackground;
    [SerializeField] private Sprite silverTierBackground;
    [SerializeField] private Sprite goldenTierBackground;

    /// <summary> L'ID du leaderboard configuré sur le dashboard Unity Services. </summary>
    private string leaderboardID = "lbcall";
    
    /// <summary> Pseudo local du joueur récupéré ou défini. </summary>
    private string pseudoActuel;

    /// <summary>
    /// Initialise les services Unity, connecte l'utilisateur anonymement et prépare le leaderboard.
    /// </summary>
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        
        pseudoActuel = PlayerPrefs.GetString(clefPseudo, "Invité Anonyme");
        
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        await MettreAJourPseudo(pseudoActuel);

        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, 0);
        }
        catch (LeaderboardsException e)
        {
            Debug.LogWarning("Erreur d'ajout de score initial : " + e.Reason);
        }
        
        leaderboardParent.SetActive(false);
    }

    /// <summary>
    /// Surveille les entrées clavier pour afficher le menu (Echap) ou ajouter des scores de test (Espace).
    /// </summary>
    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (leaderboardParent.activeInHierarchy)
            {
                leaderboardParent.SetActive(false);
            }
            else
            {
                leaderboardParent.SetActive(true);
                UpdateLeaderboard();
                
                try
                {
                    await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, 100); 
                }
                catch (LeaderboardsException e)
                {
                    Debug.LogWarning("Erreur lors de la soumission du score : " + e.Reason);
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AjouterScoresDeTest(10); 
        }
    }
 
    /// <summary>
    /// Met à jour le nom du joueur sur les serveurs d'authentification Unity.
    /// </summary>
    /// <param name="nouveauPseudo">Le nouveau nom à afficher dans le classement.</param>
    public async Task MettreAJourPseudo(string nouveauPseudo)
    {
        if (string.IsNullOrWhiteSpace(nouveauPseudo) || nouveauPseudo == AuthenticationService.Instance.PlayerName)
        {
            return;
        }
        
        if (AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(nouveauPseudo);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Erreur lors de la mise à jour du pseudo : " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Boucle de rafraîchissement du classement tant que le panneau est actif.
    /// Récupère les scores et instancie les éléments visuels correspondants.
    /// </summary>
    private async void UpdateLeaderboard()
    {
        while (Application.isPlaying && leaderboardParent.activeInHierarchy)
        {
            LeaderboardScoresPage leaderboardScoresPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID);
            
            // Nettoyage de l'ancienne liste
            foreach (Transform t in leaderboardContentParent)
            {
                Destroy(t.gameObject);
            }

            // Création des nouveaux items
            foreach (LeaderboardEntry entry in leaderboardScoresPage.Results)
            {
                Transform leaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
                 
                Image itemBackground = leaderboardItem.GetComponent<Image>();
                Sprite backgroundSprite = defaultBackgroundSprite;

                switch (entry.Tier)
                {
                    case "bronze_tier":
                        backgroundSprite = bronzeTierBackground;
                        break;
                    case "silver_tier":
                        backgroundSprite = silverTierBackground;
                        break;
                    case "golden_tier":
                        backgroundSprite = goldenTierBackground;
                        break;
                }
                
                if (itemBackground != null)
                {
                    itemBackground.sprite = backgroundSprite;
                }

                leaderboardItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = entry.PlayerName;
                leaderboardItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = entry.Score.ToString();
                leaderboardItem.GetChild(2).GetComponent<TextMeshProUGUI>().text = entry.Tier;
            }

            await Task.Delay(500); 
        }
    }

    /// <summary>
    /// Génère et envoie plusieurs scores aléatoires pour tester le comportement du classement.
    /// </summary>
    /// <param name="nombreDeSoumissions">Nombre de scores fictifs à envoyer.</param>
    public async void AjouterScoresDeTest(int nombreDeSoumissions)
    {
        string leaderboardId = "lbcall"; 
        
        for (int i = 1; i <= nombreDeSoumissions; i++)
        {
            int score = Random.Range(1000, 50000); 
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            }
            catch (LeaderboardsException e)
            {
                Debug.LogError("Erreur lors de l'ajout du score : " + e.Reason);
            }
            await Task.Delay(50); 
        }
    }
}