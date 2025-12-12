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

public class LeaderboardsManager : MonoBehaviour
{

    private const string clefPseudo = "NomDuJoueur"; 

    [Header("UI References")]
    [SerializeField] private GameObject leaderboardParent;
    [SerializeField] private Transform leaderboardContentParent;
    [SerializeField] private Transform leaderboardItemPrefab;
    
    [Header("Tier Sprites")]
    [SerializeField] private Sprite bronzeTierSprite;
    [SerializeField] private Sprite silverTierSprite;
    [SerializeField] private Sprite goldenTierSprite;

    [SerializeField] private Sprite defaultBackgroundSprite;
    [SerializeField] private Sprite bronzeTierBackground;
    [SerializeField] private Sprite silverTierBackground;
    [SerializeField] private Sprite goldenTierBackground;


    private string leaderboardID = "lbcall";
    

    private string pseudoActuel;

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
        
        Debug.Log("Leaderboard initialisé. Pseudo du joueur : " + pseudoActuel);
    }

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
                    Debug.Log("Score 100 soumis pour " + pseudoActuel);
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
                Debug.Log("Pseudo mis à jour sur le serveur : " + nouveauPseudo);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Erreur lors de la mise à jour du pseudo : " + ex.Message);
            }
        }
    }
    
    private async void UpdateLeaderboard()
    {
        while (Application.isPlaying && leaderboardParent.activeInHierarchy)
        {
            LeaderboardScoresPage leaderboardScoresPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID);
            
            foreach (Transform t in leaderboardContentParent)
            {
                Destroy(t.gameObject);
            }
            foreach (LeaderboardEntry entry in leaderboardScoresPage.Results)
            {
                 Transform leaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
                 
                Image itemBackground = leaderboardItem.GetComponent<Image>();
                Sprite tierSprite = null;
                Sprite backgroundSprite = defaultBackgroundSprite;

                switch (entry.Tier)
                {
                    case "bronze_tier":
                        tierSprite = bronzeTierSprite;
                        backgroundSprite = bronzeTierBackground;
                        break;
                    case "silver_tier":
                        tierSprite = silverTierSprite;
                        backgroundSprite = silverTierBackground;
                        break;
                    case "golden_tier":
                        tierSprite = goldenTierSprite;
                        backgroundSprite = goldenTierBackground;
                        break;
                    default:
                        break;
                }
                
                if (itemBackground != null)
                {
                    itemBackground.sprite = backgroundSprite;
                }

                leaderboardItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = entry.PlayerName;
                leaderboardItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = entry.Score.ToString();
                leaderboardItem.GetChild(2).GetComponent<TextMeshProUGUI>().text = entry.Tier;
                //leaderboardItem.GetChild(2).GetComponent<Image>().sprite = tierSprite;
            }

            await Task.Delay(500); 
        }
    }

    public async void AjouterScoresDeTest(int nombreDeSoumissions)
    {
        Debug.Log("Soumission de " + nombreDeSoumissions + " scores de test pour le joueur local...");
        
        string leaderboardId = "lbcall"; 
        
        for (int i = 1; i <= nombreDeSoumissions; i++)
        {
            int score = Random.Range(1000, 50000); 
            
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(
                    leaderboardId, 
                    score
                );
                Debug.Log("Score " + score + " soumis.");
            }
            catch (LeaderboardsException e)
            {
                Debug.LogError("Erreur lors de l'ajout du score : " + e.Reason);
            }
            
            await Task.Delay(50); 
        }
        
        Debug.Log("Simulation de scores terminée. Appuyez sur ESC pour voir le classement mis à jour.");
    }
}