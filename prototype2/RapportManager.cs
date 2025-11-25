using System;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;

public class RapportManager
{
    private Object ColomnTrue;
    private Object ColomnFalse;
    private Object ListInfos;
    
    private int nbInfosVraies;          //Nombre total d'infos vraies
    private string fileTrue;
    
    private CarnetManager carnetManager;
    
    public RapportManager(string pathFile, CarnetManager carnet)
    {
        this.nbInfosVraies = 0;
        this.fileTrue = pathFile;
        this.carnetManager = carnet;
        
        //Récupérer nombre d'infos vraies dans le json des verites
        string json = File.ReadAllText(this.fileTrue);
        JObject obj = JObject.Parse(json);

        foreach (var service in (JObject)obj["verites"])
        {
            var serviceObj = (JObject)service.Value;
            var postesContainer = (JObject)serviceObj["postes"];
            
            foreach (var poste in postesContainer.Properties())
            {
                var questions = (JObject)poste.Value;
                foreach (var question in questions.Properties())
                {
                    nbInfosVraies += question.Value.Count;
                }
            }
        }
        
        //Afficher toutes les infos récupérées dans la liste
        
    }

    private List<string> getInfos(int numQuestion)
    {
        return carnetManager.getInfos(numQuestion);
    }

    //Renvoie si la réponse est vraie ou fausse
    private bool checkTrue(Service service, Metier metier, int numQuestion, int numInfo)
    {
        string json = File.ReadAllText(this.fileTrue);
        JObject obj = JObject.Parse(json);

        string _service = service.ToString().ToLower();
        string _metier = metier.ToString().ToLower();
        
        JArray liste = obj["verites"][_service][_metier][numQuestion];
        
        return liste.Contains(new JValue(numInfo));
    }
}