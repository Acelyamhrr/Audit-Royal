using UnityEngine;
using System.Collections.Generic;

/// Gestionnaire global qui persiste entre toutes les scènes
/// Stocke l'état actuel du jeu (scénario, niveau, service sélectionné, etc.)
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // État du jeu
    public int ScenarioActuel { get; set; } = 1;
    public int NiveauActuel { get; set; } = 1;
    public string ServiceActuel { get; set; } = "";
    public string PosteActuel { get; set; } = "";
    public string FichierPersonnageActuel { get; set; } = "";
    
    private Dictionary<string, string> batimentVersService = new Dictionary<string, string>()
    {
        { "Crous", "restauration" },  // Crous
        { "Informatique", "info" },          // Info
        { "Compta", "comptabilite" },           // Compta
        { "Communication", "communication" }, // Com/BTP
        { "Techniciens", "technicien" },        // techniciens
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

    /// Appelé quand on entre dans un bâtiment
    /// Détermine le service associé au bâtiment
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

    /// Appelé quand on clique sur un personnage
    /// Récupère le fichier JSON associé à ce personnage
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

    /// Reset l'état du jeu (retour au menu principal)
    public void ResetEtat()
    {
        ServiceActuel = "";
        PosteActuel = "";
        FichierPersonnageActuel = "";
        Debug.Log("État du jeu réinitialisé");
    }

    /// Change de scénario et niveau
    public void DefinirScenarioEtNiveau(int scenario, int niveau)
    {
        ScenarioActuel = scenario;
        NiveauActuel = niveau;
        Debug.Log($"Scénario {scenario} - Niveau {niveau} défini");
    }
}