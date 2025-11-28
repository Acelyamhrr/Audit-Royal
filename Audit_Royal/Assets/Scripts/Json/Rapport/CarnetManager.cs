using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

public class CarnetManager : MonoBehaviour
{
    private string pathFile;
    private string numScenario;
    
    void Awake()
    {
        string newFile = Path.Combine(Application.streamingAssetsPath, "JSON/carnet.json");
        string oldFile = Path.Combine(Application.streamingAssetsPath, "JSON/scenario_verites.json");
        
        pathFile = newFile;

        string originalJson = File.ReadAllText(oldFile);
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
        File.WriteAllText(pathFile, obj.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Méthode pour ajouter entrée au carnet
    public void ajoutInfo(Service service, Metier metier, string numQuestion, int numVar)
    {
        string _service = service.ToString().ToLower();
        string _metier = metier.ToString().ToLower();


        string json = File.ReadAllText(this.pathFile);
        JObject obj = JObject.Parse(json);

        // Accéder à la liste
        JArray liste = (JArray)obj["informations"][_service]["postes"][_metier]["verites"][numQuestion];

        //Ajouter l'info
		if(!liste.Contains(new JValue(numVar))){
        	liste.Add(numVar);
		}

        //Sauvegarder
        File.WriteAllText(this.pathFile, obj.ToString());
    }

    //Méthode pour afficher le carnet (donne les informations reçues)
    public string afficherCarnet()
    {
        string json = File.ReadAllText(this.pathFile);
        JObject obj = JObject.Parse(json);

        var sb = new System.Text.StringBuilder();
        var services = ((JObject)obj["informations"]).Properties().OrderBy(s => s.Name);

        foreach (var service in services)
        {
            string serviceName = service.Name;
            var metiers = ((JObject)service.Value).Properties();

            // Collecter toutes les questions utilisées
            var allQuestions = metiers
                .SelectMany(m => ((JObject)m.Value).Properties().Select(q => q.Name))
                .Distinct()
                .OrderBy(q => int.Parse(q));

            bool serviceHasInfo = false;

            foreach (var questionNum in allQuestions)
            {
                string questionText = getQuestion(serviceName, questionNum);
                var questionLines = new List<string>();

                foreach (var metier in metiers.OrderBy(m => m.Name))
                {
                    var metierObj = (JObject)metier.Value;
                    if (metierObj.ContainsKey(questionNum))
                    {
                        var infos = (JArray)metierObj[questionNum];
                        for (int i = 0; i < infos.Count; i++)
                        {
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
                        sb.AppendLine($"📂 Service : {serviceName}");
                        serviceHasInfo = true;
                    }

                    sb.AppendLine($"\n  ❓ {questionText}");
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

    //Récupère l'intitulé de la question à partir du service, du numéro de la question et du numéro du scénario en cours
    public string getQuestion(string service, string numQuestion)
    {
        string file = Path.Combine(Application.streamingAssetsPath, $"scenario{this.numScenario}.json") ;
        string json = File.ReadAllText(file);
        JObject obj = JObject.Parse(json);

        string serviceAudite = obj["service_audite"].ToString().ToLower();
        string serviceKey = service.ToLower() == serviceAudite ? "service_" + service.ToLower() : "autres_services";

        int index = int.Parse(numQuestion);

        return obj["questions"][serviceKey]["liste"][index].ToString();
    }

    //Récupère l'info clef à partir du service, du métier, du numéro de la question et de l'info et du numéro du scénario en cours
    private string getInfo(string service, string metier, string numQuestion, string numInfo)
    {
        string file = $"scenario{this.numScenario}/scenario{this.numScenario}_{service.ToLower()}.json";
        string json = File.ReadAllText(file);
        JObject obj = JObject.Parse(json);

        int index = int.Parse(numInfo)-1;

        var infos = (JArray)obj["postes"][metier.ToLower()][numQuestion];
        if (index >= 0 && index < infos.Count)
        {
            return infos[index]["info_cle"]?.ToString() ?? $"[info_cle manquante]";
        }
        else
        {
            return $"[Info {numInfo} introuvable pour {metier} - question {numQuestion}]";
        }
    }

    //Récupère les infos clefs d'une question dans le carnet
    public List<string> getInfos(int numQuestion)
    {
        string json = File.ReadAllText(this.pathFile);
        JObject obj = JObject.Parse(json);
        List<string> lst = new List<string>();

        foreach (var service in (JObject)obj["informations"])
        {
            var postes = (JObject)service.Value;

            foreach (var metier in postes.Properties())
            {
                var verites = (JObject)metier.Value;
                var infos = (JArray)verites[numQuestion.ToString()];

                for (int i = 0; i < infos.Count; i++)
                {
                    string infoText = getInfo(service.Key, metier.Name, numQuestion.ToString(), infos[i].ToString());
                    if (!infoText.StartsWith("["))
                    {
                        lst.Add(infoText);
                    }
                }
            }
        }

        return lst;
    }

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

    public List<string> getAllQuestions()
	{
    	List<string> questions = new List<string>();

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
					string question = getQuestion(service.Key, numQuestion.Key);
					if(!questions.Contains(question)){
						questions.Add(question);
					}
            	}
        	}
    	}

    	return questions;
	}


}
