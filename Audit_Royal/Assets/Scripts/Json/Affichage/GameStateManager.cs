using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire global qui persiste entre toutes les scènes
/// Stocke l'état actuel du jeu (scénario, niveau, service sélectionné, etc.)
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // État du jeu
    public int ScenarioActuel { get; private set; } = 0;
    public int NiveauActuel { get; private set; } = 1;
    public string ServiceActuel { get; set; } = "";
    public string PosteActuel { get; set; } = "";
    public string FichierPersonnageActuel { get; set; } = "";
    
    private Dictionary<string, string> batimentVersService = new Dictionary<string, string>()
    {
        { "Crous", "restauration" },
        { "Informatique", "info" },
        { "Compta", "comptabilite" },
        { "Communication", "communication" },
        { "Techniciens", "technicien" },
    };
    
    private Dictionary<string, string> sceneVersPersonnage = new Dictionary<string, string>()
    {
        // Communication
        { "DirectorCom", "com_responsable_reseaux_sociaux.json" },
        { "SecretaireCom", "com_graphiste.json" },
        { "Technicien", "com_technicien_son_video.json" },
        
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
    /// Appelé quand on entre dans un bâtiment
    /// Détermine le service associé au bâtiment
    /// </summary>
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
    /// Appelé quand on clique sur un personnage
    /// Récupère le fichier JSON associé à ce personnage
    /// </summary>
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
    /// Reset l'état du jeu (retour au menu principal)
    /// </summary>
    public void ResetEtat()
    {
        ServiceActuel = "";
        PosteActuel = "";
        FichierPersonnageActuel = "";
        Debug.Log("État du jeu réinitialisé");
    }

    /// <summary>
    /// Change de scénario et niveau
    /// </summary>
    public void DefinirScenarioEtNiveau(int scenario, int niveau)
    {
        ScenarioActuel = scenario;
        NiveauActuel = niveau;
        Debug.Log($"Scénario {scenario} - Niveau {niveau} défini");
    }
    
    /// <summary>
    /// Passe au niveau suivant (appelé depuis MapUIManager)
    /// </summary>
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
    /// Retourne le nom du service en fonction de son identifiant
    /// </summary>
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