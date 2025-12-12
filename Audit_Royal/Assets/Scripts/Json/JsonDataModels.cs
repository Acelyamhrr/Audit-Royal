using System;
using System.Collections.Generic;
using UnityEngine; 

[Serializable]
public class DialogueVariation
{
    // Correspond à l'objet interne (un dialogue)
    public string normal;
    public string colere;
    public string anxieux;
    public string menteur;
    public string balance;
    public string info_cle;
    public int variation_id;
}

[Serializable]
public class ServiceData
{
    public string service;
    public Dictionary<string, Dictionary<string, List<DialogueVariation>>> postes; 
}


[Serializable]
public class VeritesByPoste
{
    // clé / valeur
    public Dictionary<string, List<int>> verites; 
}

[Serializable]
public class VeritesByService
{
    // ("secrétaire", "patron")
    public Dictionary<string, VeritesByPoste> postes; 
}

[Serializable]
public class VeritesScenarioRoot
{
    public int scenario;
    public int niveau;
    public Dictionary<string, VeritesByService> verites; 
}

[Serializable]
public class PlayerData
{
    public string nom;
    public string prenom;
    public string service;
    public string metier;
    public string caractere;  
    public double taux;
}

[Serializable]
public class QuestionBloc
{
    public string description;
    public List<string> liste;
}

[Serializable]
public class ScenarioQuestions
{
    public QuestionBloc service_audite;
    public QuestionBloc autres_services;
}

[Serializable]
public class ScenarioRoot
{
    public int scenario;
    public string titre;
    public string theme;
    public string service_audite;
    public string problematique;
    public ScenarioQuestions questions;
}

public class PosteInfo
    {
        public string nomPoste;
        public string fichierJson;
        
        public PosteInfo(string nom, string fichier)
        {
            nomPoste = nom;
            fichierJson = fichier;
        }
    }