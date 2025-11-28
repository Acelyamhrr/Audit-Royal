using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class JsonDialogueManager : MonoBehaviour
{
    private VeritesScenarioRoot veritesData;

    private const string DOSSIER_PERSONNAGES = "personnes_json";
    
    private static readonly Dictionary<string, float> TAUX_VERITE_PAR_CARACTERE = new Dictionary<string, float>
    {
        { "menteur", 0.0f },      // Ment toujours
        { "anxieux", 0.5f },     // 50% de vérité
        { "colere", 0.75f },      // 75% de vérité
        { "balance", 1.0f },      // Dit toujours la vérité
        { "normal", 0.8f }        // Par défaut
    };
        
    // Stocke tous les dialogues organisés par métier puis par numéro de question
    private Dictionary<string, Dictionary<string, List<DialogueVariation>>> dialogues;
    
    // Données du personnage actuellement interrogé
    private PlayerData personnageActuel;

    // Chemin du fichier JSON du personnage actuel
    private string cheminPersonnageActuel;

    // Charge les dialogues d'un service pour un scénario donné
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

    // charge les vérités de scenario_verites
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

    // verifie si une variation est vraie
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


    // Charge les données d'un personnage depuis son fichier JSON
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
    
    // Augmente le taux d'énervement du personnage de 10% à chaque question posée
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

    // NOUVELLE FONCTION : Retourne le dialogue ET l'émotion utilisée
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

        // MODIFICATION : Utilise la nouvelle fonction qui retourne texte ET émotion
        (string texte, string emotion) = SelectionnerTexteDialogueAvecEmotion(variationChoisie, ditLaVerite);
        
        AugmenterTauxEnervement();
        
        return (texte, emotion);
    }

    // pour obtenir le dialogue approprié pour une question donnée (ancienne version, gardée pour compatibilité)
    public string ObtenirDialogue(int numeroScenario, string nomFichierPerso, string numeroQuestion)
    {
        (string texte, string _) = ObtenirDialogueAvecEmotion(numeroScenario, nomFichierPerso, numeroQuestion);
        return texte;
    }

    // Choisit aléatoirement une variation parmi toutes les variations disponibles
    private DialogueVariation ChoisirVariationAleatoire(List<DialogueVariation> variations)
    {
        int indexAleatoire = Random.Range(0, variations.Count);
        return variations[indexAleatoire];
    }

    // Détermine si le personnage va dire la vérité selon son caractère
    private bool TireAuSortVerite(string caractere)
    {
        // Récupère le taux de vérité du caractère, ou utilise "normal" par défaut
        float tauxVerite = TAUX_VERITE_PAR_CARACTERE.ContainsKey(caractere.ToLower()) ? TAUX_VERITE_PAR_CARACTERE[caractere.ToLower()] : TAUX_VERITE_PAR_CARACTERE["normal"];
        
        float tirage = Random.Range(0f, 1f);
        bool ditVerite = tirage < tauxVerite;
        
        Debug.Log($"Caractère: {caractere}, Taux vérité: {tauxVerite*100}%, Tirage: {tirage*100}%, Résultat: {(ditVerite ? "VÉRITÉ" : "MENSONGE")}");
        
        return ditVerite;
    }

    // NOUVELLE FONCTION : Sélectionne le texte ET retourne l'émotion utilisée
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

    // Permet d'obtenir toutes les info d'un personnage
    public PlayerData ObtenirInfosPersonnage(string nomFichierPerso)
    {
        ChargerPersonnage(nomFichierPerso);
        return personnageActuel;
    }

    // Retourne le nombre total de questions disponibles pour un personnage
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