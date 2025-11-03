using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

public class CarnetManager
{
    private string pathFile;
    private string numScenario;
    public CarnetManager(string newFile, string oldFile)
    {
        pathFile = newFile;

        // Charger le JSON existant (celui du scénario)
        string originalJson = File.ReadAllText(oldFile);
        JObject obj = JObject.Parse(originalJson);

        //Récupérer numéro du scénario courant
        numScenario = obj["scenario"].ToString();

        // Renommer "verites" en "informations"
        var verites = obj["verites"];
        obj.Remove("verites");
        obj["informations"] = verites;

        // Parcourir chaque service
        foreach (var service in (JObject)obj["informations"])
        {
            var serviceObj = (JObject)service.Value;
            var postesContainer = (JObject)serviceObj["postes"];

            // Vider les listes (infos)
            foreach (var poste in postesContainer.Properties())
            {
                var niveaux = (JObject)poste.Value;
                foreach (var niveau in niveaux.Properties())
                {
                    niveau.Value.Replace(new JArray());
                }
            }

            // Remplacer le contenu du service par les rôles directement (enlever "postes")
            service.Value.Replace(postesContainer);
        }

        // Sauvegarder dans un nouveau json
        File.WriteAllText(pathFile, obj.ToString());
    }

    //Méthode pour ajouter entrée au carnet
    public void ajoutInfo(Service service, Metier metier, string numQuestion, int numVar)
    {
        string _service = service.ToString().ToLower();
        string _metier = metier.ToString().ToLower();


        string json = File.ReadAllText(this.pathFile);
        JObject obj = JObject.Parse(json);

        // Accéder à la liste
        JArray liste = (JArray)obj["informations"][_service][_metier][numQuestion];

        //Ajouter l'info
		if(!liste.Contains(new JValue(numVar)){
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
    private string getQuestion(string service, string numQuestion)
    {
        string file = $"scenario{this.numScenario}/scenario{this.numScenario}.json";
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

}