using System;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;

public class RapportManager
{
    private int nbInfosVraies;          //Nombre total d'infos vraies
    public RapportManager(string pathFile)
    {
        nbInfosVraies = 0;
        
        //Récupérer nombre d'infos vraies dans le json des verites
        string json = File.ReadAllText(pathFile);
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
    }

    //Renvoie le nombre de bonnes réponses
    private int checkColumnTrue()
    {
        return -1;
    }
}