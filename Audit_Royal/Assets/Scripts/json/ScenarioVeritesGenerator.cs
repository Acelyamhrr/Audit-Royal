namespace json
{ 
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json; 

    public class ScenarioInitializer : MonoBehaviour
    {
        private const string JSON_SUBDIR = "GameData";
        private const string OUTPUT_FILE_NAME = "scenario_verites.json";

        private static readonly string[] ServiceFiles = new string[]
        {

            "scenario1_communication", 
            "scenario1_compta",
            "scenario1_info",
            "scenario1_restauration",
            "scenario1_techniciens"
        };
        
        // Référence au dossier Resources
        private const string RESOURCES_PATH = "JSON/"; 

        void Awake()
        {
            if (FindObjectsOfType<ScenarioInitializer>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            GenerateVeritesFile();
        }

        public void GenerateVeritesFile()
        {
            Debug.Log("Début de la génération au démarrage du jeu.");
            
            var finalVerites = new Dictionary<string, VeritesByService>();
            var random = new System.Random();

            foreach (var serviceFileName in ServiceFiles)
            {

                TextAsset jsonTextAsset = Resources.Load<TextAsset>(RESOURCES_PATH + serviceFileName);

                if (jsonTextAsset == null)
                {
                    Debug.LogError($"Fichier source non trouvé dans Resources/{RESOURCES_PATH}{serviceFileName}.json");
                    continue;
                }
                
                string jsonContent = jsonTextAsset.text;
                ServiceData serviceData;
                
                try
                {
                    serviceData = JsonConvert.DeserializeObject<ServiceData>(jsonContent);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Erreur de désérialisation pour {serviceFileName}: {e.Message}");
                    continue;
                }

                if (serviceData?.postes == null) continue;
                
                string serviceName = serviceData.service;
                var currentServiceVerites = new VeritesByService { postes = new Dictionary<string, VeritesByPoste>() };

                foreach (KeyValuePair<string, Dictionary<string, List<DialogueVariation>>> posteEntry in serviceData.postes)
                {
                    string currentPosteName = posteEntry.Key;
                    var posteDialogues = posteEntry.Value; 
                    var currentPosteVerites = new VeritesByPoste { verites = new Dictionary<string, List<int>>() };

                    foreach (KeyValuePair<string, List<DialogueVariation>> questionEntry in posteDialogues)
                    {
                        string questionId = questionEntry.Key; 
                        var variations = questionEntry.Value; 

                        if (variations == null || variations.Count == 0) continue;

                        int veritesCount = random.Next(1, variations.Count + 1); 

                        var trueVariations = variations.OrderBy(x => random.Next()) 
                                                       .Take(veritesCount) 
                                                       .Select(v => v.variation_id)
                                                       .ToList();
                        
                        currentPosteVerites.verites.Add(questionId, trueVariations);
                    }

                    currentServiceVerites.postes.Add(currentPosteName, currentPosteVerites);
                }

                finalVerites.Add(serviceName, currentServiceVerites);
            }
            
            VeritesScenarioRoot scenarioVerites = new VeritesScenarioRoot { /* ... */ };
            string outputJson = JsonConvert.SerializeObject(scenarioVerites, Formatting.Indented); 

            string outputDirPath = Path.Combine(Application.persistentDataPath, JSON_SUBDIR);
            string scenarioPath = Path.Combine(outputDirPath, OUTPUT_FILE_NAME);

            if (!Directory.Exists(outputDirPath))
            {
                Directory.CreateDirectory(outputDirPath);
            }
            
            File.WriteAllText(scenarioPath, outputJson);
            Debug.Log($"Fichier des vérités généré et enregistré dans : {scenarioPath}");
        }
    }
}