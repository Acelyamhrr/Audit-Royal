using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// <summary>
/// Gère la génération automatique des fichiers de vérités d’un scénario,
/// en fonction du niveau de difficulté et des services disponibles.
/// </summary>
/// <remarks>
/// Le <see cref="ScenarioManager"/> analyse les fichiers JSON de dialogues,
/// sélectionne des questions communes selon des règles précises (niveau 1 à 5),
/// puis génère un fichier de vérités utilisé par le système de dialogue.
/// 
/// Cette classe est persistante entre les scènes (singleton Unity).
/// </remarks>
public class ScenarioManager : MonoBehaviour
{
    /// <summary>
    /// Sous-dossier utilisé pour stocker les fichiers générés.
    /// </summary>
    private const string JSON_SUBDIR = "GameData";
    
    /// <summary>
    /// Nom du fichier JSON de sortie contenant les vérités du scénario.
    /// </summary>
    private const string OUTPUT_FILE_NAME = "scenario_verites.json";

    /// <summary>
    /// Liste des services possibles dans le jeu.
    /// </summary>
    private static readonly string[] ServiceFiles = new string[]
    {
        "communication", 
        "comptabilite",
        "info",
        "restauration",
        "technicien"
    };

    /// <summary>
    /// Initialise le singleton du <see cref="ScenarioManager"/>.
    /// Détruit les instances en double et conserve celle-ci entre les scènes.
    /// </summary>
    void Awake()
    {
        ScenarioManager[] instances = FindObjectsByType<ScenarioManager>(FindObjectsSortMode.None);
        if (instances.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Génère le fichier de vérités d’un scénario selon le niveau spécifié.
    /// </summary>
    /// <param name="numScenario">Numéro du scénario.</param>
    /// <param name="niveau">
    /// Niveau de difficulté :
    /// <list type="bullet">
    /// <item><description>Niveau 1 : 1 service audité, 1 question commune</description></item>
    /// <item><description>Niveau 2 : 1 service audité, 1/3 des questions</description></item>
    /// <item><description>Niveau 3 : 1 question service audité + 1 autre service</description></item>
    /// <item><description>Niveau 4 : Tous les services, 1/3 des questions</description></item>
    /// <item><description>Niveau 5 : Tous les services, toutes les questions</description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// Le fichier généré est stocké dans le dossier persistant de l’application
    /// et sera utilisé lors des dialogues en jeu.
    /// </remarks>
    public void GenerateVeritesFile(int numScenario, int niveau)
    {
        Debug.Log($"Début de la génération - Scénario {numScenario} - Niveau {niveau}");
        
        if (niveau < 1 || niveau > 5)
        {
            Debug.LogError($"Niveau invalide: {niveau}. Doit être entre 1 et 5.");
            return;
        }

        Dictionary<string, VeritesByService> finalVerites = new Dictionary<string, VeritesByService>();
        System.Random random = new System.Random();

        // Charger le service audité
        string serviceAudite = ChargerServiceAudite(numScenario);
        Debug.Log($"Service audité: {serviceAudite}");

        // Détermine quels services utiliser selon le niveau
        List<string> servicesAUtiliser = ObtenirServicesParNiveau(niveau, serviceAudite);
        
        Debug.Log($"Services sélectionnés pour niveau {niveau}: {string.Join(", ", servicesAUtiliser)}");

        // Collecter toutes les questions disponibles pour chaque service
        Dictionary<string, List<string>> questionsParService = new Dictionary<string, List<string>>();
        
        foreach (string service in servicesAUtiliser)
        {
            string nomFichier = $"scenario{numScenario}_{service}.json";
            string cheminComplet = Path.Combine(Application.streamingAssetsPath, nomFichier);
            
            if (!File.Exists(cheminComplet))
                continue;
                
            try
            {
                string jsonContent = File.ReadAllText(cheminComplet);
                ServiceData serviceData = JsonConvert.DeserializeObject<ServiceData>(jsonContent);
                
                if (serviceData?.postes != null && serviceData.postes.Count > 0)
                {
                    var premierPoste = serviceData.postes.First().Value;
                    questionsParService[service] = premierPoste.Keys.ToList();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors de la lecture de {nomFichier}: {e.Message}");
            }
        }
        
        // Sélectionner les questions communes selon le niveau
        List<string> questionsCommunesSelectionnees = SelectionnerQuestionsCommunesParNiveau(
            niveau, 
            questionsParService, 
            serviceAudite, 
            random
        );
        
        Debug.Log($"Questions sélectionnées pour tous: [{string.Join(", ", questionsCommunesSelectionnees)}]");

        // Générer les vérités pour chaque service avec ces questions
        foreach (string service in servicesAUtiliser)
        {
            string nomFichier = $"scenario{numScenario}_{service}.json";
            string cheminComplet = Path.Combine(Application.streamingAssetsPath, nomFichier);            

            if (!File.Exists(cheminComplet))
            {
                Debug.LogError($"Service {service} non trouvé : {nomFichier}");
                continue;
            }

            string jsonContent = File.ReadAllText(cheminComplet);
            ServiceData serviceData;

            try
            {
                serviceData = JsonConvert.DeserializeObject<ServiceData>(jsonContent);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur : {nomFichier}: {e.Message}");
                continue;
            }

            if (serviceData?.postes == null)
            {
                Debug.LogWarning($"Aucun poste trouvé dans {nomFichier}");
                continue;
            }
            
            string serviceName = serviceData.service;

            VeritesByService currentServiceVerites = new VeritesByService
            {
                postes = new Dictionary<string, VeritesByPoste>()
            };

            foreach (KeyValuePair<string, Dictionary<string, List<DialogueVariation>>> posteEntry in serviceData.postes)
            {
                string posteName = posteEntry.Key;
                Dictionary<string, List<DialogueVariation>> posteDialogues = posteEntry.Value; 
                
                VeritesByPoste currentPosteVerites = new VeritesByPoste
                {
                    verites = new Dictionary<string, List<int>>()
                };

                foreach (string questionId in questionsCommunesSelectionnees)
                {
                    if (!posteDialogues.ContainsKey(questionId))
                    {
                        Debug.LogWarning($"Question {questionId} n'existe pas pour {serviceName}/{posteName}");
                        continue;
                    }

                    List<DialogueVariation> variations = posteDialogues[questionId];

                    if (variations == null || variations.Count == 0)
                    {
                        Debug.LogWarning($"Aucune variation pour {serviceName}/{posteName}/Q{questionId}");
                        continue;
                    }
    
                    int nbVerites = random.Next(1, variations.Count + 1); 
                    
                    List<int> variationsVraies = variations
                        .OrderBy(x => random.Next())
                        .Take(nbVerites)
                        .Select(v => v.variation_id)
                        .ToList();
                    
                    currentPosteVerites.verites.Add(questionId, variationsVraies);
                    
                    Debug.Log($"  → {serviceName}/{posteName}/Q{questionId}: {nbVerites}/{variations.Count} vérités = [{string.Join(", ", variationsVraies)}]");
                }
                
                if (currentPosteVerites.verites.Count > 0)
                {
                    currentServiceVerites.postes.Add(posteName, currentPosteVerites);
                }
            }
            
            if (currentServiceVerites.postes.Count > 0)
            {
                finalVerites.Add(serviceName, currentServiceVerites);
                Debug.Log($"✓ Service '{serviceName}' traité avec succès");
            }
        }

        if (finalVerites.Count == 0)
        {
            Debug.LogError($"Erreur !!!! Aucun service trouvé !!");
            return;
        }
        
        VeritesScenarioRoot scenarioVerites = new VeritesScenarioRoot 
        { 
            scenario = numScenario,
            niveau = niveau,
            verites = finalVerites 
        };
        
        string outputJson = JsonConvert.SerializeObject(scenarioVerites, Formatting.Indented); 

        string outputDirPath = Path.Combine(Application.persistentDataPath, JSON_SUBDIR);
        
        if (!Directory.Exists(outputDirPath))
        {
            Directory.CreateDirectory(outputDirPath);
        }

        string scenarioPath = Path.Combine(outputDirPath, OUTPUT_FILE_NAME);
        File.WriteAllText(scenarioPath, outputJson);

        Debug.Log($"======== FICHIER COMPLÉTÉ AVEC SUCCÈS ========");
        Debug.Log($"Chemin: {scenarioPath}");
        Debug.Log($"Niveau: {niveau}");
        Debug.Log($"Services traités: {finalVerites.Count}");
    }

    /// <summary>
    /// Sélectionne les questions communes à utiliser selon le niveau.
    /// </summary>
    /// <param name="niveau">Niveau de difficulté.</param>
    /// <param name="questionsParService">
    /// Dictionnaire associant chaque service à ses questions disponibles.
    /// </param>
    /// <param name="serviceAudite">Nom du service audité.</param>
    /// <param name="random">Générateur aléatoire.</param>
    /// <returns>
    /// Liste des identifiants de questions sélectionnées.
    /// </returns>
    private List<string> SelectionnerQuestionsCommunesParNiveau(
        int niveau, 
        Dictionary<string, List<string>> questionsParService,
        string serviceAudite,
        System.Random random)
    {
        List<string> questionsSelectionnees = new List<string>();
        
        if (questionsParService.Count == 0)
            return questionsSelectionnees;

        switch (niveau)
        {
            case 1:
                if (questionsParService.ContainsKey(serviceAudite))
                {
                    var questionsAuditees = questionsParService[serviceAudite];
                    if (questionsAuditees.Count > 0)
                    {
                        questionsSelectionnees.Add(questionsAuditees[random.Next(questionsAuditees.Count)]);
                    }
                }
                break;
                
            case 2:
                if (questionsParService.ContainsKey(serviceAudite))
                {
                    var questionsAuditees = questionsParService[serviceAudite];
                    int nbQuestions = Mathf.Max(1, questionsAuditees.Count / 3);
                    questionsSelectionnees = questionsAuditees
                        .OrderBy(x => random.Next())
                        .Take(nbQuestions)
                        .ToList();
                }
                break;
                
            case 3:
                if (questionsParService.ContainsKey(serviceAudite))
                {
                    var questionsAuditees = questionsParService[serviceAudite];
                    if (questionsAuditees.Count > 0)
                    {
                        string questionAuditee = questionsAuditees[random.Next(questionsAuditees.Count)];
                        questionsSelectionnees.Add(questionAuditee);
                        
                        foreach (var entry in questionsParService)
                        {
                            if (entry.Key != serviceAudite && entry.Value.Count > 0)
                            {
                                string questionAutresServices = entry.Value[random.Next(entry.Value.Count)];
                                questionsSelectionnees.Add(questionAutresServices);
                                break;
                            }
                        }
                    }
                }
                break;
                
            case 4:
                var premiereListeQuestions = questionsParService.Values.First();
                int nbQuestionsNiveau4 = Mathf.Max(1, premiereListeQuestions.Count / 3);
                questionsSelectionnees = premiereListeQuestions
                    .OrderBy(x => random.Next())
                    .Take(nbQuestionsNiveau4)
                    .ToList();
                break;
                
            case 5:
                questionsSelectionnees = questionsParService.Values.First().ToList();
                break;
        }

        return questionsSelectionnees;
    }

    /// <summary>
    /// Charge le service audité à partir du fichier de configuration du scénario.
    /// </summary>
    /// <param name="numScenario">Numéro du scénario.</param>
    /// <returns>
    /// Nom du service audité.
    /// </returns>
    /// <remarks>
    /// Si le fichier est manquant ou invalide, un service par défaut est retourné.
    /// </remarks>
    private string ChargerServiceAudite(int numScenario)
    {
        string nomFichier = $"scenario{numScenario}.json";
        string cheminComplet = Path.Combine(Application.streamingAssetsPath, nomFichier);

        if (!File.Exists(cheminComplet))
        {
            Debug.LogWarning($"Fichier de configuration du scénario non trouvé: {nomFichier}");
            return ServiceFiles[0];
        }

        try
        {
            string jsonContent = File.ReadAllText(cheminComplet);
            var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
            
            if (config.ContainsKey("service_audite"))
            {
                return config["service_audite"].ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la lecture du service audité: {e.Message}");
        }

        return ServiceFiles[0];
    }

    /// <summary>
    /// Détermine la liste des services à utiliser selon le niveau.
    /// </summary>
    /// <param name="niveau">Niveau de difficulté.</param>
    /// <param name="serviceAudite">Service audité.</param>
    /// <returns>
    /// Liste des services à traiter.
    /// </returns>
    private List<string> ObtenirServicesParNiveau(int niveau, string serviceAudite)
    {
        List<string> services = new List<string>();

        switch (niveau)
        {
            case 1:
            case 2:
                services.Add(serviceAudite);
                Debug.Log($"Niveaux 1-2 : Service audité sélectionné = {serviceAudite}");
                break;
                
            case 3:
            case 4:
            case 5:
                services.AddRange(ServiceFiles);
                break;
        }

        return services;
    }
    
    /// <summary>
    /// Vérifie si au moins un service est disponible pour un scénario donné.
    /// </summary>
    /// <param name="numeroScenario">Numéro du scénario.</param>
    /// <returns>
    /// True si au moins un service est trouvé, false sinon.
    /// </returns>
    public bool VerifierScenarioComplet(int numeroScenario)
    {
        int servicesPresents = 0;
        
        foreach (string service in ServiceFiles)
        {
            string nomFichier = $"scenario{numeroScenario}_{service}.json";
            string cheminComplet = Path.Combine(Application.streamingAssetsPath, nomFichier);
            
            if (File.Exists(cheminComplet))
            {
                servicesPresents++;
            }
        }
        
        Debug.Log($"Scénario {numeroScenario}: {servicesPresents}/{ServiceFiles.Length} services trouvés");
        return servicesPresents > 0;
    }

    /// <summary>
    /// Retourne la liste des services disponibles pour un scénario.
    /// </summary>
    /// <param name="numeroScenario">Numéro du scénario.</param>
    /// <returns>
    /// Liste des services disponibles.
    /// </returns>
    public List<string> ObtenirServicesDisponibles(int numeroScenario)
    {
        List<string> servicesDisponibles = new List<string>();
        
        foreach (string service in ServiceFiles)
        {
            string nomFichier = $"scenario{numeroScenario}_{service}.json";
            string cheminComplet = Path.Combine(Application.streamingAssetsPath, nomFichier);
            
            if (File.Exists(cheminComplet))
            {
                servicesDisponibles.Add(service);
            }
        }
        
        return servicesDisponibles;
    }
}