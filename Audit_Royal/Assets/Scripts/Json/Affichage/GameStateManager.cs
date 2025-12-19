using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère l'état actuel du jeu, les personnages et les services.
/// </summary>
/// <remarks>
/// Cette classe est un singleton persistant qui conserve :
/// <list type="bullet">
/// <item><description>Le scénario et le niveau actuel</description></item>
/// <item><description>Le service et le poste sélectionnés</description></item>
/// <item><description>Le personnage actuellement sélectionné</description></item>
/// <item><description>La correspondance bâtiment → service et scène → personnage</description></item>
/// </list>
/// </remarks>
public class GameStateManager : MonoBehaviour
{
    /// <summary>
    /// Instance singleton accessible globalement.
    /// </summary>
    public static GameStateManager Instance { get; private set; }
    
    /// <summary>
    /// Indique si le niveau doit se terminer après le rapport.
    /// </summary>
    public bool DoTerminerNiveauApresRapport = false;
    public int ScoreTotalCumule;

    //public int ScoreDernierRapport { get; set; } = 0;
    public int ScoreDernierRapport = 0;
    
    
    // État du jeu
    /// <summary>
    /// Scénario actuel.
    /// </summary>
    public int ScenarioActuel { get; private set; } = 0;
    
    /// <summary>
    /// Niveau actuel.
    /// </summary>
    public int NiveauActuel { get; private set; } = 1;
    
    /// <summary>
    /// Service actuellement sélectionné.
    /// </summary>
    public string ServiceActuel { get; set; } = "";
    
    /// <summary>
    /// Poste actuellement sélectionné.
    /// </summary>
    public string PosteActuel { get; set; } = "";
    
    /// <summary>
    /// Fichier JSON du personnage actuellement sélectionné.
    /// </summary>
    public string FichierPersonnageActuel { get; set; } = "";
    
    /// <summary>
    /// Mapping bâtiment → service.
    /// </summary>
    private Dictionary<string, string> batimentVersService = new Dictionary<string, string>()
    {
        { "Crous", "restauration" },
        { "Informatique", "info" },
        { "Compta", "comptabilite" },
        { "Communication", "communication" },
        { "Techniciens", "technicien" },
    };
    
    /// <summary>
    /// Mapping scène → fichier JSON du personnage.
    /// </summary>
    private Dictionary<string, string> sceneVersPersonnage = new Dictionary<string, string>()
    {
        // Communication
        { "DirectorCom", "com_responsable_reseaux_sociaux.json" },
        { "SecretaireCom", "com_graphiste.json" },
        { "Technicien", "com_video.json" },
        
        // Info
        { "Director", "info_patron.json" },
        { "Reseau", "info_responsable_reseau.json" },
        { "Secretaire", "info_secretaire.json" },
        { "TechnicienInfo", "info_technicien_de_maintenance.json" },
        
        // Restauration
        { "Chef", "res_patron.json" },
        { "Cuisinier", "res_cuisinier.json" },
        
        // Comptabilité
        { "ComptaPatron", "compta_patron.json" },
        { "ComptaComptable", "compta_comptable.json" },
        { "ComptaSecretaire", "compta_secretaire.json" },
        
        // Technicien/GC
        { "GCPatron", "gc_patron.json" },
        { "GCConcierge", "gc_concierge.json" },
        { "GCPaysagiste", "gc_paysagiste.json" },
        { "GCSecretaire", "gc_secretaire.json" },
    };

    /// <summary>
    /// Initialise l'instance singleton et rend le GameObject persistant.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameStateManager créé et persistant");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Appelé lors de l'entrée dans un bâtiment pour définir le service associé.
    /// </summary>
    /// <param name="nomScene">Nom de la scène/bâtiment.</param>
    public void EntrerDansBatiment(string nomScene)
    {
        if (batimentVersService.ContainsKey(nomScene))
        {
            ServiceActuel = batimentVersService[nomScene];
            Debug.Log($"Entrée dans le bâtiment : {nomScene} → Service : {ServiceActuel}");
        }
        else
        {
            Debug.LogWarning($"Bâtiment inconnu : {nomScene}");
        }
    }

    /// <summary>
    /// Appelé lors de la sélection d’un personnage pour récupérer le fichier JSON associé.
    /// </summary>
    /// <param name="nomScene">Nom de la scène représentant le personnage.</param>
    public void SelectionnerPersonnage(string nomScene)
    {
        if (sceneVersPersonnage.ContainsKey(nomScene))
        {
            FichierPersonnageActuel = sceneVersPersonnage[nomScene];
            Debug.Log($"Personnage sélectionné : {nomScene} → Fichier : {FichierPersonnageActuel}");
        }
        else
        {
            Debug.LogWarning($"Personnage inconnu : {nomScene}");
        }
    }

    /// <summary>
    /// Réinitialise l’état du jeu pour revenir au menu principal.
    /// </summary>
    public void ResetEtat()
    {
        ServiceActuel = "";
        PosteActuel = "";
        FichierPersonnageActuel = "";
        Debug.Log("État du jeu réinitialisé");
    }

    /// <summary>
    /// Définit le scénario et le niveau actuel.
    /// </summary>
    /// <param name="scenario">Numéro du scénario.</param>
    /// <param name="niveau">Numéro du niveau.</param>
    public void DefinirScenarioEtNiveau(int scenario, int niveau)
    {
        ScenarioActuel = scenario;
        NiveauActuel = niveau;
        Debug.Log($"Scénario {scenario} - Niveau {niveau} défini");
    }
    
    /// <summary>
    /// Passe au niveau suivant (max 5).
    /// </summary>
    //TODO : appeler depuis MapUIManager
    public void PasserNiveauSuivant()
    {
        if (NiveauActuel < 5)
        {
            NiveauActuel++;
            Debug.Log($"Passage au niveau {NiveauActuel}");
        }
        else
        {
            Debug.Log("Niveau maximum atteint !");
        }
    }
    
    /// <summary>
    /// Retourne le nom complet du service à partir de son identifiant.
    /// </summary>
    /// <param name="identifiantService">Identifiant du service (ex: "info").</param>
    /// <returns>Nom complet du service.</returns>
    public string ObtenirNomService(string identifiantService)
    {
        switch (identifiantService.ToLower())
        {
            case "restauration": return "Restauration";
            case "info": return "Informatique";
            case "comptabilite": return "Comptabilité";
            case "communication": return "Communication";
            case "technicien": return "Techniciens";
            default: return identifiantService;
        }
    }
}