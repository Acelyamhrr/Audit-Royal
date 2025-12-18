using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// Gère le chargement et la sélection des dialogues depuis des fichiers JSON
/// en fonction du scénario, du personnage interrogé et de son comportement.
/// </summary>
/// <remarks>
/// Cette classe permet de déterminer si un personnage dit la vérité ou ment,
/// de choisir une variation de dialogue appropriée et de mettre à jour
/// le carnet ainsi que l’état émotionnel du personnage.
/// </remarks>
public class JsonDialogueManager : MonoBehaviour
{
    /// <summary>
    /// Données des vérités associées au scénario courant.
    /// </summary>
    private VeritesScenarioRoot veritesData;
    
    /// <summary>
    /// Nom du dossier contenant les fichiers JSON des personnages.
    /// </summary>
    private const string DOSSIER_PERSONNAGES = "personnes_json";
    
    /// <summary>
    /// Taux de vérité associé à chaque caractère de personnage.
    /// </summary>
    private static readonly Dictionary<string, float> TAUX_VERITE_PAR_CARACTERE = new Dictionary<string, float>
    {
        { "menteur", 0.0f },      // Ment toujours
        { "anxieux", 0.5f },     // 50% de vérité
        { "colere", 0.75f },      // 75% de vérité
        { "balance", 1.0f },      // Dit toujours la vérité
        { "normal", 0.8f }        // Par défaut
    };
        
    /// <summary>
    /// Dialogues organisés par métier puis par numéro de question.
    /// </summary>
    private Dictionary<string, Dictionary<string, List<DialogueVariation>>> dialogues;
    
    /// <summary>
    /// Données du personnage actuellement interrogé.
    /// </summary>
    private PlayerData personnageActuel;

    /// <summary>
    /// Chemin du fichier JSON du personnage actuellement chargé.
    /// </summary>
    private string cheminPersonnageActuel;


    /// <summary>
    /// Charge les dialogues d’un service pour un scénario donné.
    /// </summary>
    /// <param name="numeroScenario">Numéro du scénario.</param>
    /// <param name="service">Nom du service concerné.</param>
    private void ChargerDialoguesService(int numeroScenario, string service)
    {
        // Construction du nom de fichier : scenario1_comptabilite.json
        string nomFichier = $"scenario{numeroScenario}_{service}.json";
        string filePath = Path.Combine(Application.streamingAssetsPath, nomFichier);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fichier de dialogues introuvable : {filePath}");
            return;
        }

        // Lecture et désérialisation du fichier JSON
        string jsonContent = File.ReadAllText(filePath);
        ServiceData serviceData = JsonConvert.DeserializeObject<ServiceData>(jsonContent);
        dialogues = serviceData.postes;
        
        Debug.Log($"Dialogues du service '{service}' chargés avec succès");
    }

    /// <summary>
    /// Charge le fichier JSON contenant les vérités du scénario.
    /// </summary>
    private void ChargerVerites()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "GameData", "scenario_verites.json");
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Fichier vérités introuvable : {filePath}");
            return;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        veritesData = JsonConvert.DeserializeObject<VeritesScenarioRoot>(jsonContent);
        
        Debug.Log("Fichier des vérités chargé avec succès");
    }

    /// <summary>
    /// Vérifie si une variation de dialogue correspond à une vérité.
    /// </summary>
    /// <param name="service">Service concerné.</param>
    /// <param name="metier">Métier concerné.</param>
    /// <param name="numeroQuestion">Numéro de la question.</param>
    /// <param name="variationId">Identifiant de la variation.</param>
    /// <returns>
    /// True si la variation est une vérité, false sinon.
    /// </returns>
    private bool EstUneVerite(string service, string metier, string numeroQuestion, int variationId)
    {
        if (veritesData == null || veritesData.verites == null)
        {
            Debug.LogWarning("Vérités non chargées !");
            return false;
        }
        
        // verites[service].postes[metier].verites[numeroQuestion]
        if (!veritesData.verites.ContainsKey(service)) return false;
        
        VeritesByService serviceVerites = veritesData.verites[service];
        if (!serviceVerites.postes.ContainsKey(metier)) return false;
        
        VeritesByPoste posteVerites = serviceVerites.postes[metier];
        if (!posteVerites.verites.ContainsKey(numeroQuestion)) return false;
        
        List<int> veritesIds = posteVerites.verites[numeroQuestion];
        return veritesIds.Contains(variationId);
    }


    /// <summary>
    /// Charge les données d’un personnage depuis son fichier JSON.
    /// </summary>
    /// <param name="nomFichier">Nom du fichier JSON du personnage.</param>
    private void ChargerPersonnage(string nomFichier)
    {
        // Vérifie d'abord si le personnage a une sauvegarde modifiée (pr le taux par ex)
        cheminPersonnageActuel = Path.Combine(Application.persistentDataPath, nomFichier);
        
        if (!File.Exists(cheminPersonnageActuel))
        {
            // Sinon, charge le fichier initial du jeu
            cheminPersonnageActuel = Path.Combine(Application.streamingAssetsPath, DOSSIER_PERSONNAGES, nomFichier);
        }
        
        if (!File.Exists(cheminPersonnageActuel))
        {
            Debug.LogError($"Fichier personnage introuvable : {cheminPersonnageActuel}");
            return;
        }

        string jsonContent = File.ReadAllText(cheminPersonnageActuel);
        personnageActuel = JsonUtility.FromJson<PlayerData>(jsonContent);
        
        Debug.Log($"Personnage chargé : {personnageActuel.prenom} {personnageActuel.nom} {personnageActuel.metier} - Taux énervement: {personnageActuel.taux}%");
    }
    
    /// <summary>
    /// Augmente le taux d’énervement du personnage à chaque question et sauvegarde la modification.
    /// </summary>
    private void AugmenterTauxEnervement()
    {
        personnageActuel.taux += 10;
        
        if (personnageActuel.taux > 100) {
            personnageActuel.taux = 100;
        }
        
        string json = JsonUtility.ToJson(personnageActuel, true);
        File.WriteAllText(cheminPersonnageActuel, json);
        
        Debug.Log($"{personnageActuel.prenom} {personnageActuel.nom} - Nouveau taux d'énervement : {personnageActuel.taux}%");
    }

    /// <summary>
    /// Retourne le dialogue sélectionné ainsi que l’émotion associée.
    /// </summary>
    /// <param name="numeroScenario">Numéro du scénario.</param>
    /// <param name="nomFichierPerso">Nom du fichier du personnage.</param>
    /// <param name="numeroQuestion">Numéro de la question.</param>
    /// <returns>
    /// Un tuple contenant le texte du dialogue et l’émotion utilisée.
    /// </returns>
    public (string texte, string emotion) ObtenirDialogueAvecEmotion(int numeroScenario, string nomFichierPerso, string numeroQuestion)
    {
        ChargerPersonnage(nomFichierPerso);
        ChargerVerites();
        
        string service = personnageActuel.service;
        string metier = personnageActuel.metier;
        
        ChargerDialoguesService(numeroScenario, service);
        
        if (!dialogues.ContainsKey(metier))
        {
            Debug.LogError($"Métier '{metier}' introuvable dans le service '{service}'");
            return ("Erreur : Métier introuvable", "normal");
        }
        
        Dictionary<string, List<DialogueVariation>> metierDialogues = dialogues[metier];
        
        if (!metierDialogues.ContainsKey(numeroQuestion))
        {
            Debug.LogError($"Question '{numeroQuestion}' introuvable pour {metier}");
            return ("Erreur : Question introuvable", "normal");
        }
        
        List<DialogueVariation> variations = metierDialogues[numeroQuestion];
        
        // détermine si le perso dit la verite selon son caractere
        bool ditLaVerite = TireAuSortVerite(personnageActuel.caractere);
        
        // Choisit une variation selon vérité/mensonge
        DialogueVariation variationChoisie = ChoisirVariationSelonVerite(
            service, 
            metier, 
            numeroQuestion, 
            variations, 
            ditLaVerite
        );

        // Met à jour le carnet en fonction de la réponse choisie par le personnage
        //carnetManager.ajoutInfo(service, metier, numeroQuestion, variationChoisie.variation_id);
        CarnetManager.Instance.ajoutInfo(
            service,
            metier,
            numeroQuestion,
            variationChoisie.variation_id
        );

        // MODIFICATION : Utilise la nouvelle fonction qui retourne texte ET émotion
        (string texte, string emotion) = SelectionnerTexteDialogueAvecEmotion(variationChoisie, ditLaVerite);
        
        AugmenterTauxEnervement();
        
        return (texte, emotion);
    }

    /// <summary>
    /// Retourne uniquement le texte du dialogue (compatibilité ancienne version).
    /// </summary>
    /// <param name="numeroScenario">Numéro du scénario.</param>
    /// <param name="nomFichierPerso">Nom du fichier du personnage.</param>
    /// <param name="numeroQuestion">Numéro de la question.</param>
    /// <returns>
    /// Texte du dialogue sélectionné.
    /// </returns>
    public string ObtenirDialogue(int numeroScenario, string nomFichierPerso, string numeroQuestion)
    {
        (string texte, string _) = ObtenirDialogueAvecEmotion(numeroScenario, nomFichierPerso, numeroQuestion);
        return texte;
    }

    /// <summary>
    /// Choisit aléatoirement une variation de dialogue.
    /// </summary>
    /// <param name="variations">Liste des variations disponibles.</param>
    /// <returns>
    /// Variation sélectionnée.
    /// </returns>
    private DialogueVariation ChoisirVariationAleatoire(List<DialogueVariation> variations)
    {
        int indexAleatoire = Random.Range(0, variations.Count);
        return variations[indexAleatoire];
    }

    /// <summary>
    /// Détermine si le personnage dit la vérité selon son caractère.
    /// </summary>
    /// <param name="caractere">Caractère du personnage.</param>
    /// <returns>
    /// True si le personnage dit la vérité, false sinon.
    /// </returns>
    private bool TireAuSortVerite(string caractere)
    {
        // Récupère le taux de vérité du caractère, ou utilise "normal" par défaut
        float tauxVerite = TAUX_VERITE_PAR_CARACTERE.ContainsKey(caractere.ToLower()) ? TAUX_VERITE_PAR_CARACTERE[caractere.ToLower()] : TAUX_VERITE_PAR_CARACTERE["normal"];
        
        float tirage = Random.Range(0f, 1f);
        bool ditVerite = tirage < tauxVerite;
        
        Debug.Log($"Caractère: {caractere}, Taux vérité: {tauxVerite*100}%, Tirage: {tirage*100}%, Résultat: {(ditVerite ? "VÉRITÉ" : "MENSONGE")}");
        
        return ditVerite;
    }

    /// <summary>
    /// Sélectionne le texte du dialogue et retourne l’émotion associée.
    /// </summary>
    /// <param name="variation">Variation de dialogue sélectionnée.</param>
    /// <param name="ditLaVerite">Indique si le personnage dit la vérité.</param>
    /// <returns>
    /// Tuple contenant le texte et l’émotion.
    /// </returns>
    private (string texte, string emotion) SelectionnerTexteDialogueAvecEmotion(DialogueVariation variation, bool ditLaVerite)
    {
        // Si le personnage ment, renvoie systématiquement le texte "menteur"
        if (!ditLaVerite)
        {
            return (variation.menteur, "menteur");
        }
        
        double tauxEnervement = personnageActuel.taux;
        
        // Si le personnage est calme (< 50%), utilise le texte normal
        if (tauxEnervement < 50)
        {
            return (variation.normal, "normal");
        }
        
        // Si taux>= 50%, sélectionne le texte selon le caractère
        switch (personnageActuel.caractere.ToLower())
        {
            case "colere":
                return (variation.colere, "colere");
                
            case "anxieux":
                return (variation.anxieux, "anxieux");
                
            case "balance":
                return (variation.balance, "balance");
                
            case "insouciant":
            case "normal":
            default:
                return (variation.normal, "normal");
        }
    }

    /// <summary>
    /// Choisit une variation de dialogue en fonction de la vérité ou du mensonge.
    /// </summary>
    private DialogueVariation ChoisirVariationSelonVerite( string service, string metier, string numeroQuestion, List<DialogueVariation> variations, bool ditLaVerite )
    {
        // Sépare les variations vraies et fausses
        List<DialogueVariation> variationsVraies = new List<DialogueVariation>();
        List<DialogueVariation> variationsFausses = new List<DialogueVariation>();
        
        foreach (var variation in variations)
        {
            if (EstUneVerite(service, metier, numeroQuestion, variation.variation_id))
            {
                variationsVraies.Add(variation);
            }
            else
            {
                variationsFausses.Add(variation);
            }
        }
        
        // Si le perso dit la verite, prend parmi les variations vraies
        if (ditLaVerite && variationsVraies.Count > 0)
        {
            int index = Random.Range(0, variationsVraies.Count);
            Debug.Log($"Vérité : variation {variationsVraies[index].variation_id}");
            return variationsVraies[index];
        }
        // Si le perso ment, prend parmi les variations fausses
        else if (!ditLaVerite && variationsFausses.Count > 0)
        {
            int index = Random.Range(0, variationsFausses.Count);
            Debug.Log($"Mensonge : variation {variationsFausses[index].variation_id}");
            return variationsFausses[index];
        }
        
        Debug.LogWarning($"Pas de variation appropriée pour {metier} Q{numeroQuestion} (vérité={ditLaVerite})");
        return variations[Random.Range(0, variations.Count)];
    }

    /// <summary>
    /// Retourne les informations complètes d’un personnage.
    /// </summary>
    /// <param name="nomFichierPerso">Nom du fichier du personnage.</param>
    /// <returns>
    /// Données du personnage.
    /// </returns>
    public PlayerData ObtenirInfosPersonnage(string nomFichierPerso)
    {
        ChargerPersonnage(nomFichierPerso);
        return personnageActuel;
    }

    /// <summary>
    /// Retourne le nombre total de questions disponibles pour un personnage.
    /// </summary>
    /// <param name="numeroScenario">Numéro du scénario.</param>
    /// <param name="nomFichierPerso">Nom du fichier du personnage.</param>
    /// <returns>
    /// Nombre de questions disponibles.
    /// </returns>
    public int ObtenirNombreQuestions(int numeroScenario, string nomFichierPerso)
    {
        ChargerPersonnage(nomFichierPerso);
        
        string service = personnageActuel.service;
        string metier = personnageActuel.metier;
        
        ChargerDialoguesService(numeroScenario, service);
        
        if (!dialogues.ContainsKey(metier))
        {
            return 0;
        }
        
        return dialogues[metier].Count;
    }
}