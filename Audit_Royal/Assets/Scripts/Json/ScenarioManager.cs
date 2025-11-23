using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json; 

public class ScenarioManager : MonoBehaviour
{
    private const string JSON_SUBDIR = "GameData"; // save le fichier de sortie dans T3\GameData
    private const string OUTPUT_FILE_NAME = "scenario_verites.json";

    private static readonly string[] ServiceFiles = new string[]
    {
        "communication", 
        "comptabilite",
        "info",
        "restauration",
        "technicien"
    };

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
    
    public void GenerateVeritesFile(int numScenario)
    {
        Debug.Log($"Début de la génération {numScenario} ");
        
        Dictionary<string, VeritesByService> finalVerites = new Dictionary<string, VeritesByService>();
        System.Random random = new System.Random();

        foreach (string service in ServiceFiles)
        {
            string nomFichier = $"scenario{numScenario}_{service}.json";
            string cheminComplet = Path.Combine(Application.streamingAssetsPath, nomFichier);            

            if (!File.Exists(cheminComplet))
            {
                Debug.LogError($"Service {service} non trouvé pour le scenario {numScenario} : {nomFichier}");
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

            // parcourt chaque poste du service 
            foreach (KeyValuePair<string, Dictionary<string, List<DialogueVariation>>> posteEntry in serviceData.postes)
            {
                string posteName = posteEntry.Key;
                Dictionary<string, List<DialogueVariation>> posteDialogues = posteEntry.Value; 
                
                VeritesByPoste currentPosteVerites = new VeritesByPoste
                {
                    verites = new Dictionary<string, List<int>>()
                };

                // parcourt chaque question du poste
                foreach (KeyValuePair<string, List<DialogueVariation>> questionEntry in posteDialogues)
                {
                    string questionId = questionEntry.Key; 

                    List<DialogueVariation> variations = questionEntry.Value;

                    if (variations == null || variations.Count == 0)
                    {
                        Debug.LogWarning($"Aucune variation pour {serviceName}/{posteName}/Q{questionId}");
                        continue;
                    }
    
                    // selectionne aléatoirement 1 à N variations comme "vraies"
                    int nbVerites = random.Next(1, variations.Count + 1); 
                    
                    List<int> variationsVraies = variations.OrderBy(x => random.Next()).Take(nbVerites).Select(v => v.variation_id).ToList();
                    
                    currentPosteVerites.verites.Add(questionId, variationsVraies);
                    
                    Debug.Log($"  → {serviceName}/{posteName}/Q{questionId}: {nbVerites}/{variations.Count} vérités = [{string.Join(", ", variationsVraies)}]");
                    
                }
                currentServiceVerites.postes.Add(posteName, currentPosteVerites);
            }
            finalVerites.Add(serviceName, currentServiceVerites);
            Debug.Log($"✓ Service '{serviceName}' traité avec succès");
        }

        if (finalVerites.Count == 0)
        {
            Debug.LogError($"Erreur !!!! Aucun service trouvé !!");
            return;
        }
        
        VeritesScenarioRoot scenarioVerites = new VeritesScenarioRoot 
        { 
            scenario = numScenario, 
            verites = finalVerites 
        };
        
        // serialise le json
        string outputJson = JsonConvert.SerializeObject(scenarioVerites, Formatting.Indented); 

        // créer le dossier de sortie
        string outputDirPath = Path.Combine(Application.persistentDataPath, JSON_SUBDIR);
        
        if (!Directory.Exists(outputDirPath))
        {
            Directory.CreateDirectory(outputDirPath);
        }

        // écrit le fichier
        string scenarioPath = Path.Combine(outputDirPath, OUTPUT_FILE_NAME);
        File.WriteAllText(scenarioPath,  outputJson);

        Debug.Log($"======== FICHIER COMPLÉTÉ AVEC SUCCES ======");
        Debug.Log($"Chemin: {scenarioPath}");
        Debug.Log($"Services traités: {finalVerites.Count}");

    }
    
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

    // Retourne la liste des services disponibles pour un scénario
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