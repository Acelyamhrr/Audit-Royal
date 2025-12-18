using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Gère le carnet de l’auditeur : stockage, ajout et lecture des informations obtenues
/// depuis les différents services et métiers.
/// </summary>
public class CarnetManager : MonoBehaviour
{
    public static CarnetManager Instance { get; private set; }
    public static bool visible { get; set; }
    public GameObject scrollViewGameObject;

    /// <summary>
    /// Chemin du fichier JSON du carnet.
    /// </summary>
    private string pathFile;
    
    /// <summary>
    /// Numéro du scénario en cours
    /// </summary>
    private string numScenario;

    
    
    /// <summary>
    /// Initialisation du carnet à partir du fichier de vérités.
    /// Transforme "verites" en "informations" et initialise toutes les entrées comme vides.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("regeneration du carnet dans Awake");
            Instance = this;
            visible = false;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Carnet créé et persistant");
        


        string oldFile = Path.Combine(Application.persistentDataPath, "GameData", "scenario_verites.json");

		if (!File.Exists(oldFile))
        {
            Debug.LogError($"Fichier vérités introuvable : {oldFile}");
            return;
        }

        string originalJson = File.ReadAllText(oldFile);
        JObject obj = JObject.Parse(originalJson);
		
		this.numScenario = obj["scenario"].ToString();

        // Récupérer le contenu de "verites" et le renommer "informations"
        var verites = obj["verites"];
        obj.Remove("verites");
        obj["informations"] = verites;
        
        // Initialiser toutes les réponses comme vides
        foreach (var service in (JObject)obj["informations"])
        {
            var serviceObj = (JObject)service.Value;
            var postesContainer = (JObject)serviceObj["postes"];

            foreach (var poste in postesContainer.Properties())
            {
                var posteObj = (JObject)poste.Value;
                var veritesObj = (JObject)posteObj["verites"];

                foreach (var question in veritesObj.Properties())
                {
                    // Remplacer le tableau d'entiers par un tableau vide
                    question.Value.Replace(new JArray());
                }
            }
        }

        // Sauvegarder dans un nouveau json
		string outputDirPath = Path.Combine(Application.persistentDataPath, "GameData");
        
        if (!Directory.Exists(outputDirPath))
        {
            Directory.CreateDirectory(outputDirPath);
        }
        
		this.pathFile = Path.Combine(outputDirPath, "carnet.json");

        File.WriteAllText(pathFile, obj.ToString());
        }

        if(scrollViewGameObject != null)
        {
            scrollViewGameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        
    }

    /// <summary>
    /// Ajoute une information au carnet pour un service, un métier et une question donnés.
    /// </summary>
    public void ajoutInfo(string service, string metier, string numQuestion, int numVar)
    {
        Debug.Log($"paramètre : {service}, {metier}, {numQuestion}, {numVar}");
        string _service = service.ToLower();
        string _metier = metier.ToLower();


        string json = File.ReadAllText(this.pathFile);
        JObject obj = JObject.Parse(json);
        Debug.Log($"|| pathfile {this.pathFile}||");
        Debug.Log("|| ajoutInfo : 1 ||");
        // Accéder à la liste
        JArray liste = (JArray)obj["informations"][_service]["postes"][_metier]["verites"][numQuestion];
        Debug.Log($"|| ajoutInfo : 2 size : {liste.Count.ToString()} ||");

        //Ajouter l'info
		if(!liste.Contains(new JValue(numVar))){
        	liste.Add(numVar);
		}

        //Sauvegarder
        File.WriteAllText(this.pathFile, obj.ToString());
    }

    /// <summary>
    /// Affiche le contenu du carnet sous forme textuelle pour tous les services et métiers.
    /// </summary>
    public string afficherCarnet()
    {
        Debug.Log("|| afficherCarnet : 1 ||");
        string json = File.ReadAllText(this.pathFile);
        JObject obj = JObject.Parse(json);

        var sb = new System.Text.StringBuilder();
        var services = ((JObject)obj["informations"]).Properties().OrderBy(s => s.Name);
        Debug.Log("|| afficherCarnet : 2 ||");
        foreach (var service in services)
        {
            string serviceName = service.Name;
            var metiers = ((JObject)service.Value["postes"]).Properties();
            var allQuestions = metiers
                .SelectMany(m => ((JObject)m.Value["verites"]).Properties().Select(q => q.Name))
                .Distinct()
                .OrderBy(q => int.Parse(q));

            bool serviceHasInfo = false;
            Debug.Log("|| afficherCarnet : 4 ||");
            foreach (var questionNum in allQuestions)
            {
                Debug.Log("|| afficherCarnet : 5 ||");
                string questionText = getQuestion(serviceName, questionNum);
                var questionLines = new List<string>();
                foreach (var metier in metiers.OrderBy(m => m.Name))
                {
                    var metierObj = (JObject)metier.Value["verites"];
                    if (metierObj.ContainsKey(questionNum))
                    {
                        Debug.Log("|| ici 1||");
                        var infos = (JArray)metierObj[questionNum];

                        for (int i = 0; i < infos.Count; i++)
                        {
                            Debug.Log("|| ici 2||");
                            Debug.Log($"paramètre getInfo : {serviceName}, {metier.Name}, {questionNum}, {infos[i].ToString()}         bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                            string infoText = getInfo(serviceName, metier.Name, questionNum, infos[i].ToString());
                            
                            if (!infoText.StartsWith("["))
                            {
                                questionLines.Add($"    - {metier.Name} → {infoText}");
                            }
                        }
                    }
                }

                if (questionLines.Count > 0)
                {
                    if (!serviceHasInfo)
                    {
                        sb.AppendLine($"Service : {serviceName}");
                        serviceHasInfo = true;
                    }

                    sb.AppendLine($"\n  {questionText}");
                    foreach (var line in questionLines)
                    {
                        sb.AppendLine(line);
                    }
                }
            }

            if (serviceHasInfo)
            {
                sb.AppendLine(); // saut de ligne entre services
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Récupère l’intitulé d’une question à partir du fichier scénario.
    /// </summary>
    public string getQuestion(string service, string numQuestion)
    {
        string file = Path.Combine(Application.streamingAssetsPath, $"scenario{this.numScenario}.json");
        string json = File.ReadAllText(file);
        JObject obj = JObject.Parse(json);

        string serviceAudite = obj["service_audite"].ToString().ToLower();
        string serviceKey = service.ToLower() == serviceAudite ? "service_audite" : "autres_services";

        int index = int.Parse(numQuestion);
        return obj["questions"][serviceKey]["liste"][index].ToString();
    }

    /// <summary>
    /// Récupère une info clef pour un service, un métier, une question et un numéro d’info.
    /// </summary>
    private string getInfo(string service, string metier, string numQuestion, string numInfo)
    {
		string file = Path.Combine(Application.streamingAssetsPath, $"scenario{this.numScenario}_{service.ToLower()}.json");
        string json = File.ReadAllText(file);
        JObject obj = JObject.Parse(json);

        int index = int.Parse(numInfo)-1;
        JArray infos = (JArray)obj["postes"][metier.ToLower()][numQuestion];
        if (index >= 0 && index < infos.Count)
        {
            return infos[index]["info_cle"]?.ToString() ?? $"[info_cle manquante]";
        }
        else
        {
            return $"[Info {numInfo} introuvable pour {metier} - question {numQuestion}]";
        }
    }

    /// <summary>
    /// Récupère les infos clefs d’une question dans le carnet.
    /// </summary>
    public List<string> getInfos(string serviceRapport, string numQuestion)
    {
        string json = File.ReadAllText(this.pathFile);
        JObject obj = JObject.Parse(json);
        List<string> lst = new List<string>();
		string service_audite = getServiceAudite();

		if(service_audite == serviceRapport){
			var postes = (JObject)obj["informations"][service_audite]["postes"];
			foreach (var metier in postes.Properties())
        	{
            	JObject verites = (JObject)metier.Value["verites"];

                JArray infos = (JArray) verites[numQuestion];

            	for (int i = 0; i < infos.Count; i++)
            	{
                	string infoText = getInfo(service_audite, metier.Name, numQuestion.ToString(), infos[i].ToString());
                	if (!infoText.StartsWith("["))
                	{
                    	lst.Add(infoText);
                	}
            	}
        	}
		}
		else{
            JObject informations = (JObject)obj["informations"];

            foreach (var service in informations)
            {
                if(service.Key == service_audite)
                {
                    continue;
                }

                JObject postes = (JObject)service.Value["postes"];

                foreach (var metier in postes.Properties())
                {
                    JObject verites = (JObject)metier.Value["verites"];

                    JArray infos = (JArray) verites[numQuestion];

                    for (int i = 0; i < infos.Count; i++)
                    {
                        string infoText = getInfo(service.Key, metier.Name, numQuestion, infos[i].ToString());
                        if (!infoText.StartsWith("["))
                        {
                            lst.Add(infoText);
                        }
                    }
                }
            }
		}
      
        return lst;
    }

    /// <summary>
    /// Retourne toutes les infos clefs présentes dans le carnet.
    /// </summary>
    public List<string> getAllInfos()
    {
        List<string> lst = new List<string>();
        
        string json = File.ReadAllText(this.pathFile);
        JObject obj = JObject.Parse(json);

        foreach (var service in ((JObject)obj["informations"]).Properties())
        {
            var postes = (JObject)service.Value;
            foreach (var metier in postes.Properties())
            {
                var verites = (JObject)metier.Value;
                foreach (var question in verites.Properties())
                {
                    JArray infos = (JArray) question.Value;
                    for (int i = 0; i < infos.Count; i++)
                    {
                        string infoText = getInfo(service.Name, metier.Name, question.Name, infos[i].ToString());
                        if (!infoText.StartsWith("["))
                        {
                            lst.Add(infoText);
                        }
                    }
                }
            }
        }
		return lst;
    }

    /// <summary>
    /// Récupère le service audité pour le scénario en cours.
    /// </summary>
	public string getServiceAudite(){
		string file = Path.Combine(Application.streamingAssetsPath, $"scenario{this.numScenario}.json");
        string json = File.ReadAllText(file);
        JObject obj = JObject.Parse(json);

        return obj["service_audite"].ToString().ToLower();
	}

    /// <summary>
    /// Récupère le titre du scénario en cours.
    /// </summary>
	public string getNameAudit(){
		string file = Path.Combine(Application.streamingAssetsPath, $"scenario{this.numScenario}.json");
        string json = File.ReadAllText(file);
        JObject obj = JObject.Parse(json);

		return obj["titre"].ToString();
	}

    /// <summary>
    /// Récupère toutes les questions du carnet sous forme de dictionnaire (clé : service;numQuestion, valeur : texte).
    /// </summary>
    public Dictionary<string, string> getAllQuestions()
	{
        Dictionary<string, string> dico = new Dictionary<string, string>();

    	string json = File.ReadAllText(this.pathFile);
    	JObject obj = JObject.Parse(json);

    	JObject informations = (JObject)obj["informations"];

    	foreach (var service in informations)
    	{
        	JObject postes = (JObject)service.Value["postes"];

        	foreach (var poste in postes)
        	{
            	JObject verites = (JObject)poste.Value["verites"];

            	foreach (var numQuestion in verites)
            	{
					string key = $"{service.Key};{numQuestion.Key}";
					string question = getQuestion(service.Key, numQuestion.Key);
					if(service.Key != getServiceAudite()){
						bool trouver = false;

						foreach(KeyValuePair<string, string> questions in dico){
							if(questions.Value == question){
								trouver = true;
								break;
							}
						}

						if(!trouver){
							key = $"autres_services;{numQuestion.Key}";
							dico.Add(key, question);
						}
					}
					else{
						if(!dico.ContainsKey(key)){
							dico.Add(key, question);
						}
					}
            	}
        	}
    	}

    	return dico;
	}

    /// <summary>
    /// Récupère la liste des services présents dans le carnet.
    /// </summary>
    public List<string> getServices()
    {
        List<string> lst = new List<string>();
        
        string json = File.ReadAllText(this.pathFile);
    	JObject obj = JObject.Parse(json);

    	JObject informations = (JObject)obj["informations"];

        foreach(var service in informations)
        {
            lst.Add(service.Key);
        }

        return lst;
    }

    /// <summary>
    /// Retourne le numéro de l'information correspondant à un texte donné dans un service.
    /// </summary>
    public string getNumInfo(string service, string info)
    {
        string file = Path.Combine(Application.streamingAssetsPath, $"scenario{this.numScenario}_{service.ToLower()}.json");
        string json = File.ReadAllText(file);
        JObject obj = JObject.Parse(json);

        JObject postes = (JObject)obj["postes"];

        foreach(var metier in postes)
        {
            JObject questions = (JObject)metier.Value;
            foreach(var question in questions)
            {
                JArray reponse = (JArray)question.Value;

                for(int i=0; i<reponse.Count; i++)
                {
                    JObject rep = (JObject)reponse[i];
                    if (string.Equals(rep["info_cle"].ToString().Trim(), info.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return metier.Key + ";" + reponse[i]["variation_id"];
                    }
                }
            }
        }
        return "-1";
    }

    public void RegenererCarnet()
    {
        Debug.Log("||||||||||||||||  Régénération du carnet pour nouveau niveau  |||||||||||||||||||||||");
        ConstruireCarnetDepuisVerites();
    }

    public void ConstruireCarnetDepuisVerites()
    {
        string veritesFile = Path.Combine(Application.persistentDataPath, "GameData", "scenario_verites.json");

		if (!File.Exists(veritesFile))
        {
            Debug.LogError($"Fichier vérités introuvable : {veritesFile}");
            return;
        }

        string originalJson = File.ReadAllText(veritesFile);
        JObject obj = JObject.Parse(originalJson);
		
		this.numScenario = obj["scenario"].ToString();

        // Récupérer le contenu de "verites"
        var verites = obj["verites"];

        // Supprimer l'ancienne clé
        obj.Remove("verites");

        // Ajouter sous le nouveau nom
        obj["informations"] = verites;
        
        // Parcourir chaque service
        foreach (var service in (JObject)obj["informations"])
        {
            var serviceObj = (JObject)service.Value;
            var postesContainer = (JObject)serviceObj["postes"];

            foreach (var poste in postesContainer.Properties())
            {
                var posteObj = (JObject)poste.Value;
                var veritesObj = (JObject)posteObj["verites"];

                // Parcourir chaque question ("0","1",...)
                foreach (var question in veritesObj.Properties())
                {
                    // Remplacer le tableau d'entiers par un tableau vide
                    question.Value.Replace(new JArray());
                }
            }
        }

        // Sauvegarder dans un nouveau json
		string outputDirPath = Path.Combine(Application.persistentDataPath, "GameData");
        
        if (!Directory.Exists(outputDirPath))
        {
            Directory.CreateDirectory(outputDirPath);
        }
        
		this.pathFile = Path.Combine(outputDirPath, "carnet.json");

        File.WriteAllText(pathFile, obj.ToString());
        
    }


}
